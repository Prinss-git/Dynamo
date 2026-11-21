/* ------------------------------------------------------------------
   Simple dialogs: <button data-open-modal="dialog-id"> opens <dialog id="dialog-id" class="modal">;
   [data-close-modal], Esc or a click on the dimmed backdrop closes it. The first field gets focus.
   ------------------------------------------------------------------ */
document.addEventListener("click", function (e) {
    var opener = e.target.closest && e.target.closest("[data-open-modal]");
    if (opener) {
        var dialog = document.getElementById(opener.getAttribute("data-open-modal"));
        if (dialog && !dialog.open) {
            dialog.showModal();
            var first = dialog.querySelector(".modal-body input:not([type=hidden]), .modal-body select, .modal-body textarea");
            if (first) first.focus();
        }
        return;
    }
    var closer = e.target.closest && e.target.closest("[data-close-modal]");
    if (closer && closer.closest("dialog")) closer.closest("dialog").close();
});
document.addEventListener("mousedown", function (e) {
    // a press on the backdrop (the dialog element itself, outside its panel) closes it
    if (e.target.matches && e.target.matches("dialog.modal[open]")) e.target.close();
});

/* ------------------------------------------------------------------
   Confirmation dialog (replaces the browser's confirm() popup).
   - <form data-confirm="Delete 'X'? This can't be undone."> asks before submitting;
   - <button data-confirm-button="..."> asks before that one button submits.
   The question becomes the heading, any sentence after it the body. The
   confirm button copies the clicked button's icon and label, and is red when
   that button is a danger button. Esc, the backdrop or "Go back" cancels.
   ------------------------------------------------------------------ */
var confirmDialog = (function () {
    var dialog, iconEl, titleEl, textEl, okBtn, backBtn, resolver;

    function build() {
        dialog = document.createElement("dialog");
        dialog.className = "confirm";
        dialog.setAttribute("aria-labelledby", "confirm-title");
        dialog.setAttribute("aria-describedby", "confirm-text");
        dialog.innerHTML =
            '<div class="confirm-main">' +
            '  <span class="confirm-icon" aria-hidden="true"><i class="fa-solid"></i></span>' +
            '  <div class="confirm-copy"><h2 class="confirm-title" id="confirm-title"></h2><p class="confirm-text" id="confirm-text"></p></div>' +
            "</div>" +
            '<div class="confirm-actions">' +
            '  <button type="button" class="btn btn-outline" data-act="back"><i class="fa-solid fa-arrow-left" aria-hidden="true"></i> Go back</button>' +
            '  <button type="button" class="btn" data-act="ok"></button>' +
            "</div>";
        document.body.appendChild(dialog);
        iconEl = dialog.querySelector(".confirm-icon i");
        titleEl = dialog.querySelector(".confirm-title");
        textEl = dialog.querySelector(".confirm-text");
        okBtn = dialog.querySelector('[data-act="ok"]');
        backBtn = dialog.querySelector('[data-act="back"]');
        okBtn.addEventListener("click", function () { finish(true); });
        backBtn.addEventListener("click", function () { finish(false); });
        dialog.addEventListener("cancel", function (e) { e.preventDefault(); finish(false); });
        // a click on the dimmed backdrop (outside the panel) cancels
        dialog.addEventListener("mousedown", function (e) { if (e.target === dialog) finish(false); });
    }

    function finish(result) {
        if (!dialog.open) return;
        dialog.close();
        if (resolver) { var r = resolver; resolver = null; r(result); }
    }

    // opts: { message, okHtml, danger }
    return function (opts) {
        if (!dialog) build();
        var message = (opts.message || "Are you sure?").trim();
        var cut = message.indexOf("?");
        var title = cut >= 0 ? message.slice(0, cut + 1) : message;
        var rest = cut >= 0 ? message.slice(cut + 1).trim() : "";
        titleEl.textContent = title;
        textEl.textContent = rest;
        textEl.hidden = !rest;
        dialog.classList.toggle("is-danger", !!opts.danger);
        iconEl.className = "fa-solid " + (opts.danger ? "fa-triangle-exclamation" : "fa-circle-question");
        okBtn.className = "btn " + (opts.danger ? "btn-danger-solid" : "btn-primary");
        okBtn.innerHTML = opts.okHtml || '<i class="fa-solid fa-check" aria-hidden="true"></i> Confirm';
        return new Promise(function (resolve) {
            resolver = resolve;
            dialog.showModal();
            // destructive: the safe choice has focus, so Enter can't destroy anything by accident
            (opts.danger ? backBtn : okBtn).focus();
        });
    };
})();

(function () {
    // Builds the dialog options from the button that was clicked.
    function optionsFor(message, button) {
        var label = button ? button.innerHTML.trim() : "";
        if (button && !/fa-/.test(label)) label = '<i class="fa-solid fa-check" aria-hidden="true"></i> ' + label;
        return {
            message: message,
            okHtml: label || null,
            danger: !!(button && button.classList.contains("btn-danger"))
        };
    }

    function submitConfirmed(form, button) {
        form.setAttribute("data-confirmed", "");
        if (form.requestSubmit) form.requestSubmit(button || undefined); else form.submit();
        form.removeAttribute("data-confirmed");
    }

    // whole forms: <form data-confirm="...">
    document.addEventListener("submit", function (e) {
        var form = e.target;
        var message = form.getAttribute("data-confirm");
        if (!message || form.hasAttribute("data-confirmed")) return;
        e.preventDefault();
        var button = e.submitter || form.querySelector('[type="submit"]');
        confirmDialog(optionsFor(message, button)).then(function (ok) { if (ok) submitConfirmed(form, button); });
    });

    // single buttons: <button data-confirm-button="...">
    document.addEventListener("click", function (e) {
        var button = e.target.closest && e.target.closest("[data-confirm-button]");
        if (!button || button.disabled || !button.form || button.form.hasAttribute("data-confirmed")) return;
        e.preventDefault();
        confirmDialog(optionsFor(button.getAttribute("data-confirm-button"), button)).then(function (ok) {
            if (ok) submitConfirmed(button.form, button);
        });
    });
})();

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
   - data-page-size sets the first page size (tables 10, racks 12);
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
            size: 12, label: "Event pages", unit: "Cards", free: true,
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
/* ------------------------------------------------------------------
   Searchable picker: <select data-combobox> becomes a text box that filters
   the options as you type (name, student number or course), with a short
   two-line result list. The original <select> stays in the form, hidden,
   and receives the chosen value, so the server side doesn't change.
   ------------------------------------------------------------------ */
function initComboboxes() {
    var MAX_RESULTS = 30;

    function escapeHtml(s) {
        return s.replace(/[&<>"']/g, function (c) { return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]; });
    }
    function escapeRegex(s) { return s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"); }
    // wrap every typed word found in the text in <mark>
    function highlight(text, terms) {
        var html = escapeHtml(text);
        if (!terms.length) return html;
        var re = new RegExp("(" + terms.map(function (t) { return escapeRegex(escapeHtml(t)); }).join("|") + ")", "gi");
        return html.replace(re, "<mark>$1</mark>");
    }
    function initials(name) {
        var parts = name.trim().split(/\s+/);
        return ((parts[0] || "").charAt(0) + (parts.length > 1 ? parts[parts.length - 1].charAt(0) : "")).toUpperCase();
    }

    document.querySelectorAll("select[data-combobox]").forEach(function (select, n) {
        var items = Array.prototype.filter.call(select.options, function (o) { return o.value; }).map(function (o) {
            var label = o.getAttribute("data-label") || o.text;
            // second line: the option's own hint, else its group (e.g. a course's college)
            var hint = o.getAttribute("data-hint") || (o.parentNode.tagName === "OPTGROUP" ? o.parentNode.label : "");
            return { value: o.value, label: label, hint: hint, tag: o.getAttribute("data-tag") || "", haystack: (label + " " + hint).toLowerCase() };
        });
        var noun = select.getAttribute("data-noun") || "results";
        var listId = "combo-list-" + n;

        // the visible label now points at the text box; the select keeps its name for the form
        var wrap = document.createElement("div");
        wrap.className = "combo";
        var input = document.createElement("input");
        input.type = "text";
        input.className = "input combo-input";
        input.id = select.id;
        input.autocomplete = "off";
        input.spellcheck = false;
        input.placeholder = select.getAttribute("data-placeholder") || "Type to search";
        input.setAttribute("role", "combobox");
        input.setAttribute("aria-autocomplete", "list");
        input.setAttribute("aria-expanded", "false");
        input.setAttribute("aria-controls", listId);
        select.id = select.id + "-native";
        var wasRequired = select.required;   // only a required picker blocks submitting without a choice
        var avatars = select.hasAttribute("data-avatars");   // initials bubble: people yes, courses no
        select.required = false;
        select.tabIndex = -1;
        select.setAttribute("aria-hidden", "true");
        select.classList.add("combo-native");

        var icon = document.createElement("span");
        icon.className = "combo-icon";
        icon.innerHTML = '<i class="fa-solid fa-magnifying-glass" aria-hidden="true"></i>';
        var clear = document.createElement("button");
        clear.type = "button";
        clear.className = "combo-clear";
        clear.hidden = true;
        clear.setAttribute("aria-label", "Clear");
        clear.innerHTML = '<i class="fa-solid fa-xmark" aria-hidden="true"></i>';
        var pop = document.createElement("div");
        pop.className = "combo-pop";
        pop.hidden = true;
        var list = document.createElement("ul");
        list.className = "combo-list";
        list.id = listId;
        list.setAttribute("role", "listbox");
        var foot = document.createElement("div");
        foot.className = "combo-foot";
        var error = document.createElement("span");
        error.className = "text-danger combo-error";
        error.hidden = true;
        error.textContent = "Pick someone from the list.";
        pop.appendChild(list);
        pop.appendChild(foot);

        select.parentNode.insertBefore(wrap, select);
        wrap.appendChild(icon);
        wrap.appendChild(input);
        wrap.appendChild(clear);
        wrap.appendChild(pop);
        wrap.appendChild(select);
        wrap.parentNode.insertBefore(error, wrap.nextSibling);

        var shown = [];

        // editing a record: show the value it already has
        var preset = items.filter(function (it) { return it.value === select.value; })[0];
        if (preset) {
            input.value = preset.label;
            wrap.classList.add("has-value");
            clear.hidden = false;
        }
        var active = -1;

        function setActive(i) {
            var nodes = list.querySelectorAll(".combo-opt");
            if (active >= 0 && nodes[active]) nodes[active].classList.remove("is-active");
            active = i;
            if (active >= 0 && nodes[active]) {
                nodes[active].classList.add("is-active");
                input.setAttribute("aria-activedescendant", nodes[active].id);
                nodes[active].scrollIntoView({ block: "nearest" });
            } else {
                input.removeAttribute("aria-activedescendant");
            }
        }

        function render() {
            var query = input.value.trim().toLowerCase();
            var terms = query ? query.split(/\s+/) : [];
            var matches = items.filter(function (it) {
                return terms.every(function (t) { return it.haystack.indexOf(t) >= 0; });
            });
            // names that start with what was typed come first
            if (terms.length) {
                matches.sort(function (a, b) {
                    var sa = a.label.toLowerCase().indexOf(terms[0]) === 0 ? 0 : 1;
                    var sb = b.label.toLowerCase().indexOf(terms[0]) === 0 ? 0 : 1;
                    return sa - sb;
                });
            }
            shown = matches.slice(0, MAX_RESULTS);
            list.innerHTML = "";
            shown.forEach(function (it, i) {
                var li = document.createElement("li");
                li.className = "combo-opt" + (select.value === it.value ? " is-chosen" : "");
                li.id = listId + "-" + i;
                li.setAttribute("role", "option");
                li.setAttribute("aria-selected", select.value === it.value ? "true" : "false");
                var tag = it.tag && it.tag !== "Student"
                    ? '<span class="tag tag-' + escapeHtml(it.tag.toLowerCase()) + '">' + escapeHtml(it.tag) + "</span>" : "";
                li.innerHTML = (avatars ? '<span class="combo-avatar" aria-hidden="true">' + escapeHtml(initials(it.label)) + "</span>" : "")
                    + '<span class="combo-text"><span class="combo-name">' + highlight(it.label, terms) + "</span>"
                    + (it.hint ? '<span class="combo-hint">' + highlight(it.hint, terms) + "</span>" : "") + "</span>" + tag;
                li.addEventListener("mousedown", function (e) { e.preventDefault(); choose(it); });
                list.appendChild(li);
            });

            if (!matches.length) {
                list.innerHTML = '<li class="combo-empty" role="presentation">No one matches \u201c' + escapeHtml(input.value.trim()) + "\u201d</li>";
                foot.hidden = true;
            } else if (matches.length > shown.length) {
                foot.hidden = false;
                foot.textContent = "Showing " + shown.length + " of " + matches.length + " " + noun + " \u00b7 keep typing to narrow it down";
            } else {
                foot.hidden = false;
                foot.textContent = matches.length === 1 ? "1 match" : matches.length + " " + noun;
            }
            setActive(shown.length ? 0 : -1);
        }

        function open() {
            if (!pop.hidden) return;
            pop.hidden = false;
            input.setAttribute("aria-expanded", "true");
            render();
        }
        function close() {
            pop.hidden = true;
            input.setAttribute("aria-expanded", "false");
            setActive(-1);
        }
        function choose(it) {
            select.value = it.value;
            input.value = it.label;
            wrap.classList.add("has-value");
            clear.hidden = false;
            error.hidden = true;
            input.classList.remove("field-unmet");
            close();
        }
        function reset() {
            select.value = "";
            wrap.classList.remove("has-value");
        }

        input.addEventListener("focus", open);
        input.addEventListener("click", open);
        input.addEventListener("input", function () {
            reset();
            clear.hidden = !input.value;
            if (pop.hidden) open(); else render();
        });
        input.addEventListener("blur", function () {
            close();
            // typed text that wasn't picked from the list doesn't count as a choice
            if (!select.value && input.value && shown.length === 1 && shown[0].label.toLowerCase() === input.value.trim().toLowerCase()) choose(shown[0]);
        });
        input.addEventListener("keydown", function (e) {
            if (e.key === "ArrowDown") { e.preventDefault(); if (pop.hidden) open(); else setActive(Math.min(active + 1, shown.length - 1)); }
            else if (e.key === "ArrowUp") { e.preventDefault(); setActive(Math.max(active - 1, 0)); }
            else if (e.key === "Enter") { if (!pop.hidden && active >= 0) { e.preventDefault(); choose(shown[active]); } }
            else if (e.key === "Escape") { if (!pop.hidden) { e.preventDefault(); close(); } }
        });
        clear.addEventListener("click", function () {
            input.value = "";
            reset();
            clear.hidden = true;
            input.focus();
        });
        select.form && select.form.addEventListener("submit", function (e) {
            if (select.value || !wasRequired) return;
            e.preventDefault();
            e.stopImmediatePropagation();
            error.hidden = false;
            input.classList.add("field-unmet");
            input.focus();
        });
    });
}

/* ------------------------------------------------------------------
   Plain dropdowns: every ordinary <select class="input"> gets a menu that shows
   5 options and scrolls for the rest (a native select popup can't be height-limited).
   The original <select> stays in the form (hidden) and gets the chosen value plus a
   "change" event, so inline onchange handlers and the server side are unchanged.
   ------------------------------------------------------------------ */
function initSelectMenus() {
    var menu = null, openFor = null;   // one shared popup, reused by every dropdown

    function closeMenu() {
        if (!menu) return;
        menu.hidden = true;
        if (openFor) { openFor.setAttribute("aria-expanded", "false"); openFor = null; }
    }

    document.querySelectorAll("select.input:not([data-combobox]):not([multiple]):not([size]):not(.combo-native)").forEach(function (select) {
        var btn = document.createElement("button");
        btn.type = "button";
        btn.className = select.className + " smenu-btn";
        if (select.getAttribute("style")) btn.setAttribute("style", select.getAttribute("style"));
        btn.id = select.id;
        btn.setAttribute("aria-haspopup", "listbox");
        btn.setAttribute("aria-expanded", "false");
        select.id = select.id ? select.id + "-native" : "";
        select.required = false;           // a hidden required select would block submit silently
        select.tabIndex = -1;
        select.setAttribute("aria-hidden", "true");
        select.style.display = "none";
        select.parentNode.insertBefore(btn, select.nextSibling);

        function label() {
            var o = select.options[select.selectedIndex];
            btn.textContent = o ? o.text : "";
        }
        label();

        function show() {
            if (!menu) {
                menu = document.createElement("ul");
                menu.className = "smenu-list";
                menu.setAttribute("role", "listbox");
                menu.hidden = true;
                document.body.appendChild(menu);
            }
            menu.innerHTML = "";
            var chosenLi = null, activeLi = null;
            Array.prototype.forEach.call(select.options, function (o) {
                var li = document.createElement("li");
                li.className = "smenu-opt" + (o.selected ? " is-chosen" : "") + (o.disabled ? " is-disabled" : "");
                li.setAttribute("role", "option");
                li.setAttribute("aria-selected", o.selected ? "true" : "false");
                li.textContent = o.text;
                if (o.selected) chosenLi = li;
                if (!o.disabled) li.addEventListener("mousedown", function (e) {
                    e.preventDefault();
                    var changed = select.value !== o.value;
                    select.value = o.value;
                    label();
                    closeMenu();
                    btn.focus();
                    if (changed) select.dispatchEvent(new Event("change", { bubbles: true }));
                });
                menu.appendChild(li);
            });
            var r = btn.getBoundingClientRect();
            menu.style.left = r.left + "px";
            menu.style.minWidth = r.width + "px";
            menu.style.top = (r.bottom + 4) + "px";
            menu.hidden = false;
            // not enough room below: open upward
            var mr = menu.getBoundingClientRect();
            if (mr.bottom > window.innerHeight - 8 && r.top > mr.height + 8) menu.style.top = (r.top - mr.height - 4) + "px";
            openFor = btn;
            btn.setAttribute("aria-expanded", "true");
            if (chosenLi) chosenLi.scrollIntoView({ block: "nearest" });
        }

        btn.addEventListener("click", function () { if (openFor === btn) closeMenu(); else { closeMenu(); show(); } });
        btn.addEventListener("keydown", function (e) {
            if (e.key === "Escape") { closeMenu(); return; }
            if (e.key === "ArrowDown" || e.key === "ArrowUp") {
                e.preventDefault();
                var i = select.selectedIndex + (e.key === "ArrowDown" ? 1 : -1);
                while (i >= 0 && i < select.options.length && select.options[i].disabled) i += (e.key === "ArrowDown" ? 1 : -1);
                if (i < 0 || i >= select.options.length) return;
                select.selectedIndex = i;
                label();
                if (openFor === btn) show();
                select.dispatchEvent(new Event("change", { bubbles: true }));
            }
        });
        btn.addEventListener("blur", closeMenu);
    });

    window.addEventListener("resize", closeMenu);
    window.addEventListener("scroll", function (e) { if (!(menu && e.target === menu)) closeMenu(); }, true);
}

function initForms() { initPasswordToggles(); initPasswordRules(); initRequiredMarkers(); initCoursePicker(); initComboboxes(); initSelectMenus(); }
if (window.jQuery) { jQuery(initForms); } else { document.addEventListener("DOMContentLoaded", initForms); }
