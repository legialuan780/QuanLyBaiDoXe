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
            //submenuItems.forEach(function (otherItem) {
            //    if (otherItem !== item) {
            //        otherItem.classList.remove('open');
            //    }
            //});

            // Toggle current submenu
            item.classList.toggle('open');
        });
    });

    // Set active state based on current URL
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.sidebar-nav-link');

    //remove all existing active class first
    navLinks.forEach(function (link) {
        link.classList.remove('active');
    });

    // find the best matching link
    let bestMatch = null;
    let bestMatchLength = 0;

    navLinks.forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && href!=='#') {
            const hrefLower = href.toLowerCase();
            
            //check for exact match or if curretn path start with href
            if (currentPath === hrefLower ||
                (currentPath.startsWith(hrefLower) && hrefLower.length > bestMatchLength)) {
                bestMatch = link;
                bestMatchLength = hrefLower.length;
            }
        }
    });

    // Apply active class to best match
    if (bestMatch) {
        bestMatch.classList.add('active');

        // Open parent submenu if exists
        const parentSubmenu = bestMatch.closest('.sidebar-nav-item.has-submenu');
        if (parentSubmenu) {
            parentSubmenu.classList.add('open');
        }
    }

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
