document.addEventListener('DOMContentLoaded', () => {
    const carouselContainer = document.querySelector('.spotlight-carousel-container');
    if (!carouselContainer) return;

    const slides = carouselContainer.querySelectorAll('.tech-spotlight');
    const dotsContainer = carouselContainer.querySelector('.carousel-dots');
    let currentSlide = 0;
    let slideInterval;

    // Create a dot for each slide
    slides.forEach((slide, index) => {
        const dot = document.createElement('span');
        dot.classList.add('dot');
        dot.setAttribute('data-slide', index);
        dotsContainer.appendChild(dot);
    });

    const dots = dotsContainer.querySelectorAll('.dot');

    function showSlide(index, direction = 'next') {
        const oldSlide = slides[currentSlide];

        if (index === currentSlide && oldSlide.classList.contains('active')) return;

        // Animate out the old slide based on direction
        if (oldSlide) {
            oldSlide.classList.remove('active');
            oldSlide.classList.add(direction === 'next' ? 'exit-to-left' : 'exit-to-right');
        }

        // Animate in the new slide based on direction
        const newSlide = slides[index];
        // Set the starting position before the animation begins
        newSlide.classList.remove('exit-to-left', 'exit-to-right');
        newSlide.classList.add(direction === 'next' ? 'enter-from-right' : 'enter-from-left');

        // Force a reflow to ensure the browser applies the start state before transitioning
        newSlide.offsetHeight;

        // Trigger the animation to the active state
        newSlide.classList.add('active');
        newSlide.classList.remove('enter-from-right', 'enter-from-left');

        // Update dots
        dots.forEach((dot, i) => {
            dot.classList.toggle('active', i === index);
        });

        currentSlide = index;
    }

    function startSlideShow() {
        slideInterval = setInterval(() => {
            const nextSlideIndex = (currentSlide + 1) % slides.length;
            showSlide(nextSlideIndex, 'next');
        }, 8000);
    }

    function resetInterval() {
        clearInterval(slideInterval);
        startSlideShow();
    }

    // Event listeners for dots
    dots.forEach(dot => {
        dot.addEventListener('click', () => {
            const slideIndex = parseInt(dot.getAttribute('data-slide'));
            // Always animate forward for a continuous loop effect
            showSlide(slideIndex, 'next');
            resetInterval();
        });
    });

    // Initialize the carousel
    slides[0].classList.add('active');
    dots[0].classList.add('active');
    startSlideShow();
});

