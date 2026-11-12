#!/usr/bin/env bash
# End-to-end flow of the Profiles context against a running API.
#
#   ./scripts/profiles-flow.sh                 # http://localhost:5240
#   API_URL=http://host:port ./scripts/profiles-flow.sh
#
# Steps: caregiver sign-up and sign-in, caregiver profile, older adult, invitation, public lookup by code,
# family member sign-up and sign-in, family member profile and acceptance. Then a few read and permission checks.
# Each run uses new emails, so it can be repeated. Requires bash, curl and jq. Stops at the first unexpected status.
set -euo pipefail

API="${API_URL:-http://localhost:5240}/api/v1"
RUN="$(date +%s)"
CAREGIVER_EMAIL="caregiver.${RUN}@vitalita.test"
FAMILY_EMAIL="family.${RUN}@vitalita.test"
PASSWORD="Vitalita123"

STEP=0
BODY=""

# call <expected-status> <title> <method> <path> [json-body] [token]
call() {
  local expected="$1" title="$2" method="$3" path="$4" data="${5:-}" token="${6:-}"
  local args=(-s -o /tmp/profiles-flow-body.$$ -w '%{http_code}' -X "$method" "$API$path"
    -H 'Accept: application/json' -H 'Accept-Language: es')
  [[ -n "$data" ]] && args+=(-H 'Content-Type: application/json' -d "$data")
  [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
  local status
  status="$(curl "${args[@]}")"
  BODY="$(cat /tmp/profiles-flow-body.$$)"
  rm -f /tmp/profiles-flow-body.$$
  STEP=$((STEP + 1))
  printf '\n\033[1m%02d. %s\033[0m\n    %s %s -> %s\n' "$STEP" "$title" "$method" "$path" "$status"
  if [[ -n "$BODY" ]]; then
    jq '(.. | .token? | strings) |= (.[0:16] + "...")' <<<"$BODY" 2>/dev/null | sed 's/^/    /' || echo "    $BODY"
  fi
  if [[ "$status" != "$expected" ]]; then
    printf '\n\033[31mFAILED: expected %s, got %s\033[0m\n' "$expected" "$status"
    exit 1
  fi
}

echo "API: $API"

# 1-2. Caregiver account
call 201 "Caregiver sign-up" POST /authentication/sign-up \
  "{\"email\":\"$CAREGIVER_EMAIL\",\"password\":\"$PASSWORD\",\"role\":\"Caregiver\"}"
CAREGIVER_USER_ID="$(jq -r .id <<<"$BODY")"
call 200 "Caregiver sign-in" POST /authentication/sign-in \
  "{\"email\":\"$CAREGIVER_EMAIL\",\"password\":\"$PASSWORD\"}"
CAREGIVER_TOKEN="$(jq -r .token <<<"$BODY")"

# 3. Caregiver profile (same body as sign-up-form.vue)
call 201 "Caregiver profile" POST /caregiver-profiles \
  "{\"userId\":$CAREGIVER_USER_ID,\"firstName\":\"Andrea\",\"lastName\":\"Mendoza\",\"phoneNumber\":\"\",\"professionalTitle\":\"\",\"bio\":\"\",\"photoUrl\":\"\",\"certifications\":[]}" \
  "$CAREGIVER_TOKEN"
CAREGIVER_PROFILE_ID="$(jq -r .id <<<"$BODY")"

# 4. Older adult (same body as older-adult-form.vue)
call 201 "Older adult" POST /older-adults \
  "{\"caregiverId\":$CAREGIVER_PROFILE_ID,\"firstName\":\"María\",\"lastName\":\"Rodríguez\",\"birthDate\":\"1948-03-12\",\"documentNumber\":\"40218463\",\"bloodType\":\"O+\",\"allergies\":[\"Penicilina\"],\"chronicConditions\":[\"Hipertensión arterial\",\"Diabetes tipo 2\"],\"emergencyContact\":{\"name\":\"Renzo Rodríguez\",\"phoneNumber\":\"987111222\",\"relationship\":\"Son\"},\"observations\":\"Requiere asistencia para caminar distancias largas.\",\"status\":\"Active\"}" \
  "$CAREGIVER_TOKEN"
OLDER_ADULT_ID="$(jq -r .id <<<"$BODY")"

# 5. Invitation (the frontend also sends its own code and dates; the server replaces them)
call 201 "Family invitation" POST /family-access \
  "{\"olderAdultId\":$OLDER_ADULT_ID,\"invitedEmail\":\"$FAMILY_EMAIL\",\"invitationCode\":\"VITA-AAAA\",\"relationship\":\"Daughter\",\"status\":\"Active\",\"invitedAt\":\"2020-01-01T00:00:00Z\",\"expiresAt\":\"2020-01-08T00:00:00Z\"}" \
  "$CAREGIVER_TOKEN"
INVITATION_CODE="$(jq -r .invitationCode <<<"$BODY")"

# 6. Public lookup (no token), in lowercase as a user could type it
CODE_LOWER="$(tr '[:upper:]' '[:lower:]' <<<"$INVITATION_CODE")"
call 200 "Public invitation lookup (no token)" GET "/family-access?invitationCode=$CODE_LOWER"
INVITATION="$(jq '.[0]' <<<"$BODY")"
ACCESS_ID="$(jq -r .id <<<"$INVITATION")"

# 7-8. Family member account with the invited email
call 201 "Family member sign-up" POST /authentication/sign-up \
  "{\"email\":\"$FAMILY_EMAIL\",\"password\":\"$PASSWORD\",\"role\":\"FamilyMember\"}"
FAMILY_USER_ID="$(jq -r .id <<<"$BODY")"
call 200 "Family member sign-in" POST /authentication/sign-in \
  "{\"email\":\"$FAMILY_EMAIL\",\"password\":\"$PASSWORD\"}"
FAMILY_TOKEN="$(jq -r .token <<<"$BODY")"

# 9. Family member profile
call 201 "Family member profile" POST /family-member-profiles \
  "{\"userId\":$FAMILY_USER_ID,\"firstName\":\"Carla\",\"lastName\":\"Rodríguez\",\"phoneNumber\":\"945222333\"}" \
  "$FAMILY_TOKEN"
FAMILY_PROFILE_ID="$(jq -r .id <<<"$BODY")"

# 10. Acceptance: PUT with the public invitation + familyMemberId + Active (as registerFamilyMember does)
ACCEPTED="$(jq -c --argjson fm "$FAMILY_PROFILE_ID" \
  '. + {familyMemberId: $fm, status: "Active", acceptedAt: (now | todate)}' <<<"$INVITATION")"
call 200 "Accept invitation" PUT "/family-access/$ACCESS_ID" "$ACCEPTED" "$FAMILY_TOKEN"

# Checks: what the family member and the caregiver can read now
call 200 "Family: active accesses" GET "/family-access?familyMemberId=$FAMILY_PROFILE_ID&status=Active" "" \
  "$FAMILY_TOKEN"
call 200 "Family: older adults by id" GET "/older-adults?id=$OLDER_ADULT_ID&id=999999" "" "$FAMILY_TOKEN"
call 200 "Family: responsible caregiver" GET "/caregiver-profiles/$CAREGIVER_PROFILE_ID" "" "$FAMILY_TOKEN"
call 200 "Caregiver: family member names" GET "/family-member-profiles?id=$FAMILY_PROFILE_ID" "" \
  "$CAREGIVER_TOKEN"
call 403 "Family cannot edit the older adult" PUT "/older-adults/$OLDER_ADULT_ID" \
  "{\"caregiverId\":$CAREGIVER_PROFILE_ID,\"firstName\":\"X\",\"lastName\":\"Y\",\"birthDate\":\"1948-03-12\",\"status\":\"Active\"}" \
  "$FAMILY_TOKEN"
call 200 "Public lookup after acceptance returns []" GET "/family-access?invitationCode=$INVITATION_CODE"

printf '\n\033[32mOK: %d steps\033[0m\n' "$STEP"
