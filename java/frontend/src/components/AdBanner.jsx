import React, { useState, useEffect } from 'react';
import styles from '../styles/AdBanner.module.css';
import { FaChevronLeft, FaChevronRight } from 'react-icons/fa';

const AdBanner = () => {
    const [currentSlide, setCurrentSlide] = useState(0);
    const [isPaused, setIsPaused] = useState(false);

    const slides = [
        {
            id: 1,
            headline: "Mega Electronics Sale",
            description: "Experience the future with up to 50% off on premium gadgets. Upgrade your tech game today.",
            image: "https://placehold.co/800x600/0078d4/fff?text=Tech+Deals",
            primaryBtn: "Shop Now",
            secondaryBtn: "View Offers",
            bgColor: "linear-gradient(135deg, #fdfbfb 0%, #ebedee 100%)" // Clean Silver-White
        },
        {
            id: 2,
            headline: "Fashion That Speaks",
            description: "Discover the latest trends in clothing and accessories. Redefine your style with our new collection.",
            image: "https://placehold.co/800x600/e91e63/fff?text=New+Arrivals",
            primaryBtn: "Shop Fashion",
            secondaryBtn: "New Arrivals",
            bgColor: "linear-gradient(to top, #fad0c4 0%, #ffd1ff 100%)" // Soft Pink/Peach
        },
        {
            id: 3,
            headline: "Smart Home Revolution",
            description: "Automate your life with top-rated smart assistants and security systems. Comfort meets technology.",
            image: "https://placehold.co/800x600/4caf50/fff?text=Smart+Living",
            primaryBtn: "Shop Smart",
            secondaryBtn: "Learn More",
            bgColor: "linear-gradient(120deg, #d4fc79 0%, #96e6a1 100%)" // Fresh Green
        },
        {
            id: 4,
            headline: "Glow & Shine Beauty",
            description: "Unlock your radiance with our curated selection of premium skincare and makeup brands.",
            image: "https://placehold.co/800x600/9c27b0/fff?text=Beauty+Care",
            primaryBtn: "Shop Beauty",
            secondaryBtn: "Top Rated",
            bgColor: "linear-gradient(to top, #e6e9f0 0%, #eef1f5 100%)" // Soft Blue-Grey
        },
        {
            id: 5,
            headline: "Fitness & Performance",
            description: "Push your limits with high-performance gear. Equipment and apparel for every athlete.",
            image: "https://placehold.co/800x600/ff9800/fff?text=Sport+Mode",
            primaryBtn: "Shop Sports",
            secondaryBtn: "Deals",
            bgColor: "linear-gradient(120deg, #f6d365 0%, #fda085 100%)" // Warm Orange/Yellow
        }
    ];

    useEffect(() => {
        let interval;
        if (!isPaused) {
            interval = setInterval(() => {
                setCurrentSlide((prev) => (prev === slides.length - 1 ? 0 : prev + 1));
            }, 3000);
        }
        return () => clearInterval(interval);
    }, [isPaused, slides.length]);

    const nextSlide = () => {
        setCurrentSlide((prev) => (prev === slides.length - 1 ? 0 : prev + 1));
    };

    const prevSlide = () => {
        setCurrentSlide((prev) => (prev === 0 ? slides.length - 1 : prev - 1));
    };

    const goToSlide = (index) => {
        setCurrentSlide(index);
    };

    return (
        <div
            className={styles.bannerContainer}
            onMouseEnter={() => setIsPaused(true)}
            onMouseLeave={() => setIsPaused(false)}
        >
            <div
                className={styles.slideTrack}
                style={{ transform: `translateX(-${currentSlide * 100}%)` }}
            >
                {slides.map((slide) => (
                    <div
                        key={slide.id}
                        className={styles.slide}
                        style={{ backgroundColor: slide.bgColor }}
                    >
                        <div className={styles.textSection}>
                            <h2 className={styles.headline}>{slide.headline}</h2>
                            <p className={styles.description}>{slide.description}</p>
                            <div className={styles.buttonGroup}>
                                <button className={styles.primaryBtn}>{slide.primaryBtn}</button>
                                <button className={styles.secondaryBtn}>{slide.secondaryBtn}</button>
                            </div>
                        </div>
                        <div className={styles.imageSection}>
                            <img src={slide.image} alt={slide.headline} className={styles.bannerImage} />
                        </div>
                    </div>
                ))}
            </div>

            {/* Navigation Arrows */}
            <button className={`${styles.arrow} ${styles.leftArrow}`} onClick={prevSlide}>
                <FaChevronLeft />
            </button>
            <button className={`${styles.arrow} ${styles.rightArrow}`} onClick={nextSlide}>
                <FaChevronRight />
            </button>

            {/* Dots */}
            <div className={styles.dotsContainer}>
                {slides.map((_, index) => (
                    <div
                        key={index}
                        className={`${styles.dot} ${currentSlide === index ? styles.active : ''}`}
                        onClick={() => goToSlide(index)}
                    />
                ))}
            </div>
        </div>
    );
};

export default AdBanner;
