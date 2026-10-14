const KEY = 'keystage.settings';

const defaults = {
    volume: 0.8,
    labels: true,
    hints: true,
    metronome: true,
    wait: false,
    speed: 1,
    theme: 'dark', // dark | light
    lang: 'en', // en | fa
    naming: 'letters', // letters (C D E) | solfege (Do Re Mi)
};

function load() {
    try {
        return { ...defaults, ...JSON.parse(localStorage.getItem(KEY) || '{}') };
    } catch {
        return { ...defaults };
    }
}

export const settings = load();
const listeners = new Set();

/** fn(patch) runs after every change, with just the keys that changed. */
export function onSettingsChange(fn) {
    listeners.add(fn);
    return () => listeners.delete(fn);
}

export function saveSettings(patch) {
    Object.assign(settings, patch);
    try {
        localStorage.setItem(KEY, JSON.stringify(settings));
    } catch {
        // Storage can be unavailable (private mode); the session still works.
    }
    for (const fn of [...listeners]) fn(patch);
}
