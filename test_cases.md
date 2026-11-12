# Test Cases: Student Organization Attendance and Event Registration System

ASP.NET Core 9 MVC. Test cases are derived from the business rules in `UserService`, `EventService`, `RegistrationService`, `AttendanceService`, `AccessService`, `OrganizationService` and `ReportService`.

**Status legend:** `Pending` (not yet run), `Pass`, `Fail`. Update the Status column during the walkthrough.

## Contents

1. [Authentication and Access Control](#1-authentication-and-access-control)
2. [Student Flow](#2-student-flow)
3. [Admin Flow](#3-admin-flow)
4. [Officer Flow](#4-officer-flow)
5. [Cross-cutting and Data Integrity](#5-cross-cutting-and-data-integrity)
6. [Coverage Summary](#coverage-summary)
7. [Defense Walkthrough](#defense-walkthrough)

## Key Rules (quick reference)

| Item | Rule |
|---|---|
| Roles | Admin, Officer (scoped to organizations they are a member of), Student |
| Self-registration | Always creates an **active, unapproved Student**. The account cannot log in until an Admin approves it. |
| Event lifecycle | Draft → Open → Closed → Completed, or Cancelled. Allowed: Draft→Open/Cancelled; Open→Closed/Completed/Cancelled; Closed→Open/Completed/Cancelled. Completed and Cancelled are final. |
| Registration | Open events only, before the registration deadline and before the start time. If capacity > 0 and registered count ≥ capacity, the student is **Waitlisted**. Capacity 0 means unlimited. |
| Registration code | 10 characters, no `0/O/1/I`, unique. The QR pass is shown only for `Registered` status. |
| Cancel registration | Allowed only if not already cancelled, not yet checked in, event is Open/Closed, and the event has not started. |
| Waitlist | The earliest waitlisted student (by registered time) is promoted when a registered student cancels or an officer raises capacity. |
| Check-in | Allowed only while the event is Open or Closed. **Present** if scanned at or before `StartTime + LateAfterMinutes`, otherwise **Late**. Accepts a registration code or student number. |
| Completing an event | Registered students with no attendance record are automatically marked **Absent**. |

---

## 1. Authentication and Access Control

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| AUTH-01 | Login with valid, approved, active account | Approved and active user exists | 1. Open Login.<br>2. Enter valid username and password.<br>3. Submit. | Login succeeds and the user lands on the Home dashboard for their role. | Pending |
| AUTH-02 | Login with wrong password | User exists | 1. Enter valid username with a wrong password.<br>2. Submit. | Error: "Incorrect username or password." User stays on the Login page. | Pending |
| AUTH-03 | Login with unknown username | — | 1. Enter a non-existent username.<br>2. Submit. | Same error as AUTH-02, so account existence is not revealed. | Pending |
| AUTH-04 | Pending-approval account is blocked | Student has self-registered and is not yet approved | Log in with the new student's correct credentials. | Login blocked: "Your account is waiting for administrator approval. You can sign in once it's approved." | Pending |
| AUTH-05 | Deactivated account is blocked | Approved user was deactivated by Admin | Log in with that user's correct credentials. | Login blocked: "Your account has been deactivated. Please contact an administrator." | Pending |
| AUTH-06 | Sign out | Logged in | Click Sign out. | Session ends and the user returns to Login. | Pending |
| AUTH-07 | Unauthenticated access to protected page | Not logged in | Open `/Event/Manage` or `/User` directly. | Redirected to Login. | Pending |
| AUTH-08 | Student cannot reach manager or admin pages | Logged in as Student | Open `/User`, `/Organization`, `/Report`, `/Event/Manage`. | Access Denied page each time. | Pending |
| AUTH-09 | Officer cannot reach admin-only pages | Logged in as Officer | Open `/User` and `/Organization`. | Access Denied. | Pending |
| AUTH-10 | Dev account switch disabled outside Development | App run in Production environment | POST `/Account/SwitchAccount`. | Returns 404. (Skip if only Development is available.) | Pending |

---

## 2. Student Flow

### 2.1 Sign-up and Approval Gate

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| STU-01 | Successful self-registration | Valid, unused username and student number | 1. Open Register.<br>2. Fill username, first/last name, school email, student number, course, year level, password and confirmation.<br>3. Submit. | Account created as Role = Student, Active, **not approved**. User is told approval is required. | Pending |
| STU-02 | Required fields enforced | — | Submit the Register form with required fields empty (username, first name, last name, email, student number, course, year level, password, confirm). | Each required field shows its validation message and no account is created. | Pending |
| STU-03 | Duplicate username rejected | Username already exists | Register using an existing username. | Error: "UserId already exists!" | Pending |
| STU-04 | Duplicate student number rejected | Student number already used | Register using an existing student number. | Error: "Student number is already registered." | Pending |
| STU-05 | Course must come from the list | — | Submit with a course not in the list. | Error: "Please choose a course from the list." | Pending |
| STU-06 | Password confirmation mismatch | — | Enter different Password and Confirm Password. | Validation error and no account is created. | Pending |
| STU-07 | Full name composed correctly | Middle name optional | 1. Register with first, middle and last name.<br>2. View in Admin user list. | Display name is "First Middle Last". With no middle name it is "First Last". | Pending |
| STU-08 | Student login after approval | Admin approved the account (ADM-02) | Log in with the new credentials. | Login succeeds. | Pending |

### 2.2 Browsing Events

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| STU-09 | Browse upcoming events | Events exist in Open, Closed, Completed and Draft statuses | 1. Log in as Student.<br>2. Open Events. | Draft events are **not** listed. Cancelled and already-ended events are hidden under the default (upcoming only) view. | Pending |
| STU-10 | Show past events | Past events exist | Toggle "Show past". | Ended and cancelled non-draft events also appear. | Pending |
| STU-11 | Search events | Events with different titles, venues and organizations | 1. Search by title.<br>2. Search by venue.<br>3. Search by organization name or acronym. | List filters correctly for each search, ignoring case. | Pending |
| STU-12 | Draft event is hidden by direct URL | A Draft event exists | As Student, open `/Event/Details/{draftId}`. | "The requested record was not found." (Not Found) | Pending |
| STU-13 | Student sees no manage controls | Open event exists | Open Event Details as Student. | No Edit, Delete, Status or Attendance buttons are shown. | Pending |
| STU-14 | Dashboard summary for Student | Student has registrations | Open Home. | Shows upcoming registrations, number attended (Present or Late only), and count of events open for registration. | Pending |

### 2.3 Registering, Waitlist and Cancellation

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| STU-15 | Register for an open event | Event is Open, before deadline, has free slots | 1. Open Event Details.<br>2. Click Register. | Status = **Registered**, success message shown, unique 10-character code generated. Event appears in "My Events". | Pending |
| STU-16 | Registration code format | Registered student | Inspect several codes. | Each is 10 characters and contains no `0`, `O`, `1` or `I`. | Pending |
| STU-17 | Duplicate registration blocked | Student already Registered or Waitlisted | Try to register again. | Error: "You are already registered for this event." | Pending |
| STU-18 | Event full goes to waitlist | Capacity = N and N students already Registered | Register one more student. | Status = **Waitlisted** with a waitlist warning. No QR pass is shown. | Pending |
| STU-19 | Unlimited capacity | Event capacity = 0 | Register many students. | Nobody is ever waitlisted. | Pending |
| STU-20 | Registration after deadline | Event Open but `RegistrationDeadline` has passed | Try to register. | Error: "Registration for this event is closed." | Pending |
| STU-21 | Registration after event start | Event Open but `StartTime` has passed | Try to register. | Error: "Registration for this event is closed." | Pending |
| STU-22 | Registration on a non-Open event | Event is Closed, Completed or Cancelled | Try to register. | Error: "Registration for this event is closed." | Pending |
| STU-23 | Registration on Draft event | Draft event | POST a register request for the draft event id. | Not Found. | Pending |
| STU-24 | Cancel a registration | Registered, event Open/Closed, not started, not checked in | 1. Open My Events.<br>2. Click Cancel. | Status = **Cancelled**, cancelled time stored. | Pending |
| STU-25 | Waitlist auto-promotion on cancel | Event full with at least 2 students on the waitlist | A Registered student cancels. | The **earliest** waitlisted student (by registered time) becomes Registered; the next stays Waitlisted. | Pending |
| STU-26 | Cancelling a waitlisted entry does not promote | Waitlisted student exists | Waitlisted student cancels. | Status = Cancelled and no one else is promoted. | Pending |
| STU-27 | Cancel not allowed after check-in | Student already checked in | Try to cancel. | Error: "This registration can no longer be cancelled." (Cancel button hidden.) | Pending |
| STU-28 | Cancel not allowed after event start | Event already started | Try to cancel. | Cancel not allowed (same error as STU-27). | Pending |
| STU-29 | Re-register after cancelling | Student previously cancelled | Register again for the same event. | Same registration row is reused (one row per student per event), status Registered or Waitlisted depending on capacity. | Pending |
| STU-30 | Cancelled student cannot re-register when closed | Student cancelled, event now Closed | Try to register. | Error: "Registration for this event is closed." | Pending |

### 2.4 QR Pass

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| STU-31 | QR pass renders for Registered | Registered student | 1. Open My Events.<br>2. Open Ticket. | Ticket page shows event details, the registration code and a QR image encoding the code. | Pending |
| STU-32 | No QR for Waitlisted or Cancelled | Waitlisted and Cancelled registrations | Open each ticket. | No QR code is rendered. The status is displayed instead. | Pending |
| STU-33 | Ticket is private to the owner | Two students with registrations | As Student B, open Student A's ticket URL. | Refused: "You are not allowed to manage this record." | Pending |
| STU-34 | Ticket is printable | Registered student | Use browser Print on the Ticket page. | Print layout is clean and the QR remains scannable. | Pending |
| STU-35 | Attendance status visible to Student | Student was checked in (Present or Late) | Open My Events. | Standing (Registered, Waitlisted, Present, Late, Absent) shows correctly. | Pending |

---

## 3. Admin Flow

### 3.1 User Management and Sign-up Approvals

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| ADM-01 | Pending approvals list | Several unapproved sign-ups | 1. Log in as Admin.<br>2. Open Approvals. | Pending users listed oldest first; pending count badge matches. | Pending |
| ADM-02 | Approve selected accounts | Pending users exist | 1. Tick some users.<br>2. Click Approve. | Selected users approved, with approver and time recorded. Others stay pending. | Pending |
| ADM-03 | Approve all | Several pending users | Click Approve All. | All pending users approved and the count is shown. | Pending |
| ADM-04 | Approve with nothing selected | — | Click Approve with no selection. | Error: "Select at least one account first." | Pending |
| ADM-05 | Reject pending sign-ups | Pending users exist | 1. Tick users.<br>2. Click Reject. | Selected pending requests are deleted. The user can no longer log in. | Pending |
| ADM-06 | Reject never deletes approved accounts | An approved user's id is submitted to Reject | Submit Reject with an approved user's id. | Approved account untouched (0 rejected). | Pending |
| ADM-07 | Reject with nothing selected | — | Click Reject with no selection. | Error: "Select at least one account first." | Pending |
| ADM-08 | Admin creates a user | Admin logged in | 1. Users → Create.<br>2. Fill details and choose a role (Admin, Officer or Student).<br>3. Save. | User created already **approved** and can log in immediately. | Pending |
| ADM-09 | Create user requires password | — | Create a user with blank password. | Error: "Password is required." | Pending |
| ADM-10 | Create user duplicates blocked | Existing username or student number | 1. Create a user with a duplicate username.<br>2. Repeat with a duplicate student number. | Errors: "UserId already exists!" and "Student number is already registered." | Pending |
| ADM-11 | Edit user keeps password when blank | Existing user | 1. Edit a user and leave Password blank.<br>2. Save.<br>3. Log in with the old password. | Details update and the old password still works. | Pending |
| ADM-12 | Edit user changes password | Existing user | 1. Edit and enter a new password.<br>2. Log in with the new password. | New password works and the old one fails. | Pending |
| ADM-13 | Search and filter users | Mixed users | 1. Search by username, name and student number.<br>2. Filter by role. | Results match, ordered by role then name. | Pending |
| ADM-14 | Deactivate and reactivate | Active user | 1. Toggle Active off.<br>2. Try to log in.<br>3. Toggle on.<br>4. Log in again. | Login blocked while inactive, then allowed again. | Pending |
| ADM-15 | Admin cannot deactivate self | Admin logged in | Toggle Active on own account. | Blocked: "You are not allowed to manage this record." | Pending |

### 3.2 Organization Management

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| ADM-16 | Create organization | Admin logged in | 1. Organizations → Create.<br>2. Enter name, acronym, description, adviser.<br>3. Save. | Organization appears in the list with 0 members and 0 events. | Pending |
| ADM-17 | Duplicate organization name | Organization exists | Create another with the same name (any letter case). | Error: "An organization with the same name already exists." | Pending |
| ADM-18 | Edit organization | Organization exists | 1. Edit name, description, adviser.<br>2. Save. | Changes saved. Renaming to another organization's name is rejected. | Pending |
| ADM-19 | Search organizations | Several organizations | Search by name and by acronym. | List filters correctly. | Pending |
| ADM-20 | Delete organization with no events | Organization has no events | Delete it. | Organization is removed. | Pending |
| ADM-21 | Delete organization with events | Organization has at least one event | Try to delete. | Error: "Organization cannot be deleted because it already has events." | Pending |

### 3.3 Appointing Officers and Members

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| ADM-22 | Add member to organization | Approved, active user not yet a member | 1. Open Organization → Members.<br>2. Choose user, enter position.<br>3. Add. | Member listed with the position. Blank position defaults to "Member". | Pending |
| ADM-23 | Available users list is filtered | Pending, inactive and existing-member users exist | Open the Add Member dropdown. | Only **active and approved** users who are not already members are offered. | Pending |
| ADM-24 | Duplicate membership blocked | User already a member | Add the same user again. | Error: "User is already a member of this organization." | Pending |
| ADM-25 | Appoint officer | Student user | 1. Edit the user and set Role = Officer.<br>2. Add the user as a member of an organization. | User logs in as Officer and can manage **only** that organization's events. | Pending |
| ADM-26 | Officer without membership manages nothing | Officer not in any organization | 1. Log in as that Officer.<br>2. Open Event Manage and Create. | Manage list is empty, and the organization dropdown on Create is empty. | Pending |
| ADM-27 | Update member position | Member exists | 1. Change position.<br>2. Save. | Position updates. Blank resets to "Member". | Pending |
| ADM-28 | Remove member | Member exists | Remove the member. | Member removed. The officer loses management access to that organization. | Pending |

### 3.4 System-wide Oversight and Reporting

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| ADM-29 | Admin manages all organizations | Events in multiple organizations | Open Event Manage as Admin. | Events from **all** organizations listed, newest start time first, filterable by status and search. | Pending |
| ADM-30 | Admin dashboard figures | Sample data loaded | Open Home as Admin. | Shows organization count, pending approvals, active students, active (Open/Closed) events, total registrations and overall attendance rate. | Pending |
| ADM-31 | Overall attendance rate | Mix of Present, Late, Absent and unrecorded | Compare dashboard rate with a manual count. | Rate = (Present + Late) ÷ recorded attendance records × 100, rounded to 1 decimal. 0 if nothing is recorded. | Pending |
| ADM-32 | Event summary report | Non-draft events with attendance | Reports → Index. | Lists non-draft events with Registered, Present, Late, Absent counts and attendance rate. Drafts excluded. | Pending |
| ADM-33 | Filter report by organization | Multiple organizations | Select an organization filter. | Only that organization's events appear. | Pending |
| ADM-34 | Export summary CSV (Admin) | Report has data | Click Export Summary. | `event_summary_yyyyMMdd_HHmm.csv` downloads with Event, Organization, Date, Status, Registered, Present, Late, Absent, AttendanceRatePercent. | Pending |
| ADM-35 | Admin can run attendance on any event | Event from any organization | Open Attendance for the event and check in a student. | Allowed, since Admin can manage every organization. | Pending |

---

## 4. Officer Flow

### 4.1 Event Creation, Visibility and Status

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| OFF-01 | Create event starts as Draft | Officer is a member of an organization | 1. Event → Create.<br>2. Fill organization, title, description, venue, start, end, registration deadline, capacity, late-after minutes.<br>3. Save. | Event created with status **Draft**. | Pending |
| OFF-02 | Draft is hidden from students | Draft event exists | Log in as Student and open Events. | Draft not listed; its direct URL returns Not Found. | Pending |
| OFF-03 | Draft is visible to its officer | Draft event of officer's organization | Open Event Manage and Details as the officer. | Draft listed and viewable with manage controls. | Pending |
| OFF-04 | Draft hidden from other organization's officer | Draft belongs to Org A; officer is only in Org B | Open the draft URL as the Org B officer. | Not Found. | Pending |
| OFF-05 | Publish: Draft → Open | Draft event | Change status to Open. | Event becomes visible to students and accepts registrations. | Pending |
| OFF-06 | Validation: end after start | Create form | Set end time earlier than or equal to start time. | Error: "Event end time must be later than the start time." | Pending |
| OFF-07 | Validation: deadline before start | Create form | Set registration deadline after the start time. | Error: "Registration deadline must be on or before the event start time." | Pending |
| OFF-08 | Required fields | Create form | Leave title, venue or dates empty. | Validation messages shown; no event created. | Pending |
| OFF-09 | Officer limited to own organizations | Officer in Org A only | 1. Open Create.<br>2. Try posting an event for Org B. | Org B not in the dropdown; a forged post is refused: "You are not allowed to manage this record." | Pending |
| OFF-10 | Allowed status transitions | Events in each status | Try Draft→Open, Draft→Cancelled, Open→Closed, Open→Completed, Open→Cancelled, Closed→Open, Closed→Completed, Closed→Cancelled. | All succeed. | Pending |
| OFF-11 | Disallowed status transitions | Events in each status | Try Draft→Closed, Draft→Completed, Open→Draft, and any change from Completed or Cancelled. | Error: "The event status cannot be changed to the selected value." | Pending |
| OFF-12 | Close registration | Open event | 1. Change to Closed.<br>2. Student tries to register. | Registration refused ("Registration for this event is closed."). Check-in still allowed. | Pending |
| OFF-13 | Edit event details | Event with registrations | 1. Edit title, venue or times.<br>2. Save. | Changes saved. | Pending |
| OFF-14 | Capacity cannot drop below registered count | 5 students Registered | Edit capacity to 3. | Error: "Capacity cannot be lower than the number of registered attendees." | Pending |
| OFF-15 | Raising capacity promotes waitlist | Event full with 2 on waitlist | 1. Increase capacity by 2.<br>2. Save. | Both waitlisted students promoted to Registered in order of registration time. | Pending |
| OFF-16 | Set capacity to 0 (unlimited) | Event with waitlist | Edit capacity to 0. | All waitlisted students are promoted. | Pending |
| OFF-17 | Delete event with no registrations | Event has no registrations | Delete. | Event is deleted. | Pending |
| OFF-18 | Delete event with registrations | Event has registrations | Try to delete. | Error: "Event cannot be deleted because it already has registrations. Cancel the event instead." | Pending |
| OFF-19 | Cancel an event | Open event | Change status to Cancelled. | Event cancelled. Students see it only with "Show past" on; it cannot be reopened. | Pending |
| OFF-20 | Officer cannot touch another organization's event | Event belongs to another organization | Try Edit, Delete or ChangeStatus by URL. | Refused: "You are not allowed to manage this record." | Pending |

### 4.2 Door Check-in (Present vs Late)

Use an event with `StartTime = T` and `LateAfterMinutes = L`. The Late threshold is `T + L`.

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| OFF-21 | Attendance sheet loads | Event with Registered and Waitlisted students | Open Attendance for the event. | Registered attendees listed with counts for Present, Late, Absent and Pending. Waitlist shown separately. Cancelled excluded. | Pending |
| OFF-22 | Check in by code, on time | Event Open, current time ≤ T + L | Enter the student's registration code (or scan QR). | Success message with student name and time. Status = **Present**. | Pending |
| OFF-23 | Check in after the late threshold | Current time > T + L | Check in a different registered student. | Status = **Late**. | Pending |
| OFF-24 | Check in exactly at the threshold | Current time = T + L | Check in. | Status = **Present** (Late only when strictly later than T + L). | Pending |
| OFF-25 | Check in by student number | Registered student | Enter the student number instead of the code. | Same result as OFF-22 or OFF-23. | Pending |
| OFF-26 | Code input is case-insensitive and trimmed | Registered student | Enter the code in lowercase with extra spaces. | Check-in succeeds. | Pending |
| OFF-27 | Check in before the event start | Event Open, current time before T | Check in a student. | Status = **Present** (early arrivals are on time). | Pending |
| OFF-28 | Invalid code | — | Enter a random code or an unregistered student number. | Failure: "No registration matches that code or student number for this event." | Pending |
| OFF-29 | Code from a different event | Student registered for another event | Enter that code here. | Same failure as OFF-28. | Pending |
| OFF-30 | Waitlisted or cancelled student | One waitlisted and one cancelled student | Try to check in each. | Failure: "This registration is not active (cancelled or waitlisted)." | Pending |
| OFF-31 | Duplicate check-in | Student already checked in | Scan the same code again. | Message: "Attendee is already checked in." Shows existing status and time; original record unchanged. | Pending |
| OFF-32 | Check-in while event not Open/Closed | Draft, Completed or Cancelled event | Try to check in. | Error: "Attendance can only be recorded while the event is Open or Closed." | Pending |
| OFF-33 | Officer outside the organization | Officer of another organization | Open Attendance for this event. | Refused: "You are not allowed to manage this record." | Pending |
| OFF-34 | Check-out | Student checked in | Click Check-out. | Check-out time recorded. | Pending |
| OFF-35 | Check-out without check-in | Registered, not checked in | Try to check out. | Error: "Attendee has not checked in yet." | Pending |
| OFF-36 | Manual status: Present or Late | Registered, no record | Set status Present (or Late) with remarks. | Record created with check-in time = now and remarks saved. | Pending |
| OFF-37 | Manual status: Absent | Student already checked in | Set status Absent. | Check-in and check-out times cleared; status = Absent. | Pending |
| OFF-38 | Absent student arrives later | Student manually marked Absent | Check in with their code. | Existing record updated to Present or Late with a new check-in time. | Pending |
| OFF-39 | Manual status on non-active registration | Waitlisted or cancelled | Try to set status. | Error: "This registration is not active (cancelled or waitlisted)." | Pending |
| OFF-40 | Manual status on Draft or Cancelled event | Draft or Cancelled event | Try to set status. | Error: "Attendance can only be recorded while the event is Open or Closed." | Pending |
| OFF-41 | Manual status allowed after Completed | Completed event; officer corrects a record | Set a student's status. | Allowed (only Draft and Cancelled events block manual correction). | Pending |

### 4.3 Event Completion (auto-Absent)

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| OFF-42 | Completing marks no-shows Absent | Open/Closed event with 5 Registered; 3 checked in, 2 not | Change status to Completed. | The 2 students without a record are marked **Absent** (recorded by the officer). The 3 checked-in keep their status. | Pending |
| OFF-43 | Waitlisted and cancelled not marked | Event has waitlisted and cancelled registrations | Complete the event. | No attendance records created for them. | Pending |
| OFF-44 | Existing records preserved | Some students already marked Late or manually Absent | Complete the event. | Existing attendance rows are not overwritten. | Pending |
| OFF-45 | Completed event is final | Completed event | Try to change status to any value. | Error: "The event status cannot be changed to the selected value." | Pending |
| OFF-46 | Check-in blocked after completion | Completed event | Try to check in a student. | Error: "Attendance can only be recorded while the event is Open or Closed." | Pending |
| OFF-47 | Complete directly from Closed | Closed event | Change to Completed. | Allowed; absentees marked as in OFF-42. | Pending |

### 4.4 Reports and CSV Export

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| OFF-48 | Officer sees only own organizations' reports | Officer in Org A; events in Org A and Org B | Open Reports. | Only Org A non-draft events shown. | Pending |
| OFF-49 | Event report page | Event with attendance data | Open Report → Event. | Shows attendees with registration code, status, check-in/check-out times and remarks. | Pending |
| OFF-50 | Export event attendance CSV | Event with mixed attendance | Click Export on the event report. | Downloads `attendance_{eventId}_{yyyyMMdd_HHmm}.csv` with columns: StudentNumber, Name, Course, YearLevel, RegistrationCode, RegisteredTime, Attendance, CheckIn, CheckOut, Remarks. | Pending |
| OFF-51 | CSV content accuracy | Same event as OFF-50 | Open the CSV in Excel. | One row per **Registered** student. Attendance shows Present, Late, Absent or "Not yet recorded". Times formatted `yyyy-MM-dd HH:mm`. Waitlisted and cancelled excluded. | Pending |
| OFF-52 | Export summary CSV (Officer) | Officer with events | Click Export Summary. | CSV contains only the officer's organizations' events (no drafts) with attendance rate. | Pending |
| OFF-53 | Export another organization's event | Event belongs to another organization | Request its export URL. | Refused: "You are not allowed to manage this record." No file returned. | Pending |
| OFF-54 | Student cannot export | Logged in as Student | Open `/Report/ExportEvent/{id}`. | Access Denied. | Pending |
| OFF-55 | Officer dashboard | Officer with events | Open Home as Officer. | Counts scoped to the officer's organizations; pending approvals shows 0 (non-Admin). | Pending |

---

## 5. Cross-cutting and Data Integrity

| ID | Scenario | Setup | Steps | Expected Result | Status |
|---|---|---|---|---|---|
| X-01 | One registration row per student per event | Student registers, cancels and re-registers | Inspect the `EventRegistration` table. | Only one row exists for the student and event. | Pending |
| X-02 | Registration codes are unique | Many registrations | Check the table for duplicate codes. | No duplicates. | Pending |
| X-03 | Passwords not stored in plain text | Any user | Inspect the `User` table. | Password column holds an encrypted/hashed value, not the typed password. | Pending |
| X-04 | Audit fields populated | Create or update records | Inspect CreatedBy, UpdatedBy and times for users, events and organizations. | Fields hold the acting username and current time. | Pending |
| X-05 | CSRF protection on POST actions | Logged in | Submit a POST (register, cancel, check-in) without an anti-forgery token. | Request is rejected. | Pending |
| X-06 | Mobile usability at the door | Phone browser | 1. Open the Attendance page on a phone.<br>2. Check in with camera or typed code. | Usable one-handed, with large touch targets and readable text. | Pending |

---

## Coverage Summary

| Area | Test IDs | Count |
|---|---|---|
| Authentication and access | AUTH-01 to AUTH-10 | 10 |
| Student flow | STU-01 to STU-35 | 35 |
| Admin flow | ADM-01 to ADM-35 | 35 |
| Officer flow | OFF-01 to OFF-55 | 55 |
| Cross-cutting | X-01 to X-06 | 6 |
| **Total** | | **141** |

## Defense Walkthrough

Suggested order, about 10 minutes.

| # | Tests | What it shows |
|---|---|---|
| 1 | AUTH-04, STU-01 | Student signs up and is blocked at login. |
| 2 | ADM-02, STU-08 | Admin approves; student logs in. |
| 3 | ADM-16, ADM-22, ADM-25 | Admin creates an organization and appoints an officer. |
| 4 | OFF-01, OFF-02, OFF-05 | Officer creates a Draft (hidden from students), then publishes it. |
| 5 | STU-15, STU-18, STU-25 | Student registers, a full event waitlists, and a cancellation promotes the waitlist. |
| 6 | STU-31 | QR pass appears for a Registered student only. |
| 7 | OFF-22, OFF-23, OFF-31 | Door check-in shows Present, Late and a duplicate scan. |
| 8 | OFF-42 | Completing the event marks no-shows Absent. |
| 9 | OFF-50, ADM-32 | CSV export and system-wide report. |
