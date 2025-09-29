// --- SCRIPT FOR CONTENT MODAL ---
document.addEventListener('DOMContentLoaded', () => {

    const caseStudyModal = document.getElementById('caseStudyModal');
    const openBtn = document.getElementById('openModalBtn');
    const closeBtn = caseStudyModal ? caseStudyModal.querySelector('.close-btn') : null;

    if (caseStudyModal && openBtn && closeBtn) {
        openBtn.onclick = () => { caseStudyModal.style.display = 'block'; };
        closeBtn.onclick = () => { caseStudyModal.style.display = 'none'; };
        window.onclick = (event) => {
            if (event.target == caseStudyModal) {
                caseStudyModal.style.display = 'none';
            }
        };
    }
});

// --- SCRIPT FOR CAROUSEL IN MODAL ---
const carousel = document.querySelector('.carousel-container');
if (carousel) {
    const slides = carousel.querySelectorAll('.carousel-slide');
    const captions = document.querySelectorAll('.carousel-caption-text'); // Get all captions
    const prevBtn = carousel.querySelector('.prev-btn');
    const nextBtn = carousel.querySelector('.next-btn');
    let currentSlide = 0;

    function showSlide(index) {
        slides.forEach((slide, i) => {
            slide.classList.toggle('visible', i === index);
        });
        captions.forEach((caption, i) => {
            caption.classList.toggle('visible', i === index);
        });
    }

    function nextSlide() {
        currentSlide = (currentSlide + 1) % slides.length;
        showSlide(currentSlide);
    }

    function prevSlide() {
        currentSlide = (currentSlide - 1 + slides.length) % slides.length;
        showSlide(currentSlide);
    }

    if (prevBtn && nextBtn) {
        prevBtn.addEventListener('click', prevSlide);
        nextBtn.addEventListener('click', nextSlide);
    }

    showSlide(currentSlide); // Initialize first slide and caption
}

// --- SCRIPT FOR VIDEO TO PLAY ONCE AND FREEZE ON SCOLLING INTO VIEW ---
document.addEventListener('DOMContentLoaded', () => {
    // ... your existing modal and carousel scripts ...

    // --- SCRIPT FOR LAZY LOADING VIDEO (Play Once) ---
    const lazyVideo = document.querySelector('.lazy-video');

    if (lazyVideo) {
        const videoObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const video = entry.target;
                    const sources = video.querySelectorAll('source');

                    sources.forEach(source => {
                        if (source.dataset.src) {
                            source.src = source.dataset.src;
                        }
                    });

                    video.load();
                    video.addEventListener('canplay', () => {
                        video.play();
                    });

                    observer.unobserve(video);
                }
            });
        });

        videoObserver.observe(lazyVideo);
    }
});