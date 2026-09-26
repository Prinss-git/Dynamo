---
version: 1
slug: "asi-basecode-webapp"
primary_target: "ASI.Basecode.WebApp"
related_targets: []
---

# Surface brief: whole web app (ASI.Basecode.WebApp)

Scope: every Razor view (auth, dashboard, events, tickets, attendance, reports, organizations, users). Mode: Operate.
Audience/job: students register and show a pass on phones; officers check in at the door one-handed; admins manage and report on laptops.
Constraint (user): the ID/lanyard world must keep officer and admin tables plain, dense and easy to scan. The pass look lives on events, tickets, auth and the check-in result, never inside table rows.
No ASI/Alliance branding anywhere. Code-led (no image generation).

## Direction contract

THESIS: every registration is a school ID pass you wear; the app is the pass rack plus the registrar's desk. Refuses the purple SaaS sidebar-and-KPI-card dashboard.
OWN-WORLD: cool laminate-grey desk ground, white card stock with a punched lanyard slot, navy ID header band, marigold lanyard strap and "valid" stripe, condensed caps field labels over values, mono ID numbers, QR in the barcode strip. Tables: white sheet, navy header labels in condensed caps, hairline rules, tabular numerals.
STORY: students see their passes and their standing at a glance, open a pass, and show it at the door; officers scan and see the pass punched; admins read clean ledgers.
FIRST VIEWPORT: login is one ID card hanging from a marigold lanyard from the top edge, the sign-in fields as the card's fields; the dashboard leads with the user's next pass (students) or today's door (officers).
FORM: School ID & Lanyard Pass, rank 1 of 7 (pick card over roll #3), seed 62f3bf7c.
FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

Signature interaction: the check-in result punches the pass (hole-punch animation on the result card) and the row's status updates in place. Motion grammar: passes settle with a short lanyard swing on first view; nothing else animates.

Adaptations (cited): the ticket's QR sits on white card stock directly above the marigold strip rather than printed on the strip, because a QR on marigold loses scan contrast at the door. The in-place row highlight (rowflash) belongs to the signature check-in moment; it is not a separate motion.
