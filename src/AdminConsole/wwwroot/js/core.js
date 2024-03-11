// Get references to elements
const sidebar = document.getElementById('sidebar');
const sidebarOverlay = document.getElementById('sidebar-overlay');
const btnSidebarOpen = document.getElementById('btn-sidebar-open');
const navbar = document.getElementById('navbar-mobile');

// Event listener for the open button
if (btnSidebarOpen) {
    btnSidebarOpen.addEventListener('click', () => {
        sidebar.classList.remove('hidden');
        sidebar.classList.add('z-50', 'absolute', 'inset-y-0', 'flex', 'flex-col', 'h-full');
        navbar.classList.add('hidden');
        sidebarOverlay.classList.remove('hidden');
    });
}

const closeSidebar = () => {
    if (sidebar) {
        sidebar.classList.add('hidden');
        sidebar.classList.remove('z-50', 'absolute', 'inset-y-0', 'flex', 'flex-col', 'h-full');
    }
    if (sidebarOverlay) {
        sidebarOverlay.classList.add('hidden');
    }
    if (navbar) {
        navbar.classList.remove('hidden');
    }
}

if (sidebarOverlay) {
    sidebarOverlay.addEventListener('click', closeSidebar);
}

window.addEventListener('resize', closeSidebar);

// START Form submit only once
const forms = document.querySelectorAll('form[only-once]');
forms.forEach(function (form) {
    form.addEventListener('submit', function (e) {
        // check if disabled, if not, disable and submit
        if (!form.classList.contains('disabled')) {
            form.classList.add('disabled');
            form.submit();
        } else {
            e.preventDefault();
        }
        return false;
    });
});
// END Form submit only once