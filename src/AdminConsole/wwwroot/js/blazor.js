// Get references to elements
const sidebar = document.getElementById('sidebar');
const sidebarOverlay = document.getElementById('sidebar-overlay');
const btnSidebarOpen = document.getElementById('btn-sidebar-open');
const navbar = document.getElementById('navbar-mobile');

// Event listener for the open button
btnSidebarOpen.addEventListener('click', () => {
    sidebar.classList.remove('hidden');
    sidebar.classList.add('z-50', 'absolute', 'inset-y-0', 'flex', 'flex-col', 'h-full');
    navbar.classList.add('hidden');
    sidebarOverlay.classList.remove('hidden');
});

const closeSidebar = () => {
    sidebar.classList.add('hidden');
    sidebar.classList.remove('z-50', 'absolute', 'inset-y-0', 'flex', 'flex-col', 'h-full');
    sidebarOverlay.classList.add('hidden');
    navbar.classList.remove('hidden');
}

sidebarOverlay.addEventListener('click', closeSidebar);
window.addEventListener('resize', closeSidebar);
