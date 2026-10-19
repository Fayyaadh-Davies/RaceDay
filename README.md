# RaceDay API (PROG6212 POE Part 2)

ASP.NET Core Web API for South African road running, walking and cycling events. Built from the Part 1 ERD, endpoint plan and SQL schema using EF Core code-first, SQL Server, hashed passwords and role-based session authentication.

## Projects

- `RaceDay.Api` — REST API, Swagger UI, EF Core SQL Server
- `RaceDay.Tests` — xUnit endpoint tests (in-memory database + GitHub Actions)

## Run locally

1. Install SQL Server LocalDB (Visual Studio) or SQL Server.
2. Update `RaceDay.Api/appsettings.json` if your instance name differs.
3. From the repo root:

```bash
dotnet run --project RaceDay.Api
```
 
4. Open Swagger at `/swagger`.

Seeded demo password for all sample users: `Password123!`

| Role | Email |
| --- | --- |
| Admin | `admin@raceday.co.za` |
| Participant | `thabo@example.com` |
| Organiser | `john@jrr.co.za` |

Log in with `POST /api/auth/login`. Swagger keeps the session cookie for later Try it out calls.

## Authentication

- Passwords are hashed with BCrypt.
- Login stores `UserId`, `Role` and `Email` in session (`RaceDay.Session` cookie).
- Organiser endpoints reject Participants (403) and unauthenticated callers (401).
- Participant enrolment and personal results are blocked for Organisers.

## Tests and CI

```bash
dotnet test
```

GitHub Actions workflow: `.github/workflows/ci.yml` restores, builds and tests on push/PR.
