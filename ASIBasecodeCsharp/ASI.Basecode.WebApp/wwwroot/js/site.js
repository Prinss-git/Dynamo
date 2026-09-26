let dpicn = document.querySelector(".dpicn");
let dropdown = document.querySelector(".dropdown");

if (dpicn && dropdown) {
    dpicn.addEventListener("click", () => {
        dropdown.classList.toggle("dropdown-open");
    });
}

// Collapsible sidebar on small screens
let sidebarToggle = document.getElementById("sidebarToggle");
if (sidebarToggle) {
    sidebarToggle.addEventListener("click", () => {
        document.body.classList.toggle("sidebar-open");
    });
}

// Ask before destructive actions: <form data-confirm="Are you sure?">
document.addEventListener("submit", (e) => {
    const message = e.target.getAttribute("data-confirm");
    if (message && !window.confirm(message)) {
        e.preventDefault();
    }
});
