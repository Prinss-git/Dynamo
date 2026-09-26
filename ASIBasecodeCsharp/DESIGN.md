---
name: Student Org Events
description: School ID passes on a laminate desk; plain ledgers at the registrar's desk.
colors:
  desk: "#e5e9ef"
  card: "#ffffff"
  card-2: "#f4f6f9"
  navy: "#0e2a47"
  navy-2: "#1b4470"
  navy-ink: "#cfdbe8"
  rail-muted: "#9fb3c8"
  gold: "#f2b705"
  gold-deep: "#8a6400"
  gold-wash: "#fff4cc"
  ink: "#142033"
  muted: "#526073"
  rule: "#d5dbe3"
  rule-strong: "#b9c3cf"
  ok: "#17693f"
  ok-wash: "#e2f3e9"
  warn: "#8f5200"
  warn-wash: "#fdf0d8"
  bad: "#a8231c"
  bad-wash: "#fbe4e2"
  idle: "#55606e"
  idle-wash: "#eceff3"
typography:
  display:
    fontFamily: "Barlow Condensed, Barlow, system-ui, sans-serif"
    fontSize: "clamp(1.9rem, 1.4rem + 1.6vw, 2.6rem)"
    fontWeight: 700
    lineHeight: 1.02
    letterSpacing: "-0.005em"
  headline:
    fontFamily: "Barlow Condensed, Barlow, system-ui, sans-serif"
    fontSize: "clamp(1.5rem, 1.2rem + 1vw, 2rem)"
    fontWeight: 700
    lineHeight: 1.05
  title:
    fontFamily: "Barlow Condensed, Barlow, system-ui, sans-serif"
    fontSize: "1.3rem"
    fontWeight: 700
    lineHeight: 1.1
  figure:
    fontFamily: "Barlow Condensed, Barlow, system-ui, sans-serif"
    fontSize: "1.9rem"
    fontWeight: 700
    lineHeight: 1
    fontFeature: "tnum"
  body:
    fontFamily: "Barlow, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.5
  label:
    fontFamily: "Barlow Condensed, Barlow, system-ui, sans-serif"
    fontSize: "0.78rem"
    fontWeight: 600
    lineHeight: 1.2
    letterSpacing: "0.08em"
  code:
    fontFamily: "ui-monospace, Cascadia Mono, SFMono-Regular, Consolas, monospace"
    fontSize: "1.45rem"
    fontWeight: 600
    letterSpacing: "0.28em"
rounded:
  tag: "6px"
  control: "8px"
  sheet: "10px"
  pass: "14px"
  idcard: "18px"
spacing:
  xs: "8px"
  sm: "12px"
  md: "16px"
  lg: "20px"
  xl: "24px"
  gutter: "40px"
components:
  button-primary:
    backgroundColor: "{colors.navy}"
    textColor: "{colors.card}"
    rounded: "{rounded.control}"
    padding: "0 18px"
    height: "44px"
  button-primary-hover:
    backgroundColor: "{colors.navy-2}"
    textColor: "{colors.card}"
  button-key:
    backgroundColor: "{colors.gold}"
    textColor: "{colors.navy}"
    rounded: "{rounded.control}"
    padding: "0 18px"
    height: "44px"
  button-outline:
    backgroundColor: "{colors.card}"
    textColor: "{colors.navy}"
    rounded: "{rounded.control}"
    padding: "0 18px"
    height: "44px"
  button-danger:
    backgroundColor: "{colors.card}"
    textColor: "{colors.bad}"
    rounded: "{rounded.control}"
    padding: "0 18px"
    height: "44px"
  button-danger-hover:
    backgroundColor: "{colors.bad-wash}"
    textColor: "{colors.bad}"
  button-sm:
    rounded: "7px"
    padding: "0 12px"
    height: "36px"
  input:
    backgroundColor: "{colors.card}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "8px 12px"
    height: "44px"
  tag-ok:
    backgroundColor: "{colors.ok-wash}"
    textColor: "{colors.ok}"
    typography: "{typography.label}"
    rounded: "{rounded.tag}"
    padding: "0 10px"
    height: "26px"
  tag-warn:
    backgroundColor: "{colors.warn-wash}"
    textColor: "{colors.warn}"
    rounded: "{rounded.tag}"
    height: "26px"
  tag-bad:
    backgroundColor: "{colors.bad-wash}"
    textColor: "{colors.bad}"
    rounded: "{rounded.tag}"
    height: "26px"
  tag-idle:
    backgroundColor: "{colors.idle-wash}"
    textColor: "{colors.idle}"
    rounded: "{rounded.tag}"
    height: "26px"
  tag-final:
    backgroundColor: "{colors.navy}"
    textColor: "{colors.card}"
    rounded: "{rounded.tag}"
    height: "26px"
  sheet:
    backgroundColor: "{colors.card}"
    rounded: "{rounded.sheet}"
    padding: "20px"
  ledger-header:
    backgroundColor: "{colors.card-2}"
    textColor: "{colors.navy}"
    padding: "10px 14px"
  pass:
    backgroundColor: "{colors.card}"
    rounded: "{rounded.pass}"
  pass-band:
    backgroundColor: "{colors.navy}"
    textColor: "{colors.card}"
    padding: "26px 18px 12px"
  pass-valid-stripe:
    backgroundColor: "{colors.gold}"
    textColor: "{colors.navy}"
    padding: "12px 18px 16px"
  idcard:
    backgroundColor: "{colors.card}"
    rounded: "{rounded.idcard}"
  nav-rail:
    backgroundColor: "{colors.navy}"
    textColor: "{colors.card}"
    width: "248px"
  nav-item-current:
    backgroundColor: "{colors.card}"
    textColor: "{colors.navy}"
    rounded: "{rounded.control}"
    height: "44px"
---

# Design System: Student Org Events

## Overview

**Creative North Star: "The Pass Rack and the Registrar's Desk"**

Every registration is a school ID pass you wear. The interface is a cool laminate-grey desk (the page ground) on which two kinds of paper lie: white card-stock passes with a navy ID header band, a punched lanyard slot and, when the pass is yours and valid, a marigold stripe along the bottom; and plain white sheets that carry forms and dense ledgers. The pass is the object students hold and officers punch. The sheet is the record admins read and print.

Density is split by paper type. Passes are generous and tactile: condensed caps field labels sit over values exactly as on a printed ID, numbers are tabular, codes are monospaced and letter-spaced for reading aloud. Sheets are quiet and dense: navy condensed-caps column heads on a pale fill, hairline rules, no decoration inside rows. Navy is the ink of record and the chrome (rail, top bar, header bands); marigold is the strap, the valid stripe, the key action and the focus ring.

The system refuses the purple SaaS sidebar-and-KPI-card dashboard. Counts read as ID fields in a single strip, not as a grid of floating stat cards. Motion is limited to the lanyard swing when an ID first appears and the hole-punch (with its row flash) when a check-in succeeds.

**Key Characteristics:**
- Laminate-grey desk ground; white card stock and white sheets on top of it.
- Navy ID header band with a punched lanyard slot on every pass-type object.
- Marigold is the lanyard, the "valid" stripe, the key action and focus; never a background wash for whole regions.
- Condensed caps field labels over plain values; tabular numerals; monospaced ID codes.
- Pass look on events, tickets, auth, the officer door card and the check-in result only. Tables stay plain ledgers.
- Two motions only: lanyard swing and punch.

## Colors

A navy-and-marigold ID palette on a cool grey desk, with a four-state semantic set that is the only other colour allowed.

### Primary
- **ID Navy** (navy): the header band of every pass and ID, the app rail and mobile top bar, primary buttons, link colour, ledger header text, big field figures. It is the ink of record.
- **Band Hover Navy** (navy-2): hover on navy surfaces and primary buttons; link hover.
- **Band Ink** (navy-ink): secondary text set on navy (org subtitles, role labels, brand subline).
- **Rail Label Blue-Grey** (rail-muted): group labels and resting icons inside the navy rail, and quiet strokes inside the brand mark.

### Secondary
- **Lanyard Marigold** (gold): lanyard strap, the "valid" stripe at the foot of a held pass, the officer door card and ID footers, the key (check-in) button, the current-page slot marker in navigation, the monogram tile, text selection, and the 3px focus ring. Text on marigold is always navy.
- **Deep Marigold** (gold-deep): marigold-family text on white where AA contrast is needed.
- **Marigold Wash** (gold-wash): notices, the officer role tag, and the row flash at the punch moment.

### Neutral
- **Laminate Desk** (desk): page ground behind everything, and the colour "seen through" punched holes and lanyard slots.
- **Card Stock** (card): passes, IDs, sheets, inputs, mobile tab bar.
- **Quiet Fill** (card-2): ledger header row, ID photo slot, scanner idle slab, menu hover.
- **Ink** (ink): body text and headings.
- **Muted Slate** (muted): field labels, secondary cell text, hints, page subtitles (6.2:1 on white).
- **Hairline** (rule): table rules, sheet borders, seat-meter empty segments.
- **Strong Hairline** (rule-strong): input borders, ledger header underline, dashed tear lines and dashed empty-state outlines.

### State
- **Go Green** (ok / ok-wash): Open, Registered, Present, Active; the punched result band on a successful check-in; success toasts.
- **Hold Amber** (warn / warn-wash): Closed, Waitlisted, Late.
- **Void Red** (bad / bad-wash): Cancelled, Absent, Inactive; the failed check-in band; danger buttons; validation.
- **Idle Slate** (idle / idle-wash): Draft, Pending, Student.

### Named Rules
**The Navy Ink Rule.** Anything set on marigold is set in navy. White on marigold does not exist in this system.

**The One Vocabulary Rule.** Every status in the product maps to exactly one of the four state pairs (or navy for final states such as Completed and Admin, marigold wash for Officer). A new status joins an existing pair; it never gets a new hue.

**The Seen-Through Rule.** Holes and slots (lanyard slot, punch hole) are filled with the desk colour and an inset shadow, so they read as cut through the card to the desk beneath.

## Typography

**Display Font:** Barlow Condensed 500/600/700 (fallback Barlow, system-ui)
**Body Font:** Barlow 400/500/600 (fallback system-ui, -apple-system, Segoe UI)
**Label/Mono Font:** Barlow Condensed caps for labels; ui-monospace stack (Cascadia Mono, SFMono-Regular, Consolas) for ID codes and student numbers

**Character:** the condensed face is the printed ID: headings, names, figures and every small caps label. The regular width carries reading text and form values. Both are self-hosted woff2 in `wwwroot/fonts/barlow/`.

### Hierarchy
- **Display** (700, clamp(1.9rem → 2.6rem), 1.02): page titles only.
- **Headline** (700, clamp(1.5rem → 2rem), 1.05): the officer door card title; also the scale of the auth title (2rem) and the ID holder name (1.7rem).
- **Title** (700, 1.3–1.5rem, 1.08–1.1): sheet titles, pass titles (1.45rem), punch-card name (1.5rem), empty-state title (1.25rem).
- **Figure** (700 condensed, 1.9rem, 1, tabular): values in the field strip; 1.6rem on the door card; 1.4rem in the compact strip on phones.
- **Body** (400, 1rem, 1.5): reading text; event descriptions cap at 68ch. Secondary cell text and hints at 0.88rem.
- **Label** (600 condensed, 0.78rem, 0.08em, uppercase, muted): field labels over values, form labels (navy when on a form), ledger heads (700, 0.8rem, navy), tags (700, 0.8rem, 0.06em), tab bar labels (0.74rem).
- **Code** (600 mono, 1.45rem, 0.28em tracking): the registration code under the ticket QR. Inline code and student numbers use the same stack at 0.92em with 0.06em tracking, in navy.

### Named Rules
**The Label-Over-Value Rule.** Data is presented as a condensed caps label above a plain value, as on a printed ID. Labels name a field; they never introduce a heading.

**The Tabular Rule.** Every count, time, seat figure and ledger column uses tabular numerals.

## Layout

Desktop is a fixed navy rail (248px) on the left and a main column padded 32px 40px 56px, capped at 1240px. Below 992px the rail disappears: a fixed navy top bar (60px) carries the brand and an account menu sheet, and a white thumb-reach tab bar (64px) with 44px targets sits at the bottom; content padding clears both.

Passes sit in a rack: an auto-fill grid with a 270px minimum and 22px gaps. Forms use a 12-column grid with 16px column gaps, fields full-width on phones and spanning 4/6/8 columns from 720px. Event details split into content plus a 360px ID column above 1100px. The check-in desk keeps the attendee ledger full-width and places the scanner beside it only from 1600px; between 760px and 1600px the scanner and manual entry sit side by side inside their sheet.

Spacing rhythm is 8/12/16/20/24 with 40px page gutters: 24px between sheets and below the page head, 20px sheet padding, 16–20px pass padding. Ledgers marked for stacking collapse into labelled row blocks below 720px (96px label column).

## Elevation & Depth

Hybrid and shallow. Paper lies on the desk with a low navy-tinted shadow; only held objects lift. There is no glass, no blur, no coloured glow beyond the key button's hover.

### Shadow Vocabulary
- **Resting paper** (`box-shadow: 0 1px 2px rgba(14,42,71,.08), 0 2px 6px rgba(14,42,71,.06)`): sheets, field strips, passes, the door card.
- **Lifted paper** (`box-shadow: 0 6px 18px rgba(14,42,71,.14), 0 2px 4px rgba(14,42,71,.08)`): hovered pass, mobile menu sheet, toasts.
- **Hanging ID** (`box-shadow: 0 14px 40px rgba(14,42,71,.20), 0 2px 6px rgba(14,42,71,.10)`): the ID card on its lanyard (login, register, ticket, event details).
- **Cut-through** (`box-shadow: inset 0 1px 2px rgba(0,0,0,.35)`): lanyard slots and the punch hole.

### Named Rules
**The Navy Shadow Rule.** Shadows are tinted with navy (14,42,71), never neutral black; only cut-through holes use black insets. Print drops all shadows.

## Shapes

Soft rectangles graded by object: tags 6px, controls and nav items 8px, sheets 10px, passes and the door card 14px, the hanging ID 18px. The bigger and more "held" the object, the rounder its corners. Header bands are flush to the card edge (clipped by the card's radius). Pass-type objects carry a centred pill-shaped lanyard slot in the band. Dashed lines mean a perforation or an empty slot: the tear line above the QR strip, the empty-state outline, the scanner idle slab, the pending tag.

## Components

### Buttons
Solid, 44px, plainly labelled.
- **Shape:** gently rounded (8px); small variant 36px tall with 7px radius; icon-only is 36px square.
- **Primary:** navy fill, white 600 Barlow text, 0 18px padding; hover to band-hover navy.
- **Key:** marigold fill with navy text. Reserved for the door action (open the check-in desk, check in). Hover brightens to #ffc81f with a soft marigold-brown shadow.
- **Outline:** white with strong-hairline border and navy text; hover darkens the border to navy. The default for secondary actions and row actions.
- **Danger:** white with red text and pale red border; hover fills void-red wash.
- **Focus:** global 3px marigold outline, 2px offset. Disabled at 50% opacity.

### Tags
- **Style:** 26px condensed caps chips (700, 0.8rem, 0.06em), 6px radius, a 7px dot in currentColor before the text, fill from the state pair.
- **State:** one class per status, generated by the badge helper from the status name. Pending is hollow (dashed border, ring dot). A cancelled registration is struck through.

### Cards / Containers
- **Sheet:** white, 1px hairline border, 10px radius, resting shadow, 16px 20px head with hairline divider, 20px body. Holds forms and ledgers.
- **Field strip:** one white bar of label-over-figure cells divided by hairlines (flex, 140px minimum per cell). This replaces KPI cards.
- **Notice:** marigold wash with dark amber text, 10px radius.
- **Empty state:** centred muted text under a small dashed outline of an empty pass (inline SVG) and a condensed title.

### Inputs / Fields
- **Style:** 44px, white, 1px strong-hairline border, 8px radius, 8px 12px padding; labels are navy condensed caps above.
- **Focus:** border to navy plus a 3px marigold ring (`0 0 0 3px rgba(242,183,5,.55)`).
- **Error:** red border, red 0.88rem message; summary block in void-red wash. Checkboxes are 20px with navy accent inside a 44px row.
- **Code entry:** the manual check-in field sets its value in the mono stack, uppercase, 0.12em tracking.

### Navigation
- **Rail:** navy, 44px items in white-ish Barlow 500 with blue-grey icons; hover navy-2. The current page is a white tab with navy text and a small marigold slot marker at its right end. Group labels are blue-grey condensed caps. The foot holds a marigold monogram tile with name and role.
- **Mobile:** navy top bar with brand and account sheet; white bottom tab bar with condensed caps labels; the current tab is navy with a 28×4px marigold tab hanging from the top edge.

### Ledger (tables)
Plain, dense, printable. White sheet, sticky header row on quiet fill with navy condensed caps heads and a strong hairline beneath, 12px 14px cells divided by hairlines, a barely-there hover (#f8fafc), tabular numerals throughout. Primary cell text is 600 ink with a muted 0.88rem sub-line. Row actions are small outline buttons (key for check-in). Status appears only as a tag. On phones, stacking ledgers become label/value row blocks.

### The Pass (signature)
The event card in the rack. White card stock, 14px radius, resting shadow; a navy band with the lanyard slot holds the organisation name in condensed caps (and a status tag when the event is not open); the body has the title and a 2×2 label-over-value grid (date, time, venue, seats with a ten-segment seat meter); the foot carries the registration deadline or "Your pass" with the student's standing. A pass the student holds turns its foot into the marigold "valid" stripe. The whole pass is one link.

### The Hanging ID (signature)
Used for login, register, the ticket, and the event-details registration panel. A marigold lanyard SVG with a grey clip hangs from the top edge onto an 18px-radius ID with a larger band slot, a 76×92px photo slot showing initials, the holder's name, label-over-value fields, a dashed tear line, the QR on white card stock, the spaced mono code, and a marigold foot showing standing. It swings once on first view.

### The Door Card (signature)
The officer dashboard's lead: a wide pass with the band (org plus registration status tag), the event title, a row of four label-over-figure counts, a column of actions led by the key button, and a full-width marigold foot naming "Today's door" or "Next door" and the venue.

### The Punch Card (signature)
The check-in result. A small pass whose band turns go-green on success or void-red on failure, with the name and message beneath. On success a desk-coloured hole punches in at the band's right edge while the green chad falls away, and the matching ledger row flashes marigold wash once and updates its tag in place.

### Scanner Idle Slab
Before the camera starts, the reader is a quiet-fill slab with a 2px dashed strong-hairline border, 14px radius and a grey slot at the top, like an empty card slot; its start button is a primary navy button.

## Do's and Don'ts

### Do:
- **Do** put the pass look (navy band, lanyard slot, marigold stripe) only on events, tickets, auth, the officer door card and the check-in result.
- **Do** keep officer and admin tables as plain, dense ledgers: navy condensed caps heads on quiet fill, hairline rules, tabular numerals, tags for status.
- **Do** present data as condensed caps labels over plain values, and show counts in a single field strip.
- **Do** set every text on marigold in navy, and reserve the marigold key button for the door action.
- **Do** keep touch targets at 44px minimum and focus visible with the 3px marigold ring.
- **Do** tint shadows navy and drop them when printing.
- **Do** limit motion to the lanyard swing and the punch (with its row flash), both behind prefers-reduced-motion.

### Don't:
- **Don't** put pass bands, slots, stripes or lanyards inside table rows.
- **Don't** build dashboards from grids of floating KPI stat cards, and don't use purple or gradient SaaS chrome.
- **Don't** introduce a new hue for a new status; map it to an existing state pair.
- **Don't** put a QR code on marigold; it sits on white card stock above the stripe for scan contrast.
- **Don't** set a small caps label above a heading as an eyebrow; labels name fields only.
- **Don't** add school names, crests, mottos or vendor branding; the brand mark is the lanyard-pass glyph only.
