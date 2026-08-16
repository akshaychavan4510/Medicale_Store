// site.js  – global scripts

document.addEventListener('DOMContentLoaded', function () {

    // ── Sidebar Toggle ────────────────────────────────────────────────
    const toggleBtn = document.getElementById('sidebarToggle');
    const wrapper = document.getElementById('wrapper');
    if (toggleBtn && wrapper) {
        toggleBtn.addEventListener('click', function () {
            wrapper.classList.toggle('sidebar-collapsed');
        });
    }

    // ── Auto-dismiss success/info alerts after 4 seconds ─────────────
    setTimeout(function () {
        document.querySelectorAll('.alert-success, .alert-info').forEach(function (el) {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            if (bsAlert) bsAlert.close();
        });
    }, 4000);

    // ── Confirm delete on any form with data-confirm attribute ────────
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            const msg = form.getAttribute('data-confirm') || 'Are you sure?';
            if (!confirm(msg)) e.preventDefault();
        });
    });

    // ── Highlight active nav link (fallback for sidebar) ─────────────
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('#sidebar-wrapper .list-group-item').forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && currentPath.startsWith(href.toLowerCase()) && href !== '/') {
            link.classList.add('active');
        }
    });
});
