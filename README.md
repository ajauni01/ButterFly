# Butterfly 🦋

A cross-border micro-mentorship platform. US-based **mentors** are matched — by shared values and interests — with talented, rising **mentees** in rural Bangladesh. Local **care managers** create mentee profiles, mediate all communication, and log weekly, transaction-level impact updates. **Admins** approve profiles and verify care managers.

> A small gesture that creates an outsized effect — the butterfly effect.

---

## Architecture

Monorepo, one solution, a single mobile client for all three roles:

```
Butterfly/
├── Butterfly.sln
├── src/
│   ├── Butterfly.Api/       ASP.NET Core Web API (controllers, Entra auth, DI)
│   ├── Butterfly.Data/      EF Core entities, DbContext, migrations, dev seed
│   ├── Butterfly.Shared/    DTOs / contracts + enums (shared by API and client)
│   └── Butterfly.Mobile/    .NET MAUI app — the ONLY client (Mentor + CareManager + Admin)
├── tests/
│   └── Butterfly.Api.Tests/ xUnit tests (matching, approval gating, ownership)
└── README.md
```

**Reference rules** (enforced): `Data → Shared`; `Api → Data + Shared`; `Mobile → Shared` only; `Api` and `Mobile` never reference each other.

- **Auth:** Microsoft Entra External ID (CIAM). The API validates Entra-issued tokens via `Microsoft.Identity.Web`; roles come from Entra **App Roles** in the `roles` claim. No credentials are stored app-side — an `AppUser` is keyed by the Entra `oid`.
- **Data:** EF Core code-first against Azure SQL (local SQL Server / LocalDB for dev). Connection string in config only; passwordless managed identity preferred in production.
- **Client:** .NET MAUI (Android, iOS, MacCatalyst), MVVM via `CommunityToolkit.Mvvm`, Refit + Polly over `HttpClient`, MSAL sign-in, `SecureStorage` token cache.

### Roles & flows
- **Mentor** (US): survey (first run) → tag-matched approved mentees → "Mentor this person" → my mentorships → impact feed.
- **Care Manager** (Bangladesh): create mentee profile (saved `Pending`) → my profiles with status badges → log weekly impact.
- **Admin**: pending-profile approval queue (approve/reject) → verify care managers → record payments.

### Child-safeguarding non-negotiables (enforced in the data model + API, not just UI)
- Display name / pseudonym only; never a full legal name.
- Region/village-level location only; never an address or GPS coordinate.
- A profile is `Pending` until an Admin approves it; **only `Approved` profiles are ever returned to a mentor** (a mentor hitting a non-approved profile gets `404`, not `403`, so its existence isn't disclosed).
- No direct mentor↔mentee messaging — care managers mediate all communication.
- `Age` is constrained to 1–17 on the DTO, the client, and the domain.

---

## API endpoints

| Method & route | Role | Purpose |
|---|---|---|
| `GET /api/me` | any | Current user profile; upserts `AppUser` + role record on first call |
| `POST /api/mentors/survey` | Mentor | Submit/replace values-interests survey |
| `GET /api/mentors/matches` | Mentor | Tag-matched **Approved** mentees, best first |
| `POST /api/mentorships` | Mentor | Start a mentorship (money only if financial) |
| `GET /api/mentorships/mine` | Mentor | The mentor's mentorships |
| `GET /api/mentorships/{id}/impact` | Mentor | Impact feed (must own the mentorship) |
| `GET /api/profiles/{id}` | any | Role-scoped: Mentor→Approved only, CareManager→own, Admin→all |
| `POST /api/caremanagers/profiles` | CareManager | Create mentee profile (`Pending`) |
| `GET /api/caremanagers/profiles/mine` | CareManager | Own profiles + statuses |
| `POST /api/mentorships/{id}/impact` | CareManager | Log weekly impact (must manage the mentee) |
| `GET /api/admin/profiles/pending` | Admin | Approval queue |
| `POST /api/admin/profiles/{id}/approve` | Admin | Approve (records admin id) |
| `POST /api/admin/profiles/{id}/reject` | Admin | Reject with reason |
| `POST /api/admin/caremanagers/{id}/verify` | Admin | Verify a care manager |
| `POST /api/admin/payments` | Admin | Record a payment (Financial/Both only) |

All failures return a consistent `ErrorDto` (`{ code, message, errors? }`).

---

## Prerequisites

- **.NET 8 SDK** (pinned via [`global.json`](global.json)).
- **MAUI workloads**: `dotnet workload install maui-android maui-ios maui-maccatalyst`.
- **Android**: Android SDK + a JDK 17–21 (Android Studio's bundled JBR works). Machine-local SDK/JDK paths go in `src/Butterfly.Mobile/Butterfly.Mobile.csproj.user` (gitignored).
- **iOS/MacCatalyst**: full Xcode active (`sudo xcode-select -s /Applications/Xcode.app/Contents/Developer`, then `sudo xcodebuild -license accept`) and an iOS simulator runtime (`xcodebuild -downloadPlatform iOS`).
- Local **SQL Server / LocalDB** for dev, or an **Azure SQL** database (see below).

---

## Configuration (never commit secrets)

The API reads Entra + the SQL connection string from **user-secrets** in development. The default `appsettings.Development.json` ships *syntactically-valid dummy* Entra values so the API starts and returns a clean `401` for unauthenticated requests — token **validation** only works once you set real values.

Set real values (from the Entra setup below):

```bash
cd src/Butterfly.Api
dotnet user-secrets set "EntraExternalId:Instance"    "https://<subdomain>.ciamlogin.com/"
dotnet user-secrets set "EntraExternalId:TenantId"    "<tenant-id>"
dotnet user-secrets set "EntraExternalId:ClientId"    "<api-app-client-id>"
dotnet user-secrets set "EntraExternalId:Audience"    "<api-app-client-id>"
dotnet user-secrets set "EntraExternalId:Authority"   "https://<subdomain>.ciamlogin.com/<tenant-id>/v2.0"
dotnet user-secrets set "EntraExternalId:ApiScopeUri" "api://<api-app-client-id>"
dotnet user-secrets set "Swagger:ClientId"            "<client-app-client-id>"
dotnet user-secrets set "ConnectionStrings:ButterflyDb" "<your-connection-string>"
```

The MAUI client's public (non-secret) config lives in [`ButterflyConfig.cs`](src/Butterfly.Mobile/Configuration/ButterflyConfig.cs): tenant subdomain, client-app id, the `api://…/access_as_user` scope, and the API base URL. Update the Android MSAL redirect scheme in [`AndroidManifest.xml`](src/Butterfly.Mobile/Platforms/Android/AndroidManifest.xml) to `msal<client-app-id>` to match.

---

## Microsoft Entra External ID setup (portal, one-time)

1. **Create an External ID tenant** (Entra admin center → *Manage tenants → Create → External*). Note the **Tenant ID** and subdomain (`https://<subdomain>.ciamlogin.com/`).
2. **Register the API app** (`Butterfly.Api`): copy its **client id** (= `ClientId` and `Audience`). *Expose an API* → accept App ID URI `api://<api-client-id>` → add a scope `access_as_user`.
3. **Define three App Roles** on the API app — values **exactly** `Mentor`, `CareManager`, `Admin` (case-sensitive; the code checks these strings).
4. **Register the client app** (`Butterfly.Client`, public): add redirect URIs
   - Web (Swagger): `https://localhost:5001/swagger/oauth2-redirect.html`
   - Mobile/desktop (MAUI): `msal<client-app-id>://auth`
   - Grant it delegated `access_as_user` on `Butterfly.Api` + admin consent.
5. **Create test users** and assign each an App Role via *Enterprise applications → Butterfly.Api → Users and groups*.

**Social sign-up (Google, etc.) & default role.** To let users sign up with Google, add Google as an identity provider (*External Identities → Identity providers → Google*, using an OAuth client from Google Cloud Console) and enable it on your sign-up/sign-in user flow. Self-service sign-ups do **not** receive an Entra App Role, so the API grants them the **Mentor** role by default (`DefaultRoleClaimsTransformation`) — the public-facing persona. **CareManager** and **Admin** remain invite-only: assign them explicitly in Entra, and they always take precedence when present.

---

## Azure SQL setup (portal, one-time)

1. **Create a SQL Database** named `Butterfly` on a new SQL server (pick the free/serverless tier for the pilot).
2. Prefer **Microsoft Entra-only (passwordless)** authentication; set yourself as the Entra admin.
3. **Networking**: allow your client IP (one-click "Add current client IP") and enable "Allow Azure services…".
4. Copy the **ADO.NET connection string** and set it as `ConnectionStrings:ButterflyDb` (user-secrets in dev; Key Vault + managed identity in prod). Passwordless form:
   `Server=tcp:<server>.database.windows.net,1433;Database=Butterfly;Authentication=Active Directory Default;Encrypt=True;`

The API auto-applies migrations and seeds demo data in Development (3 Approved + 1 Pending mentee). To apply migrations manually:
```bash
dotnet tool restore
dotnet ef database update --project src/Butterfly.Data
```

---

## Running

### API (with Entra-aware Swagger)
```bash
dotnet run --project src/Butterfly.Api --launch-profile https
# → https://localhost:5001/swagger  (acquire a token via the Authorize button)
```

### Android app
```bash
dotnet build src/Butterfly.Mobile -f net8.0-android -t:Run
```
The Android emulator reaches the host API at `https://10.0.2.2:5001` (already set in `ButterflyConfig`). In DEBUG the client trusts the ASP.NET dev certificate so the emulator can call `https://localhost` — never shipped.

### iOS app (simulator)
```bash
dotnet build src/Butterfly.Mobile -f net8.0-ios -t:Run
```

### Tests
```bash
dotnet test tests/Butterfly.Api.Tests
```

---

## Testing

xUnit + FluentAssertions, EF Core InMemory. Three areas:
- **MatchingService** ordering (descending tag overlap, case-insensitive, preferred-category tiebreak).
- **Approval gating** — `Pending`/`Rejected` never returned to a mentor (matches feed *and* direct lookup).
- **Ownership** — a mentor can't read another's impact feed; a care manager can't log impact for a mentee they don't manage.

---

## Not yet implemented / production TODO

- **Stripe payments** — the `Payment` entity is Stripe-ready (`Method`, `ExternalRef`, `Status`) but the pilot is record-only; no live processor.
- **Key Vault + managed identity** — dev uses user-secrets; production should source Entra config + the SQL connection string from Key Vault with a passwordless Azure SQL connection.
- **Photo storage** — `PhotoUrl` fields assume an external blob store; upload flow not built.
- **Safeguarding-policy document** — the code enforces the rules; a written policy + review process is a separate deliverable.
- **iOS release signing / App Store** — only simulator/dev builds are wired.
- **Richer client screens** — impact-feed detail, care-manager impact logging form, and admin payment/verify screens are supported by the API and DTOs; the pilot client focuses on the primary flow per role.

---

## Build order (how this was assembled)

Scaffold → Shared contracts → Data layer → API auth (Entra) → API features + matching → tests → Azure SQL wiring → MAUI auth shell → MAUI dashboards → this README. Each phase built and committed independently.
