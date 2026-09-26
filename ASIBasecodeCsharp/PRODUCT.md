# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- **Students** browse upcoming events from their school's student organizations, register (or join a waitlist when an event is full), and show a QR ticket at the door. Mostly on their phones, between classes.
- **Officers** of a student organization create and run their organization's events and take attendance at the venue entrance by scanning ticket QR codes, often on a phone held in one hand while a queue forms.
- **Admins** (student affairs / system administrators) manage organizations, members, users and roles, and read attendance reports. Mostly on laptops.

## Product Purpose

A school's hub for student-organization events: publish an event, let students register, and record who actually attended, fast enough to keep a line moving at the door. Success is an accurate attendance record per event with no paper sign-in sheets.

## Operating Context

- Event lifecycle: Draft → Open (registration) → Closed → Completed, or Cancelled. Students never see drafts.
- Capacity with automatic waitlist; the earliest waitlisted student is promoted when a slot opens.
- Check-in happens at a venue door: camera QR scan, or typing the 10-character registration code or student number. Late threshold per event; completing an event marks no-shows Absent.
- Reports and CSV exports feed student-affairs record keeping; tickets and reports may be printed.

## Capabilities and Constraints

- ASP.NET Core MVC with Razor views (.NET 9), Bootstrap 4.3 and jQuery shipped locally, PostgreSQL. Server-rendered pages; no SPA framework.
- Roles: Admin, Officer (scoped to organizations they belong to), Student.
- Working product name: "Student Org Events" is descriptive only; no official name has been chosen (open decision).

## Brand Commitments

- No Alliance Software / ASI branding anywhere in the interface: no ASI logo, LiveIT artwork, or Alliance copyright. (Internal C# namespaces `ASI.Basecode.*` are code-only and stay.)
- The product belongs to a school's student organizations, not a vendor. No specific school identity has been provided; do not invent a school name, crest, or motto.

## Evidence on Hand

- Demo data in `Database/student_event_db.sql` (sample organizations, events, students). It is illustrative, not real usage data.
- No real photos, testimonials, statistics, or school assets exist; do not fabricate them.

## Product Principles

1. The door is the hardest moment: check-in must work one-handed on a phone, in a hurry, in bad light.
2. A student should always know their standing for an event (registered, waitlisted, checked in) at a glance.
3. Officers manage only their own organizations; the interface should make scope obvious.
4. Records are official: attendance and reports must read as trustworthy and print cleanly.

## Accessibility & Inclusion

Students use a wide range of low-end phones and outdoor/venue lighting; keep text contrast high (WCAG AA minimum) and touch targets large.
