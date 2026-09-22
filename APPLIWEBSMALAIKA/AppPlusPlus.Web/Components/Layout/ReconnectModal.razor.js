const reconnectModal = document.getElementById("components-reconnect-modal");
const retryButton = document.getElementById("components-reconnect-button");
const resumeButton = document.getElementById("components-resume-button");

const QUIET_PATH_PREFIXES = [
    "/auth/connexion",
    "/login",
    "/client/inscription",
    "/client/connexion"
];

const SHOW_MODAL_DELAY_MS = 6000;

let showModalTimer = null;

if (reconnectModal) {
    reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);
}

if (retryButton) {
    retryButton.addEventListener("click", retry);
}

if (resumeButton) {
    resumeButton.addEventListener("click", resume);
}

updateQuietMode();
window.addEventListener("popstate", updateQuietMode);

function updateQuietMode() {
    const quiet = isQuietReconnectPage();
    document.documentElement.classList.toggle("reconnect-quiet", quiet);
}

function isQuietReconnectPage() {
    const path = (location.pathname || "/").toLowerCase().replace(/\/+$/, "") || "/";
    return QUIET_PATH_PREFIXES.some(prefix =>
        path === prefix || path.startsWith(prefix + "/"));
}

function clearReconnectTimers() {
    if (showModalTimer) {
        clearTimeout(showModalTimer);
        showModalTimer = null;
    }
}

function handleReconnectStateChanged(event) {
    if (!reconnectModal) {
        return;
    }

    const state = event.detail.state;

    if (state === "show") {
        clearReconnectTimers();
        reconnectModal.classList.remove("reconnect-visible");
        reconnectModal.close();

        if (isQuietReconnectPage()) {
            return;
        }

        showModalTimer = setTimeout(() => {
            reconnectModal.classList.add("reconnect-visible");
            if (!reconnectModal.open) {
                reconnectModal.showModal();
            }
        }, SHOW_MODAL_DELAY_MS);
        return;
    }

    if (state === "hide") {
        clearReconnectTimers();
        reconnectModal.classList.remove("reconnect-visible");
        reconnectModal.close();
        return;
    }

    if (state === "failed") {
        if (isQuietReconnectPage()) {
            location.reload();
            return;
        }

        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
        return;
    }

    if (state === "rejected") {
        location.reload();
    }
}

async function retry() {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);

    try {
        const successful = await Blazor.reconnect();
        if (!successful) {
            const resumeSuccessful = await Blazor.resumeCircuit();
            if (!resumeSuccessful) {
                location.reload();
            } else if (reconnectModal) {
                reconnectModal.close();
            }
        } else if (reconnectModal) {
            reconnectModal.close();
        }
    } catch {
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    }
}

async function resume() {
    try {
        const successful = await Blazor.resumeCircuit();
        if (!successful) {
            location.reload();
        }
    } catch {
        location.reload();
    }
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        await retry();
    }
}
