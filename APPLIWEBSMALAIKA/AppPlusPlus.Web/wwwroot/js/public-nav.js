let scrollHandler = null;
let spyHandler = null;
let spyClickTimer = null;

function prefersReducedMotion() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

function getShell() {
    return document.querySelector(".landing-shell");
}

function bindLandingScroll() {
    const shell = getShell();
    if (!shell || scrollHandler) return;

    scrollHandler = () => {
        shell.classList.toggle("landing-shell--scrolled", window.scrollY > 12);
    };

    window.addEventListener("scroll", scrollHandler, { passive: true });
    scrollHandler();
}

function unbindLandingScroll() {
    if (!scrollHandler) return;
    window.removeEventListener("scroll", scrollHandler);
    scrollHandler = null;
}

function sectionAnchors() {
    return [
        { id: "accueil", el: document.querySelector(".home-hero") },
        { id: "produits", el: document.getElementById("produits") },
        { id: "ecoles", el: document.getElementById("ecoles") },
        { id: "grh", el: document.getElementById("grh") },
        { id: "modules", el: document.getElementById("modules") },
        { id: "contact", el: document.getElementById("contact") }
    ].filter((s) => s.el);
}

function navLinksFor(sectionId) {
    return document.querySelectorAll(`[data-nav-section="${sectionId}"]`);
}

function setScrollSpySection(sectionId) {
    document.querySelectorAll("[data-nav-section]").forEach((link) => {
        const active = link.getAttribute("data-nav-section") === sectionId;
        link.classList.toggle("landing-header-link--scroll-active", active);
        link.setAttribute("aria-current", active ? "true" : "false");
    });
}

function resolveSpySection() {
    const offset = 96;
    const scrollPos = window.scrollY + offset;
    const sections = sectionAnchors();

    let active = sections[0]?.id ?? "accueil";
    for (const section of sections) {
        const top = section.el.getBoundingClientRect().top + window.scrollY;
        if (scrollPos >= top) active = section.id;
    }

    return active;
}

function bindHomeScrollSpy() {
    const shell = getShell();
    const home = document.querySelector(".home-page");
    if (!shell || !home || spyHandler) return;

    shell.classList.add("landing-shell--spy-ready");

    const update = () => {
        if (spyClickTimer) return;
        setScrollSpySection(resolveSpySection());
    };

    spyHandler = update;
    window.addEventListener("scroll", spyHandler, { passive: true });
    window.addEventListener("resize", spyHandler, { passive: true });

    document.querySelectorAll('a[href^="#"]').forEach((link) => {
        link.addEventListener("click", (event) => {
            const href = link.getAttribute("href");
            if (!href || href === "#") return;

            const targetId = href.slice(1);
            const section = sectionAnchors().find((s) => s.id === targetId);
            if (!section) return;

            event.preventDefault();
            spyClickTimer = window.setTimeout(() => {
                spyClickTimer = null;
            }, 700);

            setScrollSpySection(targetId);
            section.el.scrollIntoView({
                behavior: prefersReducedMotion() ? "auto" : "smooth",
                block: "start"
            });

            if (history.replaceState) {
                history.replaceState(null, "", `#${targetId}`);
            } else {
                location.hash = targetId;
            }
        });
    });

    const hash = location.hash.replace("#", "");
    if (hash && sectionAnchors().some((s) => s.id === hash)) {
        setScrollSpySection(hash);
        requestAnimationFrame(() => {
            document.getElementById(hash)?.scrollIntoView({ block: "start" });
        });
    } else {
        update();
    }
}

function unbindHomeScrollSpy() {
    if (spyHandler) {
        window.removeEventListener("scroll", spyHandler);
        window.removeEventListener("resize", spyHandler);
        spyHandler = null;
    }

    if (spyClickTimer) {
        clearTimeout(spyClickTimer);
        spyClickTimer = null;
    }

    getShell()?.classList.remove("landing-shell--spy-ready");
    document.querySelectorAll("[data-nav-section]").forEach((link) => {
        link.classList.remove("landing-header-link--scroll-active");
        link.removeAttribute("aria-current");
    });
}

export function scrollToId(id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.scrollIntoView({
        behavior: prefersReducedMotion() ? "auto" : "smooth",
        block: "start"
    });
}

export function init() {
    dispose();
    bindLandingScroll();
    bindHomeScrollSpy();
}

export function dispose() {
    unbindLandingScroll();
    unbindHomeScrollSpy();
}
