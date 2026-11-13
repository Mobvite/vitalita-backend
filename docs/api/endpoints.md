# Vitalita API – Endpoint reference (Sprint 3)

- Base URL (local): `http://localhost:5240/api/v1`
- OpenAPI document: `/swagger/v1/swagger.json` · Swagger UI: `/swagger`
- Deployed URL: _(to be completed when the deployment is published)_

**Conventions**

- Authentication: `Authorization: Bearer <token>` (JWT, 15 minutes). Endpoints marked **Public** do not need it.
- JSON in camelCase, enums as strings, UTC instants ending in `Z`, date-only fields as `yyyy-MM-dd`.
- Lists are plain arrays. Filters are query parameters; several ids are sent as `?id=1&id=2`.
- Errors use `application/problem+json` with a `message` field. Language: `Accept-Language: es` or `en`.
- Access rules: the **owning caregiver** (or an Admin) can write; a **family member with an active access** can only read.

## 1. Summary

| # | Method | Route | Access | User stories | Success |
|---|---|---|---|---|---|
| 1 | POST | `/authentication/sign-up` | Public | US05, US31 | 201 |
| 2 | POST | `/authentication/sign-in` | Public | US06 | 200 |
| 3 | GET | `/authentication/me` | Bearer | US06 | 200 |
| 4 | POST | `/authentication/refresh` | Bearer | US06 | 200 |
| 5 | GET | `/users` | Public with `?email=`; Admin without it | US06 | 200 |
| 6 | POST | `/users` | Admin | – (administration) | 201 |
| 7 | GET | `/users/{id}` | Bearer (self or Admin) | – (administration) | 200 |
| 8 | PUT | `/users/{id}` | Bearer (self or Admin) | – (administration) | 200 |
| 9 | DELETE | `/users/{id}` | Admin | – (administration) | 204 |
| 10 | GET | `/caregiver-profiles?userId=` | Bearer (own profile) | US35 | 200 |
| 11 | GET | `/caregiver-profiles/{id}` | Bearer (owner, or family with active access) | US35 | 200 |
| 12 | POST | `/caregiver-profiles` | Bearer (Caregiver) | US05 | 201 |
| 13 | PUT | `/caregiver-profiles/{id}` | Bearer (owner) | US35 | 200 |
| 14 | GET | `/family-member-profiles?userId=` / `?id=` | Bearer | US31 | 200 |
| 15 | GET | `/family-member-profiles/{id}` | Bearer | US31 | 200 |
| 16 | POST | `/family-member-profiles` | Bearer (FamilyMember with a pending invitation) | US31 | 201 |
| 17 | PUT | `/family-member-profiles/{id}` | Bearer (owner) | US31 | 200 |
| 18 | GET | `/older-adults?caregiverId=` / `?id=` | Bearer | US08, US10 | 200 |
| 19 | GET | `/older-adults/{id}` | Bearer (owning caregiver, or family with active access) | US08 | 200 |
| 20 | POST | `/older-adults` | Bearer (Caregiver) | US07 | 201 |
| 21 | PUT | `/older-adults/{id}` | Bearer (owning caregiver) | US09 | 200 |
| 22 | GET | `/family-access?olderAdultId=` / `?familyMemberId=&status=` / `?invitationCode=` | Bearer; **Public** only with `?invitationCode=` | US32, US31 | 200 |
| 23 | GET | `/family-access/{id}` | Bearer | US32 | 200 |
| 24 | POST | `/family-access` | Bearer (owning caregiver) | US11 | 201 |
| 25 | PUT | `/family-access/{id}` | Bearer (invitee accepts; owning caregiver revokes) | US31, US32 | 200 |
| 26 | GET | `/daily-reports?olderAdultId=` | Bearer | US13, US22 | 200 |
| 27 | GET | `/daily-reports/{id}` | Bearer | US13 | 200 |
| 28 | POST | `/daily-reports` | Bearer (owning caregiver) | US13 | 201 |
| 29 | PUT | `/daily-reports/{id}` | Bearer (owning caregiver) | US13 | 200 |
| 30 | GET | `/vital-signs?olderAdultId=` | Bearer | US14, US22 | 200 |
| 31 | GET | `/vital-signs/{id}` | Bearer | US14 | 200 |
| 32 | POST | `/vital-signs` | Bearer (owning caregiver) | US14 | 201 |
| 33 | PUT | `/vital-signs/{id}` | Bearer (owning caregiver) | US14 | 200 |
| 34 | GET | `/medications?olderAdultId=[&active=]` | Bearer | US15, US26 | 200 |
| 35 | GET | `/medications/{id}` | Bearer | US15 | 200 |
| 36 | POST | `/medications` | Bearer (owning caregiver) | US15 | 201 |
| 37 | PUT | `/medications/{id}` | Bearer (owning caregiver) | US15 | 200 |

Utility: `GET /health` (outside `/api/v1`, Public) returns `200 {"status":"Healthy"}`; it is used by the hosting platform.

## 2. IAM

### POST `/authentication/sign-up` (Public)

Role can be `Caregiver` or `FamilyMember`; `Admin` is rejected with 400.

```json
{ "email": "andrea.mendoza@vitalita.pe", "password": "Vitalita123", "role": "Caregiver" }
```

Response `201 Created` (header `Location`):

```json
{
  "id": 1,
  "email": "andrea.mendoza@vitalita.pe",
  "role": "Caregiver",
  "status": "Active",
  "createdAt": "2026-11-11T15:00:00Z",
  "lastLoginAt": null
}
```

### POST `/authentication/sign-in` (Public) and POST `/authentication/refresh` (Bearer, no body)

Sign-in request: `{ "email": "andrea.mendoza@vitalita.pe", "password": "Vitalita123" }`.
Both return `200 OK`:

```json
{
  "id": 1,
  "email": "andrea.mendoza@vitalita.pe",
  "role": "Caregiver",
  "status": "Active",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Wrong credentials or inactive account: `401`.

### GET `/authentication/me` (Bearer)

`200 OK` with the same user object as sign-up (`id`, `email`, `role`, `status`, `createdAt`, `lastLoginAt`).

### Users

- `GET /users?email=andrea.mendoza@vitalita.pe` (Public lookup) → `200 [ { "id": 1, "email": "andrea.mendoza@vitalita.pe" } ]`.
- `GET /users` (Admin, no filter) → `200` array of user objects (as above).
- `GET /users/{id}` → `200` user object. Admin can read anyone; other users only themselves.
- `POST /users` (Admin) → `201` user object. Body: `{ "email": "...", "password": "...", "role": "Caregiver" }`.
- `PUT /users/{id}` → `200` user object. Body: `{ "email": "...", "role": "Caregiver", "status": "Active", "password": "..." }` (`password` optional). Nobody can change their own role or status.
- `DELETE /users/{id}` (Admin) → `204 No Content`. It is a soft delete: the user becomes `Inactive`.

## 3. Profiles

POST and PUT bodies are the same object as the response, **without `id`**.

### Caregiver profile – `/caregiver-profiles`

`GET /caregiver-profiles?userId=1` → `200 [ ... ]`; `GET /caregiver-profiles/1` → `200 { ... }`; `POST` → `201`; `PUT /caregiver-profiles/1` → `200`.

```json
{
  "id": 1,
  "userId": 1,
  "firstName": "Andrea",
  "lastName": "Mendoza",
  "phoneNumber": "987654321",
  "professionalTitle": "Licenciada en Enfermería",
  "bio": "Enfermera con 12 años de experiencia en cuidado geriátrico domiciliario.",
  "photoUrl": "",
  "certifications": ["Cuidado geriátrico", "Primeros auxilios"]
}
```

`userId` cannot be changed on PUT (409). One profile per user (409 if it already exists).

### Family member profile – `/family-member-profiles`

`GET /family-member-profiles?userId=2` or `?id=1&id=3` → `200 [ ... ]`.

```json
{ "id": 1, "userId": 2, "firstName": "Renzo", "lastName": "Rodríguez", "phoneNumber": "987111222" }
```

### Older adult – `/older-adults`

`GET /older-adults?caregiverId=1` or `?id=1&id=2` → `200 [ ... ]`. On PUT, `status` is required; on POST it defaults to `Active`.

```json
{
  "id": 1,
  "caregiverId": 1,
  "firstName": "María",
  "lastName": "Rodríguez",
  "birthDate": "1948-03-12",
  "documentNumber": "40218463",
  "bloodType": "O+",
  "allergies": ["Penicilina"],
  "chronicConditions": ["Hipertensión arterial", "Diabetes tipo 2"],
  "emergencyContact": { "name": "Renzo Rodríguez", "phoneNumber": "987111222", "relationship": "Son" },
  "observations": "Requiere asistencia para caminar distancias largas.",
  "status": "Active"
}
```

`status`: `Active`, `Inactive` or `Archived` (there is no DELETE; an older adult is archived with a PUT). `caregiverId` cannot be changed (409).

### Family access (invitations) – `/family-access`

Complete resource (`GET` with token, `GET /{id}`, response of POST and PUT):

```json
{
  "id": 1,
  "olderAdultId": 1,
  "familyMemberId": null,
  "invitedEmail": "carla.rodriguez@correo.pe",
  "invitationCode": "VITA-7KQ2M9XH",
  "relationship": "Daughter",
  "status": "Pending",
  "invitedAt": "2026-11-11T15:00:00Z",
  "acceptedAt": null,
  "expiresAt": "2026-11-18T15:00:00Z"
}
```

`POST /family-access` (owning caregiver). The server generates the code, sets `Pending` and an expiry of 7 days:

```json
{ "olderAdultId": 1, "invitedEmail": "carla.rodriguez@correo.pe", "relationship": "Daughter" }
```

`PUT /family-access/1` – the invitee accepts (`Active`) or the caregiver revokes (`Revoked`):

```json
{ "status": "Active", "familyMemberId": 1, "relationship": "Daughter" }
```

`GET /family-access?invitationCode=VITA-7KQ2M9XH` (**Public**) returns only the minimal data, or `[]` when the code does not exist, was revoked or has expired:

```json
[
  {
    "id": 1,
    "invitedEmail": "carla.rodriguez@correo.pe",
    "invitationCode": "VITA-7KQ2M9XH",
    "relationship": "Daughter",
    "status": "Pending",
    "expiresAt": "2026-11-18T15:00:00Z"
  }
]
```

`relationship`: `Son`, `Daughter`, `Spouse`, `Grandchild`, `Sibling` or `Other`. `status`: `Pending`, `Active` or `Revoked` (`Revoked` is final).

## 4. Monitoring

All lists are filtered with `?olderAdultId=`. Reading requires access to the older adult (owning caregiver or family with active access); writing requires the owning caregiver.

### Daily reports – `/daily-reports`

`recordedByUserId` is taken from the token; any value sent by the client is ignored.

```json
{
  "id": 1,
  "olderAdultId": 1,
  "recordedByUserId": 1,
  "reportDate": "2026-11-11T20:00:00Z",
  "mood": "Good",
  "generalCondition": "Stable",
  "observations": "Durmió bien y almorzó completo."
}
```

`mood`: `VeryGood`, `Good`, `Neutral`, `Bad`, `VeryBad`. `generalCondition`: `Stable`, `Attention`, `Critical`. A future date is rejected (400).

### Vital signs – `/vital-signs`

The `unit` is derived from `type`; the value sent by the client is ignored. `secondaryValue` is required only for `BloodPressure`.

```json
{
  "id": 1,
  "olderAdultId": 1,
  "type": "BloodPressure",
  "value": 120,
  "secondaryValue": 80,
  "unit": "mmHg",
  "measuredAt": "2026-11-11T13:00:00Z"
}
```

Valid ranges: `HeartRate` 20–250, `BloodPressure` 50–260 / 30–160 (systolic greater than diastolic), `OxygenSaturation` 50–100, `Temperature` 30–45, `Glucose` 20–600.

### Medications – `/medications`

`GET /medications?olderAdultId=1&active=true` → `200 [ ... ]`. There is no DELETE: a medication is deactivated with a PUT (`"active": false`).

```json
{
  "id": 1,
  "olderAdultId": 1,
  "name": "Losartán",
  "dosage": "50 mg",
  "frequency": "Cada 12 horas",
  "scheduleTimes": ["08:00", "20:00"],
  "instructions": "Tomar con el almuerzo.",
  "active": true
}
```

`scheduleTimes` has 1 to 12 values in `HH:mm` format; they are stored sorted and without duplicates.

## 5. Error responses

All errors are `application/problem+json` with a `message` field.

| Status | When | Example `message` (`Accept-Language: es`) |
|---|---|---|
| 400 | Invalid body or parameters; the `errors` object lists each field | "La solicitud contiene valores no válidos." |
| 401 | Missing or invalid token, wrong credentials, inactive account | "Se requiere autenticación para acceder a este recurso." |
| 403 | The user cannot access or modify the resource | "No tienes acceso a este recurso." |
| 404 | The resource does not exist | "No se encontró el signo vital." |
| 409 | Duplicate profile or invitation, or an immutable field was changed | "No se puede cambiar el adulto mayor de un registro." |

Validation error (`400`, `Accept-Language: en`):

```json
{
  "title": "The request contains invalid values.",
  "status": 400,
  "errors": { "status": ["This field is required."] },
  "message": "The request contains invalid values."
}
```

Business rule: when a filter points to a resource that belongs to someone else or does not exist, the API answers `403` (not `404`) so it does not reveal what exists.

## 6. Screenshots

Add one screenshot of Swagger UI per resource under `docs/api/img/` once the service is deployed, and reference them here.
