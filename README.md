# Butterfly 🦋

A cross-border micro-mentorship platform. US-based **mentors** are matched — by shared values and interests — with talented, rising **mentees** in rural Bangladesh. Local **care managers** create mentee profiles, mediate all communication, and log weekly, transaction-level impact updates. **Admins** approve profiles and verify care managers.

> A small gesture that creates an outsized effect — the butterfly effect.

## Architecture

Monorepo, one solution, single mobile client for all three roles:

```
Butterfly/
├── Butterfly.sln
├── src/
│   ├── Butterfly.Api/       ASP.NET Core Web API (controllers, Entra auth, DI)
│   ├── Butterfly.Data/      EF Core entities, DbContext, migrations
│   ├── Butterfly.Shared/    DTOs / contracts + enums (shared by API and client)
│   └── Butterfly.Mobile/    .NET MAUI app — the ONLY client (Mentor + CareManager + Admin)
├── tests/
│   └── Butterfly.Api.Tests/ xUnit tests
└── README.md
```

- **Auth:** Microsoft Entra External ID (CIAM). The API validates Entra-issued tokens (`Microsoft.Identity.Web`); roles come from Entra App Roles via the `roles` claim. No credentials are stored app-side.
- **Data:** EF Core code-first against Azure SQL (LocalDB / local SQL Server for dev).
- **Client:** .NET MAUI (Android first), MVVM via CommunityToolkit.Mvvm, Refit + Polly, MSAL sign-in.

## Child-safeguarding non-negotiables

Mentee profiles describe minors. Enforced in the data model and API — not just the UI:

- Display name / pseudonym only; never a full legal name.
- Region/village-level location only; never an address or GPS coordinate.
- Profiles are `Pending` until an Admin approves them; only `Approved` profiles are ever visible to mentors.
- No direct mentor↔mentee messaging — care managers mediate all communication.

## Getting started

_Sections below are filled in as the corresponding build phases land._

### Prerequisites
- .NET 8 SDK (pinned via `global.json`)
- `maui-android` workload (for the mobile app)
- Local SQL Server / LocalDB for dev; Azure SQL for deployment

### Configuration (user-secrets — never committed)
_To be documented in the auth + database phases._

### Run the API
_To be documented._

### Run the Android app
_To be documented._

## Status / roadmap
- [x] Phase 0–1: scaffold
- [ ] Shared contracts · Data layer · API auth (Entra) · API features · Tests · Azure SQL · MAUI shell · Dashboards

### Not yet implemented / production TODO
Stripe payments (record-only for pilot) · Key Vault + managed identity · iOS target · safeguarding policy doc.
