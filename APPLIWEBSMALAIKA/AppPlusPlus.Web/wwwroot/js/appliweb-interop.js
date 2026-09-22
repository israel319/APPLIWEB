// Interop global — chargé avant Blazor (head) pour éviter les fonctions undefined
(function () {
    function readBarcodeValue(el) {
        if (!el) return "";
        if (typeof el.value === "string") return el.value.trim();
        return "";
    }

    function printFacture(id) {
        if (!id) return;
        var iframe = document.createElement("iframe");
        iframe.style.position = "fixed";
        iframe.style.width = "0";
        iframe.style.height = "0";
        iframe.style.border = "none";
        iframe.style.left = "-9999px";
        iframe.style.top = "0";
        iframe.src = "/vente/facture/print/" + id + "/true";
        document.body.appendChild(iframe);
        setTimeout(function () {
            if (iframe.parentNode) document.body.removeChild(iframe);
        }, 60000);
    }

    function printReport() {
        document.body.classList.add("print-all-rows");
        setTimeout(function () {
            window.print();
            document.body.classList.remove("print-all-rows");
        }, 100);
    }

    function posPrintTicket() {
        var root = document.getElementById("pos-print-root");
        if (!root) return;
        root.style.display = "block";
        setTimeout(function () {
            window.print();
            root.style.display = "none";
        }, 150);
    }

    window.barcodeScanner = {
        readValue: readBarcodeValue
    };

    window.printFacture = printFacture;
    window.printReport = printReport;
    window.posPrintTicket = posPrintTicket;

    window.appNavigation = {
        _progressTimer: null,
        _progressBound: false,
        backOrFallback: function (fallbackUrl) {
            try {
                var referrer = document.referrer || "";
                var sameOriginReferrer = referrer.indexOf(window.location.origin) === 0;
                if (window.history.length > 1 && sameOriginReferrer) {
                    window.history.back();
                    return;
                }
            } catch (e) {
            }
            window.location.href = fallbackUrl || "/";
        },
        scrollToElement: function (id) {
            var el = document.getElementById(id);
            if (el) el.scrollIntoView({ behavior: "smooth", block: "start" });
        },
        startProgress: function () {
            var bar = document.getElementById("app-nav-progress");
            if (!bar) return;
            bar.classList.add("is-active");
            if (window.appNavigation._progressTimer) {
                clearTimeout(window.appNavigation._progressTimer);
            }
            window.appNavigation._progressTimer = setTimeout(function () {
                window.appNavigation.finishProgress();
            }, 3200);
        },
        finishProgress: function () {
            var bar = document.getElementById("app-nav-progress");
            if (window.appNavigation._progressTimer) {
                clearTimeout(window.appNavigation._progressTimer);
                window.appNavigation._progressTimer = null;
            }
            if (bar) bar.classList.remove("is-active");
        },
        hideBootFallback: function () { },
        bindProgress: function () {
            if (window.appNavigation._progressBound) return;
            window.appNavigation._progressBound = true;
            document.addEventListener("click", function (e) {
                var link = e.target.closest("a[href]");
                if (!link || link.target === "_blank" || link.hasAttribute("download")) return;
                var href = link.getAttribute("href") || "";
                if (!href || href.charAt(0) === "#" || href.indexOf("javascript:") === 0) return;
                var isInternal = href.charAt(0) === "/" || href.indexOf(window.location.origin) === 0;
                if (!isInternal) return;
                window.appNavigation.startProgress();
            });
            window.addEventListener("pageshow", function () {
                window.appNavigation.finishProgress();
            });
            document.addEventListener("visibilitychange", function () {
                if (document.visibilityState === "visible") {
                    window.appNavigation.finishProgress();
                }
            });
        }
    };

    /**
     * Couche flottante globale — position:fixed + z-index élevé.
     * Évite le clipping par overflow/transform des ancêtres (workspace, cards, tables).
     */
    var awOverlayWatchers = Object.create(null);
    var awOverlayNextId = 1;

    function awOverlayMeasure(el) {
        if (!el || !el.getBoundingClientRect) return null;
        var r = el.getBoundingClientRect();
        return {
            top: r.top,
            bottom: r.bottom,
            left: r.left,
            right: r.right,
            width: r.width,
            height: r.height,
            viewportWidth: window.innerWidth || document.documentElement.clientWidth,
            viewportHeight: window.innerHeight || document.documentElement.clientHeight
        };
    }

    function awOverlayPortalToBody(el) {
        if (!el || !el.parentNode || el.parentNode === document.body) return;
        if (!el.__awOverlayHome) {
            el.__awOverlayHome = { parent: el.parentNode, next: el.nextSibling };
        }
        document.body.appendChild(el);
    }

    function awOverlayRestoreHome(el) {
        if (!el || !el.__awOverlayHome) return;
        var home = el.__awOverlayHome;
        delete el.__awOverlayHome;
        if (!home.parent || !home.parent.isConnected) return;
        try {
            if (home.next && home.next.parentNode === home.parent) {
                home.parent.insertBefore(el, home.next);
            } else {
                home.parent.appendChild(el);
            }
        } catch (e) { /* Blazor may already have moved the node */ }
    }

    function awOverlayHideLive(w) {
        if (!w) return;
        var backdropEl = awOverlayResolveBackdrop(w.panel, w.backdrop);
        if (backdropEl) {
            backdropEl.style.pointerEvents = "none";
            backdropEl.style.visibility = "hidden";
        }
        if (w.panel) {
            w.panel.style.pointerEvents = "none";
            w.panel.style.visibility = "hidden";
        }
    }

    function awOverlayTeardown(w) {
        if (!w) return;
        var backdropEl = awOverlayResolveBackdrop(w.panel, w.backdrop);
        if (backdropEl) {
            backdropEl.onclick = null;
            backdropEl.style.pointerEvents = "none";
            backdropEl.style.visibility = "hidden";
            awOverlayRestoreHome(backdropEl);
        }
        if (w.panel) {
            w.panel.classList.remove("aw-float-panel--ready");
            w.panel.style.pointerEvents = "none";
            w.panel.style.visibility = "hidden";
            awOverlayRestoreHome(w.panel);
        }
    }

    function awOverlayDismissWatcher(w) {
        if (!w || w.dismissed) return;
        w.dismissed = true;
        // Masquer tout de suite sans retirer du DOM — Blazor doit encore pouvoir removeChild.
        awOverlayHideLive(w);
        if (w.onPointerDown) {
            document.removeEventListener("pointerdown", w.onPointerDown, true);
        }
        if (w.dotNetRef) {
            w.dotNetRef.invokeMethodAsync("OnOverlayDismiss").catch(function () { });
        }
    }

    function awOverlayResolveBackdrop(panel, backdrop) {
        if (backdrop && document.body.contains(backdrop)) return backdrop;
        if (!panel) return null;
        var prev = panel.previousElementSibling;
        if (prev && prev.classList && prev.classList.contains("aw-float-backdrop")) return prev;
        return null;
    }

    function awOverlayOnPointerDown(w) {
        return function (e) {
            if (w.dismissed) return;
            var t = e.target;
            if (w.panel.contains(t) || w.trigger.contains(t)) return;
            awOverlayDismissWatcher(w);
        };
    }

    function awOverlayLayoutEntry(w) {
        if (w.dismissed) return;
        var trigger = w.trigger;
        var panel = w.panel;
        var opts = w.opts || {};
        if (!document.body.contains(trigger) || !document.body.contains(panel)) {
            awOverlayUnwatch(w.id);
            return;
        }
        if (!w.placedOnce) {
            panel.classList.remove("aw-float-panel--ready");
        }
        var backdropEl = awOverlayResolveBackdrop(panel, w.backdrop);
        if (backdropEl) {
            backdropEl.style.display = "";
            backdropEl.style.pointerEvents = "";
            backdropEl.style.visibility = "";
            awOverlayPortalToBody(backdropEl);
            backdropEl.style.position = "fixed";
            backdropEl.style.inset = "0";
            backdropEl.style.zIndex = String(opts.zBackdrop != null ? opts.zBackdrop : 10500);
            backdropEl.onclick = function (ev) {
                ev.stopPropagation();
                ev.preventDefault();
                awOverlayDismissWatcher(w);
            };
        }
        awOverlayPortalToBody(panel);
        panel.style.display = "";
        panel.style.pointerEvents = "";
        panel.style.visibility = "";
        awOverlayPlace(trigger, panel, opts);
        panel.classList.add("aw-float-panel--ready");
        w.placedOnce = true;
    }

    function awOverlayPlace(trigger, panel, opts) {
        opts = opts || {};
        var rect = awOverlayMeasure(trigger);
        if (!rect || !panel) return null;

        var gap = opts.gap != null ? opts.gap : 4;
        var pad = opts.pad != null ? opts.pad : 8;
        var align = opts.align || "start"; // start | end | stretch
        var prefer = opts.prefer || "below"; // below | above
        var vw = rect.viewportWidth;
        var vh = rect.viewportHeight;

        var width = opts.width != null ? opts.width : (align === "stretch" ? rect.width : (opts.minWidth || rect.width));
        if (opts.minWidth != null) width = Math.max(width, opts.minWidth);
        if (opts.maxWidth != null) width = Math.min(width, opts.maxWidth);
        width = Math.min(width, vw - pad * 2);

        var left;
        if (align === "end") {
            left = rect.right - width;
        } else if (align === "stretch") {
            left = rect.left;
            width = Math.min(Math.max(rect.width, opts.minWidth || 0), opts.maxWidth || (vw - pad * 2), vw - pad * 2);
        } else {
            left = rect.left;
        }
        left = Math.max(pad, Math.min(left, vw - width - pad));

        var estHeight = opts.estimatedHeight || panel.offsetHeight || 280;
        var maxHeight = opts.maxHeight != null ? opts.maxHeight : Math.min(360, vh - pad * 2);
        var spaceBelow = vh - rect.bottom - gap - pad;
        var spaceAbove = rect.top - gap - pad;
        var placeAbove = prefer === "above";
        if (!placeAbove && spaceBelow < Math.min(estHeight, 200) && spaceAbove > spaceBelow) {
            placeAbove = true;
        }
        if (placeAbove && spaceAbove < 120 && spaceBelow >= spaceAbove) {
            placeAbove = false;
        }

        var top;
        var available;
        if (placeAbove) {
            available = Math.max(120, spaceAbove);
            var h = Math.min(estHeight, maxHeight, available);
            top = Math.max(pad, rect.top - gap - h);
            maxHeight = Math.min(maxHeight, rect.top - gap - top);
        } else {
            top = rect.bottom + gap;
            available = Math.max(120, spaceBelow);
            maxHeight = Math.min(maxHeight, available);
        }

        var z = opts.zIndex != null ? opts.zIndex : 10510;
        panel.style.position = "fixed";
        panel.style.top = top + "px";
        panel.style.left = left + "px";
        panel.style.width = width + "px";
        panel.style.right = "auto";
        panel.style.bottom = "auto";
        panel.style.maxHeight = maxHeight + "px";
        panel.style.zIndex = String(z);
        panel.style.margin = "0";
        panel.classList.add("aw-float-panel");
        return { top: top, left: left, width: width, maxHeight: maxHeight, above: placeAbove };
    }

    function awOverlayWatch(trigger, panel, opts, backdrop, dotNetRef) {
        var id = awOverlayNextId++;
        var w = {
            id: id,
            trigger: trigger,
            panel: panel,
            backdrop: backdrop,
            opts: opts || {},
            dotNetRef: dotNetRef || null,
            dismissed: false,
            placedOnce: false
        };
        var fn = function () { awOverlayLayoutEntry(w); };
        w.fn = fn;
        awOverlayWatchers[id] = w;
        w.onPointerDown = awOverlayOnPointerDown(w);
        document.addEventListener("pointerdown", w.onPointerDown, true);
        window.addEventListener("resize", fn);
        window.addEventListener("scroll", fn, true);
        // Placement immédiat (évite le flash 0,0) puis recalage après layout
        fn();
        requestAnimationFrame(fn);
        return id;
    }

    function awOverlayUnwatch(id) {
        var w = awOverlayWatchers[id];
        if (!w) return;
        window.removeEventListener("resize", w.fn);
        window.removeEventListener("scroll", w.fn, true);
        if (w.onPointerDown) {
            document.removeEventListener("pointerdown", w.onPointerDown, true);
        }
        awOverlayTeardown(w);
        delete awOverlayWatchers[id];
    }

    function awOverlayCleanupStrays() {
        document.querySelectorAll("body > .aw-float-backdrop").forEach(function (el) {
            el.style.pointerEvents = "none";
            el.style.visibility = "hidden";
        });
    }

    /** Bootstrap 5 : strategy fixed pour sortir des overflow parents */
    function awOverlayConfigureBootstrapDropdowns(root) {
        if (typeof bootstrap === "undefined" || !bootstrap.Dropdown) return;
        var scope = root && root.querySelectorAll ? root : document;
        var toggles = scope.querySelectorAll('[data-bs-toggle="dropdown"]');
        toggles.forEach(function (toggle) {
            if (toggle.getAttribute("data-aw-overlay") === "1") return;
            toggle.setAttribute("data-aw-overlay", "1");
            var existing = bootstrap.Dropdown.getInstance(toggle);
            if (existing) existing.dispose();
            new bootstrap.Dropdown(toggle, {
                popperConfig: function (defaultConfig) {
                    var cfg = defaultConfig || {};
                    cfg.strategy = "fixed";
                    cfg.modifiers = (cfg.modifiers || []).concat([
                        { name: "computeStyles", options: { adaptive: false } }
                    ]);
                    return cfg;
                }
            });
        });
    }

    window.awOverlay = {
        measure: awOverlayMeasure,
        place: awOverlayPlace,
        watch: awOverlayWatch,
        unwatch: awOverlayUnwatch,
        configureBootstrapDropdowns: awOverlayConfigureBootstrapDropdowns,
        cleanupStrays: awOverlayCleanupStrays,
        Z_BACKDROP: 10500,
        Z_PANEL: 10510
    };

    window.awDateRange = {
        prefix: "aw.daterange.",
        save: function (key, from, to) {
            try {
                if (!key) return;
                localStorage.setItem(this.prefix + key, (from || "") + "|" + (to || ""));
            } catch (e) { }
        },
        load: function (key) {
            try {
                if (!key) return null;
                return localStorage.getItem(this.prefix + key);
            } catch (e) {
                return null;
            }
        },
        measure: awOverlayMeasure
    };

    window.ensureAppliWebInterop = function () {
        if (!window.barcodeScanner) {
            window.barcodeScanner = { readValue: readBarcodeValue };
        } else if (typeof window.barcodeScanner.readValue !== "function") {
            window.barcodeScanner.readValue = readBarcodeValue;
        }
        if (typeof window.printFacture !== "function") {
            window.printFacture = printFacture;
        }
        if (typeof window.printReport !== "function") {
            window.printReport = printReport;
        }
        if (typeof window.posPrintTicket !== "function") {
            window.posPrintTicket = posPrintTicket;
        }
        if (!window.awOverlay || typeof window.awOverlay.place !== "function") {
            window.awOverlay = {
                measure: awOverlayMeasure,
                place: awOverlayPlace,
                watch: awOverlayWatch,
                unwatch: awOverlayUnwatch,
                configureBootstrapDropdowns: awOverlayConfigureBootstrapDropdowns,
                Z_BACKDROP: 10500,
                Z_PANEL: 10510
            };
        }
        if (!window.awDateRange || typeof window.awDateRange.measure !== "function") {
            window.awDateRange = {
                prefix: "aw.daterange.",
                save: function (key, from, to) {
                    try {
                        if (!key) return;
                        localStorage.setItem(this.prefix + key, (from || "") + "|" + (to || ""));
                    } catch (e) { }
                },
                load: function (key) {
                    try {
                        if (!key) return null;
                        return localStorage.getItem(this.prefix + key);
                    } catch (e) {
                        return null;
                    }
                },
                measure: awOverlayMeasure
            };
        }
        if (!window.appNavigation || typeof window.appNavigation.backOrFallback !== "function") {
            window.appNavigation = window.appNavigation || {};
            window.appNavigation.backOrFallback = function (fallbackUrl) {
                window.location.href = fallbackUrl || "/";
            };
        }
        awOverlayCleanupStrays();
    };
})();
