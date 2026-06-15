// PharmaTrack Pro - shared client-side behavior

document.addEventListener("DOMContentLoaded", function () {
    var sidebar = document.getElementById("sidebar");
    var toggleBtn = document.getElementById("sidebarToggle");

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener("click", function () {
            if (window.innerWidth <= 768) {
                sidebar.classList.toggle("mobile-open");
            } else {
                sidebar.classList.toggle("collapsed");
            }
        });
    }
});

// Small helper other modules can reuse for confirm-delete style actions.
function ptConfirm(message, onConfirm) {
    if (typeof Swal === "undefined") {
        if (confirm(message)) onConfirm();
        return;
    }

    Swal.fire({
        title: "Are you sure?",
        text: message,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#0d6efd",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Yes, proceed"
    }).then(function (result) {
        if (result.isConfirmed) onConfirm();
    });
}
