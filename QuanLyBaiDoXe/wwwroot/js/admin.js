// ===== ADMIN SIDEBAR JAVASCRIPT =====

document.addEventListener('DOMContentLoaded', function () {
    // Sidebar Toggle for Mobile
    const sidebarToggle = document.querySelector('.sidebar-toggle');
    const sidebar = document.querySelector('.admin-sidebar');
    const overlay = document.querySelector('.sidebar-overlay');

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
            overlay.classList.toggle('show');
        });
    }

    if (overlay) {
        overlay.addEventListener('click', function () {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        });
    }

    // Submenu Toggle
    const submenuItems = document.querySelectorAll('.sidebar-nav-item.has-submenu');

    submenuItems.forEach(function (item) {
        const link = item.querySelector('.sidebar-nav-link');
        
        link.addEventListener('click', function (e) {
            e.preventDefault();
            
            // Close other submenus (optional - for accordion style)
            submenuItems.forEach(function (otherItem) {
                if (otherItem !== item) {
                    otherItem.classList.remove('open');
                }
            });

            // Toggle current submenu
            item.classList.toggle('open');
        });
    });

    // Set active state based on current URL
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar-nav-link');

    navLinks.forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href) && href !== '/') {
            link.classList.add('active');
            
            // Open parent submenu if exists
            const parentSubmenu = link.closest('.sidebar-nav-item.has-submenu');
            if (parentSubmenu) {
                parentSubmenu.classList.add('open');
            }
        }
    });

    // Search functionality placeholder
    const searchInput = document.querySelector('.header-search input');
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                const searchTerm = this.value.trim();
                if (searchTerm) {
                    // Implement search functionality
                    console.log('Searching for:', searchTerm);
                }
            }
        });
    }

    // Initialize tooltips if Bootstrap is available
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
});
