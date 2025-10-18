// js/modal.js

document.addEventListener('DOMContentLoaded', () => {
    const lightbox = document.getElementById('image-lightbox');
    if (!lightbox) return;

    const lightboxImg = document.getElementById('lightbox-img');
    const lightboxCaption = document.getElementById('lightbox-caption');
    const closeBtn = document.querySelector('.lightbox-close');
    const triggerImages = document.querySelectorAll('.enlarge-on-click');

    if (!lightboxImg || !lightboxCaption || !closeBtn) return;

    const openModal = (imgElement) => {
        // --- CHANGE IS HERE ---
        // Check for a high-quality source, otherwise fall back to the original src.
        const largeSrc = imgElement.getAttribute('data-large-src');
        const imgSrc = largeSrc || imgElement.src;
        // --- END OF CHANGE ---

        const captionText = imgElement.dataset.caption || imgElement.nextElementSibling?.innerHTML || '';

        lightboxImg.src = imgSrc;
        lightboxImg.alt = captionText;
        lightboxCaption.innerHTML = captionText;

        // Use classList to trigger the CSS transition
        lightbox.classList.add('visible');
    };

    const closeModal = () => {
        // Use classList to trigger the CSS transition
        lightbox.classList.remove('visible');
    };

    triggerImages.forEach(img => {
        img.addEventListener('click', () => openModal(img));
    });

    closeBtn.addEventListener('click', closeModal);

    lightbox.addEventListener('click', (e) => {
        if (e.target === lightbox) {
            closeModal();
        }
    });

    document.addEventListener('keydown', (e) => {
        // Check for the .visible class instead of the display style
        if (e.key === 'Escape' && lightbox.classList.contains('visible')) {
            closeModal();
        }
    });
});