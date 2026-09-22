let revealObserver = null;
let parallaxFrame = null;
let parallaxHandler = null;

function prefersReducedMotion() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

function bindImageFallbacks(root) {
    root.querySelectorAll(".home-visual__img").forEach((img) => {
        const markLoaded = () => img.classList.add("is-loaded");
        const markFallback = () => {
            img.classList.add("is-fallback");
            img.closest(".home-visual")?.classList.add("home-visual--fallback");
        };

        if (img.complete && img.naturalWidth > 0) markLoaded();
        else if (img.complete) markFallback();

        img.addEventListener("load", markLoaded, { once: true });
        img.addEventListener("error", markFallback, { once: true });
    });
}

function bindReveal(root) {
    const items = root.querySelectorAll("[data-reveal]");
    if (!items.length) return;

    if (prefersReducedMotion()) {
        items.forEach((el) => el.classList.add("is-visible"));
        return;
    }

    revealObserver = new IntersectionObserver(
        (entries) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                entry.target.classList.add("is-visible");
                revealObserver?.unobserve(entry.target);
            });
        },
        { threshold: 0.14, rootMargin: "0px 0px -48px 0px" }
    );

    items.forEach((el, index) => {
        const group = Number(el.getAttribute("data-reveal-group") || index % 5);
        el.style.setProperty("--reveal-delay", `${group * 90}ms`);
        revealObserver.observe(el);
    });
}

function bindHeroParallax(root) {
    const hero = root.querySelector(".home-hero");
    const media = root.querySelector(".home-hero__media");
    if (!hero || !media || prefersReducedMotion()) return;

    parallaxHandler = () => {
        if (parallaxFrame) return;
        parallaxFrame = requestAnimationFrame(() => {
            parallaxFrame = null;
            const rect = hero.getBoundingClientRect();
            const progress = Math.min(1, Math.max(0, (window.innerHeight - rect.top) / (window.innerHeight + rect.height)));
            media.style.setProperty("--parallax-y", `${(progress - 0.5) * 36}px`);
        });
    };

    window.addEventListener("scroll", parallaxHandler, { passive: true });
    parallaxHandler();
}

export function init(selector) {
    const root = document.querySelector(selector);
    if (!root) return;

    bindImageFallbacks(root);
    root.classList.add("home-anim-ready");
    bindReveal(root);
    bindHeroParallax(root);
}

export function dispose() {
    if (revealObserver) {
        revealObserver.disconnect();
        revealObserver = null;
    }
    if (parallaxFrame) {
        cancelAnimationFrame(parallaxFrame);
        parallaxFrame = null;
    }
    if (parallaxHandler) {
        window.removeEventListener("scroll", parallaxHandler);
        parallaxHandler = null;
    }
}
