#!/usr/bin/env bash
# End-to-end flow of the Monitoring context against a running API.
#
#   ./scripts/monitoring-flow.sh                 # http://localhost:5240
#   API_URL=http://host:port ./scripts/monitoring-flow.sh
#
# Setup (Profiles): caregiver with an older adult, and a family member with an active access.
# Monitoring: daily report, vital signs, medication, reads as caregiver and family member, deactivation, and the
# permission and validation checks. Each run uses new emails, so it can be repeated. Requires bash, curl and jq.
# Stops at the first unexpected status.
set -euo pipefail

API="${API_URL:-http://localhost:5240}/api/v1"
RUN="$(date +%s)"
PASSWORD="Vitalita123"
NOW="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
FUTURE="$(date -u -v+1d +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -d '+1 day' +%Y-%m-%dT%H:%M:%SZ)"

STEP=0
BODY=""
QUIET=0

# call <expected-status> <title> <method> <path> [json-body] [token]
call() {
  local expected="$1" title="$2" method="$3" path="$4" data="${5:-}" token="${6:-}"
  local out
  out="$(mktemp)"
  local args=(-s -o "$out" -w '%{http_code}' -X "$method" "$API$path"
    -H 'Accept: application/json' -H 'Accept-Language: es')
  [[ -n "$data" ]] && args+=(-H 'Content-Type: application/json' -d "$data")
  [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
  local status
  status="$(curl "${args[@]}")"
  BODY="$(cat "$out")"
  rm -f "$out"
  STEP=$((STEP + 1))
  printf '\n\033[1m%02d. %s\033[0m\n    %s %s -> %s\n' "$STEP" "$title" "$method" "$path" "$status"
  if [[ "$QUIET" == 0 && -n "$BODY" ]]; then
    jq '(.. | .token? | strings) |= (.[0:16] + "...") | if type == "object" and has("status") and has("title") then del(.traceId, .type) else . end' \
      <<<"$BODY" 2>/dev/null | sed 's/^/    /' || echo "    $BODY"
  fi
  if [[ "$status" != "$expected" ]]; then
    printf '\n\033[31mFAILED: expected %s, got %s\033[0m\n%s\n' "$expected" "$status" "$BODY"
    exit 1
  fi
}

# account <role> <email> -> sets USER_ID and TOKEN
account() {
  call 201 "Sign-up $1" POST /authentication/sign-up "{\"email\":\"$2\",\"password\":\"$PASSWORD\",\"role\":\"$1\"}"
  USER_ID="$(jq -r .id <<<"$BODY")"
  call 200 "Sign-in $1" POST /authentication/sign-in "{\"email\":\"$2\",\"password\":\"$PASSWORD\"}"
  TOKEN="$(jq -r .token <<<"$BODY")"
}

echo "API: $API"
echo
echo "=== Setup (Profiles): caregiver, older adult, family member with an active access, outsider caregiver"
QUIET=1
account Caregiver "mon.caregiver.${RUN}@vitalita.test"
CAREGIVER_TOKEN="$TOKEN"
call 201 "Caregiver profile" POST /caregiver-profiles \
  "{\"userId\":$USER_ID,\"firstName\":\"Andrea\",\"lastName\":\"Mendoza\"}" "$CAREGIVER_TOKEN"
CAREGIVER_PROFILE_ID="$(jq -r .id <<<"$BODY")"
call 201 "Older adult" POST /older-adults \
  "{\"caregiverId\":$CAREGIVER_PROFILE_ID,\"firstName\":\"María\",\"lastName\":\"Rodríguez\",\"birthDate\":\"1948-03-12\"}" \
  "$CAREGIVER_TOKEN"
OLDER_ADULT_ID="$(jq -r .id <<<"$BODY")"
call 201 "Second older adult (for the 409 check)" POST /older-adults \
  "{\"caregiverId\":$CAREGIVER_PROFILE_ID,\"firstName\":\"Jorge\",\"lastName\":\"Salazar\",\"birthDate\":\"1941-07-25\"}" \
  "$CAREGIVER_TOKEN"
OTHER_OLDER_ADULT_ID="$(jq -r .id <<<"$BODY")"
FAMILY_EMAIL="mon.family.${RUN}@vitalita.test"
call 201 "Invitation" POST /family-access \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"invitedEmail\":\"$FAMILY_EMAIL\",\"relationship\":\"Son\"}" "$CAREGIVER_TOKEN"
ACCESS_ID="$(jq -r .id <<<"$BODY")"
account FamilyMember "$FAMILY_EMAIL"
FAMILY_TOKEN="$TOKEN"
call 201 "Family member profile" POST /family-member-profiles \
  "{\"userId\":$USER_ID,\"firstName\":\"Renzo\",\"lastName\":\"Rodríguez\"}" "$FAMILY_TOKEN"
FAMILY_PROFILE_ID="$(jq -r .id <<<"$BODY")"
call 200 "Accept invitation" PUT "/family-access/$ACCESS_ID" \
  "{\"status\":\"Active\",\"familyMemberId\":$FAMILY_PROFILE_ID}" "$FAMILY_TOKEN"
account Caregiver "mon.outsider.${RUN}@vitalita.test"
OUTSIDER_TOKEN="$TOKEN"
QUIET=0

echo
echo "=== Monitoring as the caregiver in charge"
call 201 "Daily report (recordedByUserId in the body is ignored)" POST /daily-reports \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"recordedByUserId\":999,\"reportDate\":\"$NOW\",\"mood\":\"Good\",\"generalCondition\":\"Stable\",\"observations\":\"Durmió bien y almorzó completo.\"}" \
  "$CAREGIVER_TOKEN"
REPORT_ID="$(jq -r .id <<<"$BODY")"
call 201 "Vital sign: heart rate" POST /vital-signs \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"type\":\"HeartRate\",\"value\":72,\"secondaryValue\":null,\"unit\":\"bpm\",\"measuredAt\":\"$NOW\"}" \
  "$CAREGIVER_TOKEN"
call 201 "Vital sign: blood pressure" POST /vital-signs \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"type\":\"BloodPressure\",\"value\":120,\"secondaryValue\":80,\"measuredAt\":\"$NOW\"}" \
  "$CAREGIVER_TOKEN"
call 201 "Vital sign: temperature (unit derived from the type)" POST /vital-signs \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"type\":\"Temperature\",\"value\":36.6,\"unit\":\"F\",\"measuredAt\":\"$NOW\"}" \
  "$CAREGIVER_TOKEN"
call 201 "Medication (times sorted, duplicates removed)" POST /medications \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"name\":\"Metformina\",\"dosage\":\"850 mg\",\"frequency\":\"Cada 12 horas\",\"scheduleTimes\":[\"20:00\",\"08:00\",\"08:00\"],\"instructions\":\"Tomar después del desayuno y la cena.\"}" \
  "$CAREGIVER_TOKEN"
MEDICATION="$BODY"
MEDICATION_ID="$(jq -r .id <<<"$BODY")"

echo
echo "=== Reads"
call 200 "Caregiver: daily reports" GET "/daily-reports?olderAdultId=$OLDER_ADULT_ID" "" "$CAREGIVER_TOKEN"
call 200 "Family member (active access): vital signs" GET "/vital-signs?olderAdultId=$OLDER_ADULT_ID" "" \
  "$FAMILY_TOKEN"
call 200 "Family member: medications" GET "/medications?olderAdultId=$OLDER_ADULT_ID" "" "$FAMILY_TOKEN"
call 200 "Family member: daily report by id" GET "/daily-reports/$REPORT_ID" "" "$FAMILY_TOKEN"

echo
echo "=== Deactivate the medication (PUT active=false)"
call 200 "Deactivate medication" PUT "/medications/$MEDICATION_ID" \
  "$(jq -c '.active = false' <<<"$MEDICATION")" "$CAREGIVER_TOKEN"
call 200 "Active medications only" GET "/medications?olderAdultId=$OLDER_ADULT_ID&active=true" "" "$CAREGIVER_TOKEN"

echo
echo "=== Permission and validation checks"
call 403 "Family member cannot write" POST /daily-reports \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"reportDate\":\"$NOW\",\"mood\":\"Good\",\"generalCondition\":\"Stable\",\"observations\":\"x\"}" \
  "$FAMILY_TOKEN"
call 403 "Another caregiver cannot read" GET "/vital-signs?olderAdultId=$OLDER_ADULT_ID" "" "$OUTSIDER_TOKEN"
call 403 "Unknown older adult is 403, not 404" GET "/daily-reports?olderAdultId=999999" "" "$CAREGIVER_TOKEN"
call 403 "List without filters is admin-only" GET /medications "" "$CAREGIVER_TOKEN"
call 404 "Unknown record" GET /vital-signs/999999 "" "$CAREGIVER_TOKEN"
call 400 "Heart rate out of range" POST /vital-signs \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"type\":\"HeartRate\",\"value\":300,\"measuredAt\":\"$NOW\"}" "$CAREGIVER_TOKEN"
call 400 "Blood pressure without diastolic" POST /vital-signs \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"type\":\"BloodPressure\",\"value\":120,\"measuredAt\":\"$NOW\"}" \
  "$CAREGIVER_TOKEN"
call 400 "Report in the future" POST /daily-reports \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"reportDate\":\"$FUTURE\",\"mood\":\"Good\",\"generalCondition\":\"Stable\",\"observations\":\"x\"}" \
  "$CAREGIVER_TOKEN"
call 400 "Invalid medication data" POST /medications \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"name\":\"\",\"dosage\":\"10 mg\",\"scheduleTimes\":[\"8:00\"]}" "$CAREGIVER_TOKEN"
call 409 "Moving a report to another older adult" PUT "/daily-reports/$REPORT_ID" \
  "{\"olderAdultId\":$OTHER_OLDER_ADULT_ID,\"reportDate\":\"$NOW\",\"mood\":\"Bad\",\"generalCondition\":\"Attention\",\"observations\":\"x\"}" \
  "$CAREGIVER_TOKEN"
call 401 "No token" GET "/medications?olderAdultId=$OLDER_ADULT_ID"

printf '\n\033[32mOK: %d steps\033[0m\n' "$STEP"
