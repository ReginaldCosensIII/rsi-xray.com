document.addEventListener('DOMContentLoaded', () => {

    // --- SCRIPT FOR MOBILE HAMBURGER MENU ---
    const mainNavList = document.querySelector('#main-nav-list');
    const navToggle = document.querySelector('.mobile-nav-toggle');

    if (navToggle && mainNavList) {
        navToggle.addEventListener('click', () => {
            const isVisible = mainNavList.getAttribute('data-visible');

            if (isVisible === 'false') {
                mainNavList.setAttribute('data-visible', 'true');
                navToggle.setAttribute('aria-expanded', 'true');
            } else {
                mainNavList.setAttribute('data-visible', 'false');
                navToggle.setAttribute('aria-expanded', 'false');
            }
        });
    }

    // --- SCRIPT FOR MOBILE NAVIGATION DROPDOWN ---
    const dropdownToggle = document.querySelector('.main-nav .dropdown > a');

    if (dropdownToggle) {
        dropdownToggle.addEventListener('click', (e) => {
            // Check if we are in mobile view (matches CSS breakpoint)
            if (window.innerWidth <= 1100) {
                // Prevent the link from navigating to products.html
                e.preventDefault();

                // Toggle the 'open' class on the parent <li>
                const parentLi = dropdownToggle.parentElement;
                parentLi.classList.toggle('open');
            }
            // On screens wider than 1100px, this script does nothing,
            // and the link behaves as a normal <a> tag.
        });
    }
});