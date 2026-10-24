-- =====================================================================
--  Student Organization Event Registration and Attendance System
--  SQL Server (Express / LocalDB) schema + demo data
--
--  Run the whole file once; it creates the database if needed:
--    * Visual Studio: SQL Server Object Explorer > (localdb)\MSSQLLocalDB > right-click > New Query,
--      paste this file, Execute.  (or SSMS: File > Open > this file > Execute)
--    * Command line:
--        sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i Database\student_event_db.sql
--      For a full SQL Server Express install use -S ".\SQLEXPRESS" instead.
--
--  WARNING: re-running this script DROPS and recreates all tables (data is reset).
--
--  Names are snake_case to match EF Core's UseSnakeCaseNamingConvention()
--  (see AsiBasecodeDBContext). Enum columns store the enum name as text.
--
--  Demo accounts (password in parentheses):
--      admin     (Admin@123)    - Admin
--      msantos   (Officer@123)  - Officer, President of CSS
--      jreyes    (Officer@123)  - Officer, Governor of ESC
--      student1..student6 (Student@123) - Students
--      bsantiago, kaquino, jflores (Student@123) - pending approval; cannot sign in until approved
--      demo01..demo24 (Student@123) - extra students; applicant1..applicant9 - more pending approvals
-- =====================================================================

-- required for the filtered unique index (sqlcmd defaults these OFF; SSMS/Visual Studio default ON)
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
IF DB_ID(N'student_event_db') IS NULL
    CREATE DATABASE student_event_db;
GO
USE student_event_db;
GO
SET NOCOUNT ON;

DROP TABLE IF EXISTS attendances;
DROP TABLE IF EXISTS event_registrations;
DROP TABLE IF EXISTS events;
DROP TABLE IF EXISTS organization_members;
DROP TABLE IF EXISTS organizations;
DROP TABLE IF EXISTS users;

-- ---------------------------------------------------------------------
-- Tables
-- ---------------------------------------------------------------------

CREATE TABLE users (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    user_id         NVARCHAR(50)  NOT NULL,
    first_name      NVARCHAR(50)  NOT NULL,
    middle_name     NVARCHAR(50)  NULL,
    last_name       NVARCHAR(50)  NOT NULL,
    name            NVARCHAR(160) NOT NULL,   -- "First Middle Last", composed by the app
    email           NVARCHAR(100) NULL,
    student_number  NVARCHAR(20)  NULL,
    course          NVARCHAR(100) NULL,
    year_level      INT           NULL,
    role            NVARCHAR(20)  NOT NULL CONSTRAINT df_users_role DEFAULT 'Student',
    is_active       BIT           NOT NULL CONSTRAINT df_users_is_active DEFAULT 1,
    is_approved     BIT           NOT NULL CONSTRAINT df_users_is_approved DEFAULT 0,  -- self-registered accounts wait for an admin
    approved_by     NVARCHAR(50)  NULL,
    approved_time   DATETIME2     NULL,
    password        NVARCHAR(255) NOT NULL,
    created_by      NVARCHAR(50)  NOT NULL,
    created_time    DATETIME2     NOT NULL CONSTRAINT df_users_created DEFAULT SYSDATETIME(),
    updated_by      NVARCHAR(50)  NOT NULL,
    updated_time    DATETIME2     NOT NULL CONSTRAINT df_users_updated DEFAULT SYSDATETIME(),
    CONSTRAINT uq_users_user_id UNIQUE (user_id),
    CONSTRAINT ck_users_role CHECK (role IN ('Admin', 'Officer', 'Student')),
    CONSTRAINT ck_users_year_level CHECK (year_level IS NULL OR year_level BETWEEN 1 AND 6)
);
-- unique student numbers, but any number of staff accounts without one
CREATE UNIQUE INDEX uq_users_student_number ON users (student_number) WHERE student_number IS NOT NULL;

CREATE TABLE organizations (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    name            NVARCHAR(150)  NOT NULL,
    acronym         NVARCHAR(20)   NULL,
    description     NVARCHAR(1000) NULL,
    adviser         NVARCHAR(100)  NULL,
    is_active       BIT            NOT NULL CONSTRAINT df_organizations_is_active DEFAULT 1,
    created_by      NVARCHAR(50)   NOT NULL,
    created_time    DATETIME2      NOT NULL CONSTRAINT df_organizations_created DEFAULT SYSDATETIME(),
    updated_by      NVARCHAR(50)   NOT NULL,
    updated_time    DATETIME2      NOT NULL CONSTRAINT df_organizations_updated DEFAULT SYSDATETIME(),
    CONSTRAINT uq_organizations_name UNIQUE (name)
);

CREATE TABLE organization_members (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    organization_id INT          NOT NULL CONSTRAINT fk_members_organization REFERENCES organizations (id) ON DELETE CASCADE,
    member_id       INT          NOT NULL CONSTRAINT fk_members_user REFERENCES users (id) ON DELETE CASCADE,
    position        NVARCHAR(50) NULL,
    joined_time     DATETIME2    NOT NULL CONSTRAINT df_members_joined DEFAULT SYSDATETIME(),
    CONSTRAINT uq_organization_members UNIQUE (organization_id, member_id)
);

CREATE TABLE events (
    id                    INT IDENTITY(1,1) PRIMARY KEY,
    organization_id       INT            NOT NULL CONSTRAINT fk_events_organization REFERENCES organizations (id),  -- no cascade: keep event history
    title                 NVARCHAR(150)  NOT NULL,
    description           NVARCHAR(2000) NULL,
    venue                 NVARCHAR(150)  NOT NULL,
    start_time            DATETIME2      NOT NULL,
    end_time              DATETIME2      NOT NULL,
    registration_deadline DATETIME2      NOT NULL,
    capacity              INT            NOT NULL CONSTRAINT df_events_capacity DEFAULT 0,
    late_after_minutes    INT            NOT NULL CONSTRAINT df_events_late DEFAULT 15,
    status                NVARCHAR(20)   NOT NULL CONSTRAINT df_events_status DEFAULT 'Draft',
    created_by            NVARCHAR(50)   NOT NULL,
    created_time          DATETIME2      NOT NULL CONSTRAINT df_events_created DEFAULT SYSDATETIME(),
    updated_by            NVARCHAR(50)   NOT NULL,
    updated_time          DATETIME2      NOT NULL CONSTRAINT df_events_updated DEFAULT SYSDATETIME(),
    CONSTRAINT ck_events_status CHECK (status IN ('Draft', 'Open', 'Closed', 'Completed', 'Cancelled')),
    CONSTRAINT ck_events_schedule CHECK (end_time > start_time),
    CONSTRAINT ck_events_deadline CHECK (registration_deadline <= start_time),
    CONSTRAINT ck_events_capacity CHECK (capacity >= 0),
    CONSTRAINT ck_events_late CHECK (late_after_minutes >= 0)
);
CREATE INDEX ix_events_start_time ON events (start_time);
CREATE INDEX ix_events_organization_id ON events (organization_id);

CREATE TABLE event_registrations (
    id                INT IDENTITY(1,1) PRIMARY KEY,
    event_id          INT          NOT NULL CONSTRAINT fk_registrations_event REFERENCES events (id) ON DELETE CASCADE,
    student_id        INT          NOT NULL CONSTRAINT fk_registrations_student REFERENCES users (id) ON DELETE CASCADE,
    registration_code NVARCHAR(32) NOT NULL,
    status            NVARCHAR(20) NOT NULL CONSTRAINT df_registrations_status DEFAULT 'Registered',
    registered_time   DATETIME2    NOT NULL CONSTRAINT df_registrations_time DEFAULT SYSDATETIME(),
    cancelled_time    DATETIME2    NULL,
    CONSTRAINT uq_event_registrations_event_student UNIQUE (event_id, student_id),
    CONSTRAINT uq_event_registrations_code UNIQUE (registration_code),
    CONSTRAINT ck_event_registrations_status CHECK (status IN ('Registered', 'Waitlisted', 'Cancelled'))
);
CREATE INDEX ix_event_registrations_student_id ON event_registrations (student_id);

CREATE TABLE attendances (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    registration_id INT           NOT NULL CONSTRAINT fk_attendances_registration REFERENCES event_registrations (id) ON DELETE CASCADE,
    check_in_time   DATETIME2     NULL,
    check_out_time  DATETIME2     NULL,
    status          NVARCHAR(20)  NOT NULL,
    remarks         NVARCHAR(250) NULL,
    recorded_by     NVARCHAR(50)  NOT NULL,
    recorded_time   DATETIME2     NOT NULL CONSTRAINT df_attendances_recorded DEFAULT SYSDATETIME(),
    CONSTRAINT uq_attendances_registration UNIQUE (registration_id),
    CONSTRAINT ck_attendances_status CHECK (status IN ('Present', 'Late', 'Absent'))
);
GO

-- ---------------------------------------------------------------------
-- Demo data (dates are relative to when the script runs)
-- Passwords are AES-encrypted with PasswordManager and the SecretKey in appsettings.json.
-- ---------------------------------------------------------------------
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @now   DATETIME2 = SYSDATETIME();
DECLARE @today DATETIME2 = CAST(CAST(@now AS date) AS DATETIME2);           -- today 00:00
DECLARE @hour  DATETIME2 = DATEADD(hour, DATEPART(hour, @now), @today);      -- start of the current hour

SET IDENTITY_INSERT users ON;
INSERT INTO users (id, user_id, first_name, middle_name, last_name, name, email, student_number, course, year_level, role, is_active, password, created_by, updated_by) VALUES
 (1, 'admin',    'System', NULL,       'Administrator', 'System Administrator',     'admin@school.edu',       NULL,         NULL,                        NULL, 'Admin',   1, '0oGwxqqIMRkUY18Gmo8ygw==', 'sys', 'sys'),
 (2, 'msantos',  'Maria',  NULL,       'Santos',        'Maria Santos',             'msantos@school.edu',     '2022-10001', 'BS Computer Science',       4,    'Officer', 1, 'Bzwl+bhyITd9xUcAclUGiQ==', 'sys', 'sys'),
 (3, 'jreyes',   'Jose',   'Mercado',  'Reyes',         'Jose Mercado Reyes',       'jreyes@school.edu',      '2022-10002', 'BS Civil Engineering',      4,    'Officer', 1, 'Bzwl+bhyITd9xUcAclUGiQ==', 'sys', 'sys'),
 (4, 'student1', 'Ana',    NULL,       'Cruz',          'Ana Cruz',                 'acruz@school.edu',       '2024-20001', 'BS Computer Science',       2,    'Student', 1, 'yCu8wa23pxcND4LfRFKf4A==', 'sys', 'sys'),
 (5, 'student2', 'Mark',   NULL,       'Villanueva',    'Mark Villanueva',          'mvillanueva@school.edu', '2024-20002', 'BS Information Technology', 2,    'Student', 1, 'yCu8wa23pxcND4LfRFKf4A==', 'sys', 'sys'),
 (6, 'student3', 'Liza',   'Villareal','Garcia',        'Liza Villareal Garcia',    'lgarcia@school.edu',     '2023-20003', 'BS Civil Engineering',      3,    'Student', 1, 'yCu8wa23pxcND4LfRFKf4A==', 'sys', 'sys'),
 (7, 'student4', 'Paolo',  NULL,       'Mendoza',       'Paolo Mendoza',            'pmendoza@school.edu',    '2025-20004', 'BS Computer Science',       1,    'Student', 1, 'yCu8wa23pxcND4LfRFKf4A==', 'sys', 'sys'),
 (8, 'student5', 'Grace',  'Lim',      'Tan',           'Grace Lim Tan',            'gtan@school.edu',        '2023-20005', 'AB Communication',          3,    'Student', 1, 'yCu8wa23pxcND4LfRFKf4A==', 'sys', 'sys'),
 (9, 'student6', 'Carlo',  NULL,       'Ramos',         'Carlo Ramos',              'cramos@school.edu',      '2024-20006', 'BS Information Technology', 2,    'Student', 1, 'yCu8wa23pxcND4LfRFKf4A==', 'sys', 'sys');

-- Everyone above is an approved account
UPDATE users SET is_approved = 1, approved_by = 'admin', approved_time = DATEADD(day, -30, @now);

-- Self-registrations waiting for approval (they cannot sign in yet)
INSERT INTO users (id, user_id, first_name, middle_name, last_name, name, email, student_number, course, year_level, role, is_active, is_approved, password, created_by, updated_by, created_time, updated_time) VALUES
 (10, 'bsantiago', 'Bea',  'Cruz', 'Santiago', 'Bea Cruz Santiago', 'bsantiago@school.edu', '2026-30001', 'BS Psychology',             1, 'Student', 1, 0, 'yCu8wa23pxcND4LfRFKf4A==', 'bsantiago', 'bsantiago', DATEADD(day, -2, @now),  DATEADD(day, -2, @now)),
 (11, 'kaquino',   'Ken',  NULL,   'Aquino',   'Ken Aquino',        'kaquino@school.edu',   '2026-30002', 'BS Information Technology', 1, 'Student', 1, 0, 'yCu8wa23pxcND4LfRFKf4A==', 'kaquino',   'kaquino',   DATEADD(day, -1, @now),  DATEADD(day, -1, @now)),
 (12, 'jflores',   'Joy',  NULL,   'Flores',   'Joy Flores',        'jflores@school.edu',   '2026-30003', 'BS Accountancy',            2, 'Student', 1, 0, 'yCu8wa23pxcND4LfRFKf4A==', 'jflores',   'jflores',   DATEADD(hour, -3, @now), DATEADD(hour, -3, @now));
SET IDENTITY_INSERT users OFF;

SET IDENTITY_INSERT organizations ON;
INSERT INTO organizations (id, name, acronym, description, adviser, is_active, created_by, updated_by) VALUES
 (1, 'Computer Science Society',    'CSS', 'Academic organization of Computer Science and IT students.', 'Prof. Ramon Dizon',   1, 'admin', 'admin'),
 (2, 'Engineering Student Council', 'ESC', 'Student government of the College of Engineering.',         'Engr. Teresa Lim',    1, 'admin', 'admin'),
 (3, 'Arts and Culture Club',       'ACC', 'Promotes arts, music and cultural heritage on campus.',       'Ms. Patricia Aquino', 1, 'admin', 'admin');
SET IDENTITY_INSERT organizations OFF;

INSERT INTO organization_members (organization_id, member_id, position) VALUES
 (1, 2, 'President'),
 (1, 4, 'Member'),
 (1, 7, 'Member'),
 (2, 3, 'Governor'),
 (2, 6, 'Member'),
 (3, 8, 'Member');

SET IDENTITY_INSERT events ON;
INSERT INTO events (id, organization_id, title, description, venue, start_time, end_time, registration_deadline, capacity, late_after_minutes, status, created_by, updated_by) VALUES
 -- happening today: registration closed, ready for check-in
 (1, 1, 'Tech Talk: Careers in Software',
     'Alumni share how they landed their first software jobs. Q&A to follow.',
     'AVR 1, Main Building',
     DATEADD(hour, 1, @hour), DATEADD(hour, 3, @hour), @hour,
     50, 15, 'Closed', 'msantos', 'msantos'),
 -- upcoming, open with free slots
 (2, 1, 'Intro to Web Development Workshop',
     'Hands-on workshop on HTML, CSS and ASP.NET Core. Bring your own laptop.',
     'Computer Lab 3',
     DATEADD(hour, 7 * 24 + 9, @today), DATEADD(hour, 7 * 24 + 12, @today), DATEADD(hour, 6 * 24 + 17, @today),
     30, 15, 'Open', 'msantos', 'msantos'),
 -- upcoming, open and full (demonstrates the waitlist)
 (3, 1, 'Hackathon 2026 Orientation',
     'Team formation and rules briefing for this year''s hackathon. Limited slots.',
     'Innovation Hub',
     DATEADD(hour, 3 * 24 + 13, @today), DATEADD(hour, 3 * 24 + 15, @today), DATEADD(hour, 2 * 24 + 17, @today),
     3, 10, 'Open', 'msantos', 'msantos'),
 -- draft (not visible to students yet)
 (4, 2, 'Engineering Leadership Summit',
     'Leadership training for class officers of the College of Engineering.',
     'Engineering Auditorium',
     DATEADD(hour, 14 * 24 + 8, @today), DATEADD(hour, 14 * 24 + 17, @today), DATEADD(hour, 12 * 24 + 17, @today),
     100, 15, 'Draft', 'jreyes', 'jreyes'),
 -- finished, with attendance history
 (5, 3, 'Cultural Night 2026',
     'An evening of music, dance and theater performances.',
     'University Gymnasium',
     DATEADD(hour, -10 * 24 + 18, @today), DATEADD(hour, -10 * 24 + 21, @today), DATEADD(day, -11, @today),
     0, 30, 'Completed', 'admin', 'admin');
SET IDENTITY_INSERT events OFF;

SET IDENTITY_INSERT event_registrations ON;
INSERT INTO event_registrations (id, event_id, student_id, registration_code, status, registered_time) VALUES
 -- Tech Talk (today)
 (1,  1, 4, 'TT4K9QZP2A', 'Registered', DATEADD(day, -3, @now)),
 (2,  1, 5, 'TT7M3XWR8B', 'Registered', DATEADD(day, -3, @now)),
 (3,  1, 7, 'TT2H6LNC5D', 'Registered', DATEADD(day, -2, @now)),
 (4,  1, 9, 'TT9P4VJS3E', 'Registered', DATEADD(day, -1, @now)),
 -- Web Dev Workshop
 (5,  2, 4, 'WD3R8KTA6F', 'Registered', DATEADD(day, -1, @now)),
 (6,  2, 8, 'WD6Y2GMB4H', 'Registered', DATEADD(hour, -5, @now)),
 -- Hackathon Orientation (capacity 3 -> full, one waitlisted)
 (7,  3, 5, 'HK5N7QDC2J', 'Registered', DATEADD(day, -2, @now)),
 (8,  3, 7, 'HK8T3WEF9K', 'Registered', DATEADD(day, -2, @now)),
 (9,  3, 9, 'HK2L6ZRG7M', 'Registered', DATEADD(day, -1, @now)),
 (10, 3, 6, 'HK4B9XHN3P', 'Waitlisted', DATEADD(hour, -6, @now)),
 -- Cultural Night (completed)
 (11, 5, 4, 'CN7C2PJQ5R', 'Registered', DATEADD(day, -15, @now)),
 (12, 5, 6, 'CN3F8SKT6S', 'Registered', DATEADD(day, -15, @now)),
 (13, 5, 8, 'CN9G4UMV2T', 'Registered', DATEADD(day, -14, @now)),
 (14, 5, 9, 'CN5J6WNX8U', 'Registered', DATEADD(day, -13, @now));
SET IDENTITY_INSERT event_registrations OFF;

DECLARE @cn DATETIME2 = DATEADD(day, -10, @today);   -- Cultural Night's date
INSERT INTO attendances (registration_id, check_in_time, check_out_time, status, remarks, recorded_by, recorded_time) VALUES
 (11, DATEADD(minute, 17 * 60 + 55, @cn), DATEADD(hour, 21, @cn),              'Present', NULL,      'admin', DATEADD(minute, 17 * 60 + 55, @cn)),
 (12, DATEADD(minute, 18 * 60 + 45, @cn), NULL,                                'Late',    'Traffic', 'admin', DATEADD(minute, 18 * 60 + 45, @cn)),
 (13, DATEADD(minute, 18 * 60 + 5, @cn),  DATEADD(minute, 20 * 60 + 30, @cn),  'Present', NULL,      'admin', DATEADD(minute, 18 * 60 + 5, @cn)),
 (14, NULL,                               NULL,                                'Absent',  NULL,      'admin', DATEADD(hour, 21, @cn));

-- ---------------------------------------------------------------------
-- Bulk demo data: enough rows that every table runs past one page
-- (the UI shows 10 rows per page). New rows take normal identity ids
-- and are looked up by key.
-- ---------------------------------------------------------------------

-- 24 more approved students (password Student@123), usernames demo01..demo24
WITH n AS (SELECT TOP (24) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects),
s AS (
    SELECT n,
        CHOOSE(n, 'Andrea','Benjie','Camille','Dennis','Erika','Francis','Gabriel','Hannah','Ivan','Jasmine','Kevin','Lara',
                  'Miguel','Nicole','Oscar','Patricia','Rafael','Sofia','Tristan','Uma','Vince','Wendy','Xavier','Yna') AS f,
        CHOOSE(n, 'Bautista','Castillo','Domingo','Enriquez','Fernandez','Gonzales','Hernandez','Ignacio','Javier','Lopez','Marquez','Navarro',
                  'Ocampo','Pascual','Quinto','Rivera','Salazar','Torres','Uy','Valdez','Wong','Yap','Zamora','Agustin') AS l,
        CHOOSE(1 + (n % 8), 'BS Computer Science','BS Information Technology','BS Civil Engineering','BS Accountancy',
                            'BS Nursing','AB Communication','BS Psychology','BS Business Administration') AS c
    FROM n)
INSERT INTO users (user_id, first_name, last_name, name, email, student_number, course, year_level, role, is_active, is_approved, approved_by, approved_time, password, created_by, updated_by)
SELECT 'demo' + RIGHT('00' + CAST(n AS VARCHAR(10)), 2), f, l, f + ' ' + l,
       LOWER(f) + '.' + LOWER(l) + '@school.edu',
       '2025-5' + RIGHT('0000' + CAST(n AS VARCHAR(10)), 4), c, 1 + (n % 4), 'Student', 1, 1, 'admin', DATEADD(day, -20, @now),
       'yCu8wa23pxcND4LfRFKf4A==', 'sys', 'sys'
FROM s;

-- 9 more registrations waiting for approval (12 pending in total)
WITH n AS (SELECT TOP (9) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects),
s AS (
    SELECT n,
        CHOOSE(n, 'Abby','Bryan','Carla','Dave','Ella','Finn','Gia','Hugo','Iris') AS f,
        CHOOSE(n, 'Mercado','Soriano','Villar','Tolentino','Magno','Robles','Santos','Lim','Velasco') AS l,
        CHOOSE(n, 'BS Nursing','BS Accountancy','BS Information Systems','BS Tourism Management','BS Psychology',
                  'BS Computer Engineering','AB Political Science','BS Hospitality Management','BS Electrical Engineering') AS c
    FROM n)
INSERT INTO users (user_id, first_name, last_name, name, email, student_number, course, year_level, role, is_active, is_approved, password, created_by, updated_by, created_time, updated_time)
SELECT 'applicant' + CAST(n AS VARCHAR(10)), f, l, f + ' ' + l, LOWER(f) + '.' + LOWER(l) + '@school.edu',
       '2026-6' + RIGHT('0000' + CAST(n AS VARCHAR(10)), 4), c, 1, 'Student', 1, 0,
       'yCu8wa23pxcND4LfRFKf4A==', 'applicant' + CAST(n AS VARCHAR(10)), 'applicant' + CAST(n AS VARCHAR(10)),
       DATEADD(hour, -5 * n, @now), DATEADD(hour, -5 * n, @now)
FROM s;

-- 9 more organizations (12 in total)
INSERT INTO organizations (name, acronym, description, adviser, is_active, created_by, updated_by) VALUES
 ('Mathematics Society',            'MathSoc', 'Problem-solving sessions and the annual math olympiad.',         'Dr. Elena Ramos',      1, 'admin', 'admin'),
 ('Nursing Students Association',   'NSA',     'Health caravans, review sessions, and community outreach.',       'Prof. Grace Medina',   1, 'admin', 'admin'),
 ('Psychology Circle',              'PsyC',    'Talks and peer-support programs on mental health.',               'Dr. Paolo Serrano',    1, 'admin', 'admin'),
 ('Junior Business Leaders',        'JBL',     'Entrepreneurship workshops and the campus bazaar.',               'Mr. Ramon Uy',         1, 'admin', 'admin'),
 ('Debate Society',                 'DebSoc',  'Parliamentary debate training and inter-school tournaments.',     'Atty. Lea Morales',    1, 'admin', 'admin'),
 ('Environmental Awareness Group',  'EAG',     'Tree planting, coastal clean-ups, and recycling drives.',         'Ms. Joy Pineda',       1, 'admin', 'admin'),
 ('Photography Club',               'PhotoC',  'Photo walks, exhibits, and event coverage.',                      'Mr. Carlo Reyes',      1, 'admin', 'admin'),
 ('Robotics Club',                  'RoboC',   'Builds robots for regional competitions.',                        'Engr. Mark Dizon',     1, 'admin', 'admin'),
 ('Volunteer Corps',                'VC',      'Coordinates student volunteers for school and community events.', 'Ms. Rina Castro',      0, 'admin', 'admin');

-- Members: all demo students join CSS; the new organizations get a few each
INSERT INTO organization_members (organization_id, member_id, position)
SELECT 1, id, 'Member' FROM users WHERE user_id LIKE 'demo%';

INSERT INTO organization_members (organization_id, member_id, position)
SELECT o.id, u.id, CASE WHEN ROW_NUMBER() OVER (PARTITION BY o.id ORDER BY u.id) = 1 THEN 'President' ELSE 'Member' END
FROM organizations o
JOIN users u ON u.user_id LIKE 'demo%' AND (CAST(SUBSTRING(u.user_id, 5, 10) AS INT) % 9) = (o.id % 9)
WHERE o.id > 3;

-- CSS: 10 past (completed) sessions, one a week, plus 2 upcoming open events
WITH n AS (SELECT TOP (10) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects)
INSERT INTO events (organization_id, title, description, venue, start_time, end_time, registration_deadline, capacity, late_after_minutes, status, created_by, updated_by)
SELECT 1,
       CHOOSE(n, 'Git and GitHub Basics','Intro to SQL','UI Design Fundamentals','Python for Data','Algorithms Study Group',
                 'Cybersecurity Awareness','Cloud Computing 101','Mobile App Workshop','Career Talk: QA Testing','Open Source Contribution Day'),
       'Weekly Computer Science Society session.',
       CHOOSE(1 + (n % 3), 'Computer Lab 1','Computer Lab 2','AVR 2, Main Building'),
       DATEADD(hour, 14, DATEADD(day, -7 * n, @today)),
       DATEADD(hour, 16, DATEADD(day, -7 * n, @today)),
       DATEADD(hour, 17, DATEADD(day, -7 * n - 1, @today)),
       40, 15, 'Completed', 'msantos', 'msantos'
FROM n;

INSERT INTO events (organization_id, title, description, venue, start_time, end_time, registration_deadline, capacity, late_after_minutes, status, created_by, updated_by) VALUES
 (1, 'Hour of Code for High Schoolers', 'Members mentor visiting senior high students.', 'Computer Lab 2',
     DATEADD(hour, 10 * 24 + 9, @today), DATEADD(hour, 10 * 24 + 12, @today), DATEADD(hour, 9 * 24 + 17, @today), 25, 15, 'Open', 'msantos', 'msantos'),
 (1, 'CSS General Assembly', 'Semester planning and officer reports.', 'AVR 1, Main Building',
     DATEADD(hour, 12 * 24 + 15, @today), DATEADD(hour, 12 * 24 + 17, @today), DATEADD(hour, 11 * 24 + 17, @today), 0, 15, 'Open', 'msantos', 'msantos');

-- A few open events from the new organizations
INSERT INTO events (organization_id, title, description, venue, start_time, end_time, registration_deadline, capacity, late_after_minutes, status, created_by, updated_by)
SELECT o.id, o.acronym + ' ' + x.t, x.d, x.v,
       DATEADD(hour, 13, DATEADD(day, x.days, @today)),
       DATEADD(hour, 16, DATEADD(day, x.days, @today)),
       DATEADD(hour, 17, DATEADD(day, x.days - 1, @today)),
       60, 15, 'Open', 'admin', 'admin'
FROM organizations o
JOIN (VALUES ('Mathematics Society', 'Math Olympiad Qualifiers', 'Qualifying round for the regional olympiad.', 'Room 301', 5),
             ('Debate Society', 'Novice Debate Workshop', 'Learn parliamentary debate from scratch.', 'Room 210', 6),
             ('Photography Club', 'Campus Photo Walk', 'Golden-hour photo walk around campus.', 'Main Gate', 8)) AS x(org, t, d, v, days)
  ON x.org = o.name;

-- Registrations: Ana + the first 12 demo students attended every past CSS session
INSERT INTO event_registrations (event_id, student_id, registration_code, status, registered_time)
SELECT e.id, u.id, UPPER(LEFT(CONVERT(VARCHAR(32), HASHBYTES('MD5', CONCAT(e.id, '-', u.id)), 2), 10)), 'Registered', DATEADD(day, -3, e.start_time)
FROM events e
JOIN users u ON u.user_id = 'student1' OR (u.user_id LIKE 'demo%' AND CAST(SUBSTRING(u.user_id, 5, 10) AS INT) <= 12)
WHERE e.organization_id = 1 AND e.status = 'Completed' AND e.created_by = 'msantos';

-- Cultural Night (completed) gets 12 more attendees
INSERT INTO event_registrations (event_id, student_id, registration_code, status, registered_time)
SELECT 5, u.id, UPPER(LEFT(CONVERT(VARCHAR(32), HASHBYTES('MD5', CONCAT('5-', u.id)), 2), 10)), 'Registered', DATEADD(day, -14, @now)
FROM users u WHERE u.user_id LIKE 'demo%' AND CAST(SUBSTRING(u.user_id, 5, 10) AS INT) > 12;

-- Present / late / absent mix for every completed event that has no record yet
INSERT INTO attendances (registration_id, check_in_time, check_out_time, status, remarks, recorded_by, recorded_time)
SELECT r.id,
       CASE WHEN r.id % 7 = 0 THEN NULL
            WHEN r.id % 5 = 0 THEN DATEADD(minute, 25, e.start_time)
            ELSE DATEADD(minute, -5, e.start_time) END,
       CASE WHEN r.id % 7 = 0 THEN NULL ELSE e.end_time END,
       CASE WHEN r.id % 7 = 0 THEN 'Absent' WHEN r.id % 5 = 0 THEN 'Late' ELSE 'Present' END,
       CASE WHEN r.id % 5 = 0 AND r.id % 7 <> 0 THEN 'Arrived after the opening' END,
       'msantos', e.end_time
FROM event_registrations r
JOIN events e ON e.id = r.event_id
WHERE e.status = 'Completed'
  AND NOT EXISTS (SELECT 1 FROM attendances a WHERE a.registration_id = r.id);

-- Today's Tech Talk: 14 more registered students waiting at the door
INSERT INTO event_registrations (event_id, student_id, registration_code, status, registered_time)
SELECT 1, u.id, UPPER(LEFT(CONVERT(VARCHAR(32), HASHBYTES('MD5', CONCAT('1-', u.id)), 2), 10)), 'Registered', DATEADD(day, -1, @now)
FROM users u WHERE u.user_id LIKE 'demo%' AND CAST(SUBSTRING(u.user_id, 5, 10) AS INT) BETWEEN 1 AND 14;

-- Four more open events so the card racks run past one page
INSERT INTO events (organization_id, title, description, venue, start_time, end_time, registration_deadline, capacity, late_after_minutes, status, created_by, updated_by)
SELECT o.id, x.t, x.d, x.v,
       DATEADD(hour, 9, DATEADD(day, x.days, @today)),
       DATEADD(hour, 12, DATEADD(day, x.days, @today)),
       DATEADD(hour, 17, DATEADD(day, x.days - 1, @today)),
       80, 15, 'Open', 'admin', 'admin'
FROM organizations o
JOIN (VALUES ('Nursing Students Association', 'Community Health Caravan', 'Free blood-pressure and BMI screening for the barangay.', 'Covered Court', 9),
             ('Psychology Circle', 'Mental Health Week Talk', 'Managing academic stress, with a guest counselor.', 'AVR 2, Main Building', 11),
             ('Junior Business Leaders', 'Campus Bazaar Planning Meeting', 'Booth assignments and vendor briefing.', 'Room 105', 13),
             ('Environmental Awareness Group', 'Coastal Clean-up Drive', 'Bring gloves and a reusable water bottle.', 'Assembly at Main Gate', 15)) AS x(org, t, d, v, days)
  ON x.org = o.name;

-- Ana (student1) signs up for most open events, so her passes and dashboard list run past one page
INSERT INTO event_registrations (event_id, student_id, registration_code, status, registered_time)
SELECT e.id, u.id, UPPER(LEFT(CONVERT(VARCHAR(32), HASHBYTES('MD5', CONCAT('ana-', e.id)), 2), 10)), 'Registered', DATEADD(hour, -6, @now)
FROM events e
JOIN users u ON u.user_id = 'student1'
WHERE e.status = 'Open'
  AND (e.capacity = 0 OR e.capacity > 3)   -- skip the full Hackathon
  AND NOT EXISTS (SELECT 1 FROM event_registrations r WHERE r.event_id = e.id AND r.student_id = u.id);

COMMIT TRANSACTION;
PRINT 'student_event_db is ready.';
GO
