-- =====================================================================
--  Migration 2026-11-13: UC Banilad departments and courses, event course
--  eligibility, admin-assisted "forgot password".
--
--  For a database created by an older student_event_db.sql. Keeps every row;
--  safe to run more than once. Run it while connected to the app's database:
--    sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d student_event_db -i Database\migrations\2026-11-13_departments_eligibility_password_reset.sql
--  (A fresh install only needs student_event_db.sql, which already includes these changes.)
-- =====================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

-- 1. Courses: rename to the UC Banilad program list (ASI.Basecode.Resources/Constants/Courses.cs).
--    Courses Banilad doesn't offer move to the closest program it does.
UPDATE users SET course = CASE course
    WHEN 'BS Computer Science'        THEN 'BS Computer Science (AI-Focused Elective)'
    WHEN 'BS Information Systems'     THEN 'BS Information Technology - Software Engineering'
    WHEN 'BS Civil Engineering'       THEN 'BS Computer Engineering'
    WHEN 'BS Electrical Engineering'  THEN 'BS Computer Engineering'
    WHEN 'BS Mechanical Engineering'  THEN 'BS Computer Engineering'
    WHEN 'BS Business Administration' THEN 'BS Business Administration - Marketing Management'
    WHEN 'AB Communication'           THEN 'BS Criminology'
    WHEN 'AB Political Science'       THEN 'BS Medical Technology'
    WHEN 'BSEd - Secondary Education' THEN 'BSEd - English'
    ELSE course END
WHERE course IN ('BS Computer Science', 'BS Information Systems', 'BS Civil Engineering', 'BS Electrical Engineering',
                 'BS Mechanical Engineering', 'BS Business Administration', 'AB Communication', 'AB Political Science',
                 'BSEd - Secondary Education');
PRINT CONCAT('Courses renamed: ', @@ROWCOUNT, ' user(s).');
GO

-- 2. Users who must replace a temporary password set by an admin.
IF COL_LENGTH('users', 'must_change_password') IS NULL
    ALTER TABLE users ADD must_change_password BIT NOT NULL CONSTRAINT df_users_must_change_password DEFAULT 0;
GO

-- 3. Courses allowed to register for an event. No rows = open to all students.
IF OBJECT_ID('event_allowed_courses', 'U') IS NULL
BEGIN
    CREATE TABLE event_allowed_courses (
        id       INT IDENTITY(1,1) PRIMARY KEY,
        event_id INT           NOT NULL CONSTRAINT fk_allowed_courses_event REFERENCES events (id) ON DELETE CASCADE,
        course   NVARCHAR(100) NOT NULL,
        CONSTRAINT uq_event_allowed_courses UNIQUE (event_id, course)
    );
END
GO

-- 4. "Forgot password" requests.
IF OBJECT_ID('password_reset_requests', 'U') IS NULL
BEGIN
    CREATE TABLE password_reset_requests (
        id             INT IDENTITY(1,1) PRIMARY KEY,
        user_id        INT          NOT NULL CONSTRAINT fk_password_resets_user REFERENCES users (id) ON DELETE CASCADE,
        requested_time DATETIME2    NOT NULL CONSTRAINT df_password_resets_requested DEFAULT SYSDATETIME(),
        status         NVARCHAR(20) NOT NULL CONSTRAINT df_password_resets_status DEFAULT 'Pending',
        handled_by     NVARCHAR(50) NULL,
        handled_time   DATETIME2    NULL,
        CONSTRAINT ck_password_resets_status CHECK (status IN ('Pending', 'Completed', 'Dismissed'))
    );
    CREATE UNIQUE INDEX uq_password_resets_pending ON password_reset_requests (user_id) WHERE status = 'Pending';
END
GO

PRINT 'Migration 2026-11-13 applied.';
GO
