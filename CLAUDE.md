# Butterfly — Project Memory

Cross-border micro-mentorship platform. Single .NET MAUI app + ASP.NET Core API + Azure SQL + Microsoft Entra External ID.

## Roles
Mentor (US) · Mentee (Bangladesh, a minor) · CareManager (local middleman) · Admin (approves profiles, verifies care managers).

## NON-NEGOTIABLES (never violate)
1. CHILD SAFEGUARDING: mentees are minors. First name / pseudonym only. Region/village-level location only — never precise address or GPS. MenteeProfile is Pending until Admin approval before any Mentor can see it. No direct mentor↔mentee free-text messaging; care manager mediates.
2. AUTH: Microsoft Entra External ID only. Do NOT use ASP.NET Core Identity tables. Do NOT hand-roll JWT issuance. Validate Entra tokens via Microsoft.Identity.Web. Roles = Entra App Roles in the `roles` claim. AppUser is keyed by Entra `oid`; no credentials stored app-side.
3. PAYMENTS: record-only for the pilot. No Stripe/live processor yet — but keep the Payment entity Stripe-ready. Payments apply only to Financial/Both relationships.
4. SINGLE CLIENT: the MAUI app is the only front-end (role-routed). No Blazor / no other web UI.
5. DB: Azure SQL Database, EF Core code-first. Connection string in config only.

## Architecture
Monorepo, one solution. Butterfly.Shared (DTOs+enums), Butterfly.Data (EF), Butterfly.Api (Web API), Butterfly.Mobile (MAUI). Clients talk to API over HTTP and share only Shared. Api/Mobile never reference each other.

## Workflow
Work one build-order phase per turn. Run `dotnet build` and commit after each phase. For Entra tenant setup and Azure SQL provisioning, STOP and hand the user a checklist — those are manual/portal steps the user does, not you. Build against user-secrets placeholders until the user supplies real values.

## Stack
.NET 8 LTS · ASP.NET Core Web API (controllers) · EF Core · MAUI + CommunityToolkit.Mvvm + Refit + MSAL + Polly · xUnit + FluentAssertions. MAUI targets Android first.

## Environment notes (this machine)
- .NET SDKs live user-locally in `~/.dotnet` (8.0.422 pinned via global.json; a 10.x SDK coexists). `DOTNET_ROOT=$HOME/.dotnet`.
- Butterfly.Mobile targets `net8.0-android;net8.0-ios;net8.0-maccatalyst` (Android first, then iOS). No Windows/Tizen TFMs.
- Workloads: `maui-android`, `maui-ios`, `maui-maccatalyst`. Android builds use Android Studio's bundled JDK 21 (paths in gitignored `.csproj.user`).
- iOS builds need the full Xcode active: `sudo xcode-select -s /Applications/Xcode.app/Contents/Developer` + `sudo xcodebuild -license accept`, plus an iOS simulator runtime (`xcodebuild -downloadPlatform iOS`). VERIFIED: scaffold builds clean for net8.0-ios against Xcode 26.3 despite the .NET 8 workload being older.
