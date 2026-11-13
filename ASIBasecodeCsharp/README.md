# Student Organization Event Registration and Attendance System
_Built on the ASI Bridge/JumpStart C# BaseCode (ASP.NET Core MVC + Razor, .NET 9, SQL Server Express)_

Student organizations publish events, students register (with automatic waitlisting when an event is full),
and officers take attendance by scanning each student's QR ticket.

## Features
| Role | Can do |
|---|---|
| **Admin** | Everything below, plus manage organizations, organization members, users and roles; approve new accounts (filter by college with counts, course or year level, search by name or student no., sort oldest/newest, *Approve all shown* / *Reject all shown*, which never touch accounts that arrived after the page loaded); set temporary passwords for "forgot password" requests |
| **Officer** | Create / edit / publish / close / complete events **for organizations they are a member of**, limit an event to selected courses, take attendance (QR scan, code or student number), check-out, override attendance status, view reports and export CSV |
| **Student** | Self-register an account, browse events, register / cancel, join waitlists, view QR ticket, see attendance history, request a password reset |

Event lifecycle: `Draft → Open → Closed → Completed` (or `Cancelled`). Students only see non-draft events.
Marking an event **Completed** records every registered student who never checked in as **Absent**.
Check-ins later than *Late After (minutes)* past the start time are recorded as **Late**.

**Courses and colleges** follow University of Cebu - Banilad's programs and are defined once in
`ASI.Basecode.Resources/Constants/Courses.cs` (course dropdowns, filters and event eligibility all read it).

**Who can register:** an event is open to all students, or only to students of selected courses (several courses,
across colleges). Other students still see it, marked "For …", but the Register button is disabled and the server
refuses the registration. Students already registered keep their spot if the officer narrows the list later.

**Forgot password:** the student enters their username and student number on the sign-in page. The request appears
under *Account approvals*; the admin checks the student's ID, sets a temporary password and gives it to them in person.
After signing in with it, every page sends the student to *Change password* until they choose a new one.

## Installation
1. Requirements: Visual Studio 2022 (17.12+) or 2026 with the **ASP.NET and web development** workload, and
   **SQL Server Express LocalDB** (installed with that workload; or any SQL Server Express).
2. Create the database and load the schema + demo data by running `Database/student_event_db.sql` once
   (it creates `student_event_db` if it doesn't exist):
   - Visual Studio: *View > SQL Server Object Explorer* > `(localdb)\MSSQLLocalDB` > right-click > *New Query*, paste the file, *Execute*.
   - or command line: `sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i Database\student_event_db.sql`

   Re-running the script resets all tables to the demo data.
3. The connection string in `ASI.Basecode.WebApp/appsettings.json` already points at LocalDB with Windows
   authentication (no password). For a full SQL Server Express install, change `Server=(localdb)\\MSSQLLocalDB` to
   `Server=.\\SQLEXPRESS` (JSON needs the backslash doubled), and use `-S ".\SQLEXPRESS"` with sqlcmd.
4. Set `ASI.Basecode.WebApp` as the Startup project, then Clean and Rebuild the solution and run.

### Hosted database (deployment)
LocalDB only runs on your own PC, so a deployed site needs a hosted SQL Server database (for example
MonsterASP.NET or Azure SQL). Hosts create the database in their control panel and reject `CREATE DATABASE` / `USE`,
and the production site must not reuse the secret key committed in `appsettings.json` (it signs the login cookie
and encrypts passwords). The steps:

1. Create an empty SQL Server database in the host's control panel and copy its connection string.
2. Generate the hosted script (from the `ASIBasecodeCsharp` folder):

   ```
   powershell -ExecutionPolicy Bypass -File Database\build_hosted_script.ps1
   ```

   It prints a new random production secret key and writes `Database/student_event_db.hosted.sql` (git-ignored):
   the same schema and demo data, without `CREATE DATABASE` / `USE`, with the demo passwords re-encrypted for that
   key. The demo passwords stay `Admin@123`, `Officer@123` and `Student@123`. To reuse a key you already have, pass
   `-SecretKey "<key>"` (at least 32 characters).
3. Connect to the hosted database (the host's web SQL tool, SSMS, or Visual Studio's SQL Server Object Explorer) and
   run `student_event_db.hosted.sql` once.
4. Set these on the host as app settings / environment variables (not in `appsettings.json`):
   - `ConnectionStrings__DefaultConnection` = the host's connection string
   - `TokenAuthentication__SecretKey` = the key printed in step 2
   - `ASPNETCORE_ENVIRONMENT` = `Production`

   Keep the key private. If it changes later, regenerate and re-run the script, or every existing password stops working.

### Demo accounts
| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `msantos` | `Officer@123` | Officer (President, Computer Science Society) |
| `jreyes` | `Officer@123` | Officer (Governor, Engineering Student Council) |
| `student1` … `student6` | `Student@123` | Student |

**Switch account (development only):** while the app runs in the `Development` environment (Visual Studio's default for F5), a "Switch account" button sits at the bottom-right of every page, including the sign-in page. It signs you in as any active user without a password so you can try each role. It does not render, and its endpoint returns 404, in any other environment, so never deploy with `ASPNETCORE_ENVIRONMENT=Development`.

The QR scanner uses the device camera, which browsers only allow on `https://` or `localhost`.

## Database
Tables (snake_case, mapped by EF Core's `UseSnakeCaseNamingConvention()`):
`users`, `organizations`, `organization_members`, `events`, `event_allowed_courses`, `event_registrations`,
`attendances`, `password_reset_requests`.
The schema lives in `Database/student_event_db.sql`; `AsiBasecodeDBContext` must stay in sync with it.

**Updating an existing database** (keeps its data): run the scripts in `Database/migrations/` in date order, connected
to the app's database, e.g.
`sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d student_event_db -i Database\migrations\2026-11-13_departments_eligibility_password_reset.sql`.
They are safe to run more than once. A fresh install only needs `student_event_db.sql`.

Demo data highlights: *Hackathon 2026 Orientation* is limited to the College of Computer Studies and BS Computer
Engineering (sign in as `student5`, BS Criminology, to see the disabled Register button), and `student6` has a pending
forgot-password request under *Account approvals*.
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
