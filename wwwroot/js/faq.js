document.addEventListener('DOMContentLoaded', () => {
    const accordionItems = document.querySelectorAll('.accordion-item');

    accordionItems.forEach(item => {
        const header = item.querySelector('.accordion-header');
        const content = item.querySelector('.accordion-content');

        header.addEventListener('click', () => {
            const isExpanded = header.getAttribute('aria-expanded') === 'true';

            header.setAttribute('aria-expanded', !isExpanded);
            if (!isExpanded) {
                content.style.maxHeight = content.scrollHeight + 'px';
            } else {
                content.style.maxHeight = '0';
            }
        });
    });
});

document.addEventListener('DOMContentLoaded', () => {
    // --- Image Lightbox Functionality ---
    const lightbox = document.getElementById('image-lightbox');
    const lightboxImg = document.getElementById('lightbox-img');
    const lightboxCaption = document.getElementById('lightbox-caption');
    const closeBtn = document.querySelector('.lightbox-close');

    // Get all images in the showcase grid
    const gridImages = document.querySelectorAll('#faq-project-showcase .grid-item img');

    gridImages.forEach(img => {
        img.addEventListener('click', () => {
            lightbox.style.display = 'block';
            lightboxImg.src = img.src;
            // Get the caption from the sibling .grid-item-caption element
            const captionEl = img.nextElementSibling;
            if (captionEl) {
                lightboxCaption.innerHTML = captionEl.innerHTML;
            }
        });
    });

    // Function to close the modal
    const closeModal = () => {
        lightbox.style.display = 'none';
    };

    // Close the modal when the close button is clicked
    if (closeBtn) {
        closeBtn.addEventListener('click', closeModal);
    }

    // Close the modal when clicking outside the image
    if (lightbox) {
        lightbox.addEventListener('click', (e) => {
            if (e.target === lightbox) {
                closeModal();
            }
        });
    }
});