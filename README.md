# Student Organization Event Registration and Attendance System
_Built on the ASI Bridge/JumpStart C# BaseCode (ASP.NET Core MVC + Razor, .NET 9, PostgreSQL)_

Student organizations publish events, students register (with automatic waitlisting when an event is full),
and officers take attendance by scanning each student's QR ticket.

## Features
| Role | Can do |
|---|---|
| **Admin** | Everything below, plus manage organizations, organization members, users and roles |
| **Officer** | Create / edit / publish / close / complete events **for organizations they are a member of**, take attendance (QR scan, code or student number), check-out, override attendance status, view reports and export CSV |
| **Student** | Self-register an account, browse events, register / cancel, join waitlists, view QR ticket, see attendance history |

Event lifecycle: `Draft → Open → Closed → Completed` (or `Cancelled`). Students only see non-draft events.
Marking an event **Completed** records every registered student who never checked in as **Absent**.
Check-ins later than *Late After (minutes)* past the start time are recorded as **Late**.

## Installation
1. Requirements: Visual Studio 2022 (or the .NET 9 SDK) and PostgreSQL 13+.
2. Create the database and load the schema + demo data:
   ```
   psql -U postgres -c "CREATE DATABASE student_event_db;"
   psql -U postgres -d student_event_db -f Database/student_event_db.sql
   ```
   (Or run the same two steps from pgAdmin's Query Tool.) Re-running the script resets all tables.
3. Update `ConnectionStrings:DefaultConnection` in `ASI.Basecode.WebApp/appsettings.json` with your PostgreSQL password.
4. Set `ASI.Basecode.WebApp` as the Startup project, then Clean and Rebuild the solution and run.

### Demo accounts
| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `msantos` | `Officer@123` | Officer (President, Computer Science Society) |
| `jreyes` | `Officer@123` | Officer (Governor, Engineering Student Council) |
| `student1` … `student6` | `Student@123` | Student |

The QR scanner uses the device camera, which browsers only allow on `https://` or `localhost`.

## Database
Tables (snake_case, mapped by EF Core's `UseSnakeCaseNamingConvention()`):
`users`, `organizations`, `organization_members`, `events`, `event_registrations`, `attendances`.
The schema lives in `Database/student_event_db.sql`; `AsiBasecodeDBContext` must stay in sync with it.
Passwords are encrypted with the existing `PasswordManager` using `TokenAuthentication:SecretKey`, so changing
that key invalidates the seeded passwords.

## Code Structure

- `ASI.Basecode.Data`
    - This project contains the repositories and other logics that involves with database.
    - Make sure that the repository is for db processing only.
    - If it involves additional logic, move the logic into the service file.
- `ASI.Basecode.Resources`
    - This project is dedicated to storing messages, labels, and other resources used by the website.
    - It serves as a centralized location for managing static content that is displayed to users.
    - Store all messages, labels, translations, and any non-code assets required for the site in this project.
- `ASI.Basecode.Services`
    - This project contains the services and other processing logics before connecting to the repository.
    - This can also contains classes that can be use by the WebApp project with/without db processing.
- `ASI.Basecode.WebApp`
    - This project contains the main codes especially the `Controllers` and `Views`.
    - This is where you put the connection logic to the APIs.
    - Make sure that the controllers are clean and no other logic should involve.
    - If it involves additional logic, move the logic into the service file.
