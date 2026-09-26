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
