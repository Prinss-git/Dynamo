// Ask before destructive actions: <form data-confirm="Are you sure?">
document.addEventListener("submit", function (e) {
    var message = e.target.getAttribute("data-confirm");
    if (message && !window.confirm(message)) {
        e.preventDefault();
    }
});

// Close the mobile account menu when tapping outside it
document.addEventListener("click", function (e) {
    document.querySelectorAll("details.topbar-menu[open]").forEach(function (menu) {
        if (!menu.contains(e.target)) menu.removeAttribute("open");
    });
});

// Close the account switcher when tapping outside it
document.addEventListener("click", function (e) {
    var switcher = document.querySelector("details.switcher[open]");
    if (switcher && !switcher.contains(e.target)) switcher.removeAttribute("open");
});

/* ------------------------------------------------------------------
   Pagination for every ledger table and every pass-card rack.
   - Tables page their tbody rows; racks (.rack) page their cards.
   - data-page-size sets the first page size (tables 10, racks 9);
     the size menu offers 10/25/50 for tables, otherwise that size, x2, x4.
   - Items a page filters out carry [data-filtered-out]; call
     el.pager.render() after changing them. Each render dispatches a
     "pager:render" event on the table or rack.
   ------------------------------------------------------------------ */
(function () {
    function pageList(current, total) {
        // 1 … 4 5 6 … 12
        var pages = [];
        for (var p = 1; p <= total; p++) {
            if (p === 1 || p === total || Math.abs(p - current) <= 1) pages.push(p);
            else if (pages[pages.length - 1] !== "…") pages.push("…");
        }
        return pages;
    }

    function attach(el, opts) {
        var base = parseInt(el.getAttribute("data-page-size"), 10) || opts.size;
        var sizes = base === 10 ? [10, 25, 50] : [base, base * 2, base * 4];
        var size = base;
        var page = 1;

        var nav = document.createElement("nav");
        nav.className = "pager" + (opts.free ? " pager--free" : "");
        nav.setAttribute("aria-label", opts.label);
        opts.anchor.parentNode.insertBefore(nav, opts.anchor.nextSibling);

        function visibleItems() {
            return opts.items().filter(function (r) { return !r.hasAttribute("data-filtered-out"); });
        }

        function button(label, target, o) {
            o = o || {};
            var b = document.createElement("button");
            b.type = "button";
            b.className = "pager-btn" + (o.current ? " is-current" : "");
            b.innerHTML = label;
            if (o.aria) b.setAttribute("aria-label", o.aria);
            if (o.current) b.setAttribute("aria-current", "page");
            if (o.disabled) b.disabled = true;
            else b.addEventListener("click", function () {
                page = target;
                render();
                // keep the top of a long card rack in view after paging
                if (opts.free && el.getBoundingClientRect().top < 0) el.scrollIntoView({ block: "start" });
            });
            return b;
        }

        function render() {
            var visible = visibleItems();
            var total = visible.length;
            var pages = Math.max(1, Math.ceil(total / size));
            if (page > pages) page = pages;
            var start = (page - 1) * size;

            opts.items().forEach(function (r) { r.hidden = true; });
            visible.slice(start, start + size).forEach(function (r) { r.hidden = false; });

            nav.innerHTML = "";
            nav.hidden = total <= sizes[0];
            if (!nav.hidden) {
                var info = document.createElement("span");
                info.className = "pager-info";
                info.textContent = "Showing " + (start + 1) + "–" + Math.min(start + size, total) + " of " + total;

                var controls = document.createElement("div");
                controls.className = "pager-pages";
                controls.appendChild(button('<i class="fa-solid fa-chevron-left" aria-hidden="true"></i>', page - 1, { disabled: page === 1, aria: "Previous page" }));
                pageList(page, pages).forEach(function (p) {
                    if (p === "…") {
                        var gap = document.createElement("span");
                        gap.className = "pager-gap";
                        gap.textContent = "…";
                        controls.appendChild(gap);
                    } else {
                        controls.appendChild(button(String(p), p, { current: p === page, aria: "Page " + p }));
                    }
                });
                controls.appendChild(button('<i class="fa-solid fa-chevron-right" aria-hidden="true"></i>', page + 1, { disabled: page === pages, aria: "Next page" }));

                var sizeLabel = document.createElement("label");
                sizeLabel.className = "pager-size";
                var select = document.createElement("select");
                select.className = "input";
                sizes.forEach(function (s) {
                    var o = document.createElement("option");
                    o.value = s; o.textContent = s;
                    if (s === size) o.selected = true;
                    select.appendChild(o);
                });
                select.addEventListener("change", function () { size = parseInt(select.value, 10); page = 1; render(); });
                sizeLabel.appendChild(document.createTextNode(opts.unit + " "));
                sizeLabel.appendChild(select);

                nav.appendChild(info);
                nav.appendChild(controls);
                nav.appendChild(sizeLabel);
            }
            el.dispatchEvent(new CustomEvent("pager:render"));
        }

        el.pager = { render: function () { page = 1; render(); } };
        render();
    }

    document.querySelectorAll("table.ledger").forEach(function (table) {
        var body = table.tBodies[0];
        if (!body) return;
        attach(table, {
            size: 10, label: "Table pages", unit: "Rows",
            anchor: table.closest(".ledger-wrap") || table,
            items: function () { return Array.prototype.slice.call(body.rows); }
        });
    });

    document.querySelectorAll(".rack").forEach(function (rack) {
        attach(rack, {
            size: 9, label: "Event pages", unit: "Cards", free: true,
            anchor: rack,
            items: function () { return Array.prototype.slice.call(rack.children); }
        });
    });
})();

/* ------------------------------------------------------------------
   Required fields: <form data-live-required>
   Every [data-val-required] field gets a red "*" on its label.
   Fields start neutral. A field turns green once its value passes its
   validation rules, and red only after the user leaves it empty or
   invalid, or after a submit attempt (which also moves focus to the
   first invalid field). Optional fields are never marked.
   ------------------------------------------------------------------ */
function initRequiredMarkers() {
    document.querySelectorAll("form[data-live-required]").forEach(function (form) {
        // Reuse the validator jquery.validate.unobtrusive built for this form (never create our own:
        // a validator created first would be kept without the form's rules).
        var validator = window.jQuery ? jQuery(form).data("validator") : null;
        var marks = [];
        // A page the server sent back with errors (a failed submit) shows every field's state right away.
        var submitted = !!form.querySelector(".input-validation-error, .field-validation-error");

        form.querySelectorAll("[data-val-required]").forEach(function (field) {
            var label = form.querySelector('label[for="' + field.id + '"]');
            if (!label) return;
            field.setAttribute("aria-required", "true");
            var mark = document.createElement("span");
            mark.className = "req";
            mark.textContent = "*";
            mark.title = "Required";
            label.appendChild(mark);
            // edited: the user changed the value; left: the user has moved focus out of the field
            marks.push({ field: field, edited: false, left: false });
        });

        // Runs the field's rules without touching the validator's state. validator.check() would record
        // the field as "submitted", which makes jquery.validate show error text while the user types.
        function passesRules(field) {
            if (!validator || !jQuery.validator) return true;
            try {
                var rules = jQuery(field).rules();
                var value = validator.elementValue(field);
                for (var method in rules) {
                    if (!Object.prototype.hasOwnProperty.call(rules, method)) continue;
                    var test = jQuery.validator.methods[method];
                    if (typeof test !== "function" || method === "remote") continue;
                    var result = test.call(validator, value, field, rules[method]);
                    if (result === "dependency-mismatch" || result === "pending") continue;
                    if (!result) return false;
                }
                return true;
            } catch (e) {
                return true; // never block the form because of a client-side check; the server validates too
            }
        }

        function isMet(field) {
            if (!field.value || !field.value.trim()) return false;
            return passesRules(field);
        }

        function update() {
            marks.forEach(function (m) {
                var met = isMet(m.field);
                m.field.classList.toggle("field-met", met && (m.edited || m.left || submitted));
                m.field.classList.toggle("field-unmet", !met && (m.left || submitted));
            });
        }

        function markFor(element) {
            for (var i = 0; i < marks.length; i++) {
                if (marks[i].field === element) return marks[i];
            }
            return null;
        }

        function onEdit(e) {
            var m = markFor(e.target);
            if (m) m.edited = true;
            // re-check every field so dependent rules (confirm password) stay in sync
            update();
        }

        form.addEventListener("input", onEdit);
        form.addEventListener("change", onEdit);
        form.addEventListener("focusout", function (e) {
            var m = markFor(e.target);
            if (!m) return;
            m.left = true;
            update();
        });
        // jquery.validate's own submit handler was attached first, so its error classes are already set here.
        form.addEventListener("submit", function () {
            submitted = true;
            update();
            var first = form.querySelector(".field-unmet, .input-validation-error");
            if (first) {
                first.scrollIntoView({ block: "center" });
                first.focus({ preventScroll: true });
            }
        });
        update();
    });
}

/* ------------------------------------------------------------------
   Password checklist: <input data-password-rules>
   Shows each rule of the password policy and ticks it off live.
   Keep in sync with Const.PasswordPattern on the server.
   ------------------------------------------------------------------ */
function initPasswordRules() {
    var RULES = [
        { text: "At least 8 characters", test: function (v) { return v.length >= 8; } },
        { text: "An uppercase letter (A-Z)", test: function (v) { return /[A-Z]/.test(v); } },
        { text: "A lowercase letter (a-z)", test: function (v) { return /[a-z]/.test(v); } },
        { text: "A number (0-9)", test: function (v) { return /\d/.test(v); } },
        { text: "A special character (e.g. @ # ! ?)", test: function (v) { return /[^A-Za-z0-9]/.test(v); } }
    ];
    document.querySelectorAll("input[data-password-rules]").forEach(function (input) {
        var list = document.createElement("ul");
        list.className = "pw-rules";
        list.id = input.id + "-rules";
        input.setAttribute("aria-describedby", list.id);
        var items = RULES.map(function (rule) {
            var li = document.createElement("li");
            li.innerHTML = '<i class="fa-solid fa-circle-xmark" aria-hidden="true"></i> <span></span>';
            li.querySelector("span").textContent = rule.text;
            list.appendChild(li);
            return { rule: rule, li: li, icon: li.querySelector("i") };
        });
        // below the show/hide wrapper when there is one, so the toggle stays inside the field
        var anchor = input.closest(".pw-wrap") || input;
        anchor.parentNode.insertBefore(list, anchor.nextSibling);
        function update() {
            var v = input.value || "";
            items.forEach(function (it) {
                var ok = it.rule.test(v);
                it.li.classList.toggle("is-met", ok);
                it.icon.className = "fa-solid " + (ok ? "fa-circle-check" : "fa-circle-xmark");
            });
            // an optional password (admin edit) that is left empty hides the checklist
            list.hidden = input.hasAttribute("data-optional") && v === "";
        }
        input.addEventListener("input", update);
        update();
    });
}

/* ------------------------------------------------------------------
   Show/hide password: every <input type="password"> gets an eye
   button inside the field. It is a real <button> (Tab + Enter/Space),
   announces its state with aria-pressed, and keeps the keyboard open
   on phones by not taking focus from the field on a tap or click.
   ------------------------------------------------------------------ */
function initPasswordToggles() {
    document.querySelectorAll('input[type="password"]').forEach(function (input) {
        if (input.closest(".pw-wrap")) return;

        var wrap = document.createElement("div");
        wrap.className = "pw-wrap";
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(input);

        var button = document.createElement("button");
        button.type = "button";
        button.className = "pw-toggle";
        button.setAttribute("aria-label", "Show password");
        if (input.id) button.setAttribute("aria-controls", input.id);
        var icon = document.createElement("i");
        icon.setAttribute("aria-hidden", "true");
        button.appendChild(icon);
        wrap.appendChild(button);

        function render(visible) {
            button.setAttribute("aria-pressed", visible ? "true" : "false");
            button.title = visible ? "Hide password" : "Show password";
            icon.className = "fa-solid " + (visible ? "fa-eye-slash" : "fa-eye");
        }

        // keep focus (and the phone keyboard) in the field when the button is clicked or tapped
        button.addEventListener("mousedown", function (e) { e.preventDefault(); });
        button.addEventListener("click", function () {
            var show = input.type === "password";
            input.type = show ? "text" : "password";
            render(show);
        });
        // never submit a readable password (keeps browsers from saving it as plain text)
        if (input.form) input.form.addEventListener("submit", function () { input.type = "password"; render(false); });
        render(false);
    });
}

/* ------------------------------------------------------------------
   Event form, "Who can register": the course checkboxes show only for
   "Only students of selected courses"; each department's box selects or
   clears all of its courses and shows a dash when only some are ticked.
   ------------------------------------------------------------------ */
function initCoursePicker() {
    document.querySelectorAll("[data-eligibility]").forEach(function (box) {
        var picker = box.querySelector("[data-course-picker]");
        box.querySelectorAll('input[name="RestrictToCourses"]').forEach(function (radio) {
            radio.addEventListener("change", function () { picker.hidden = radio.value !== "true" || !radio.checked; });
        });
        box.querySelectorAll("[data-course-group]").forEach(function (group) {
            var all = group.querySelector("[data-select-department]");
            var courses = Array.prototype.slice.call(group.querySelectorAll('input[name="AllowedCourses"]'));
            function sync() {
                var ticked = courses.filter(function (c) { return c.checked; }).length;
                all.checked = ticked === courses.length;
                all.indeterminate = ticked > 0 && ticked < courses.length;
            }
            all.addEventListener("change", function () { courses.forEach(function (c) { c.checked = all.checked; }); sync(); });
            courses.forEach(function (c) { c.addEventListener("change", sync); });
            sync();
        });
    });
}

// Run after jquery.validate.unobtrusive has parsed the forms on document ready.
function initForms() { initPasswordToggles(); initPasswordRules(); initRequiredMarkers(); initCoursePicker(); }
if (window.jQuery) { jQuery(initForms); } else { document.addEventListener("DOMContentLoaded", initForms); }
