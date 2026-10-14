const KEY = 'keystage.settings';

const defaults = {
    volume: 0.8,
    labels: true,
    hints: true,
    metronome: true,
    wait: false,
    speed: 1,
};

function load() {
    try {
        return { ...defaults, ...JSON.parse(localStorage.getItem(KEY) || '{}') };
    } catch {
        return { ...defaults };
    }
}

export const settings = load();

export function saveSettings(patch) {
    Object.assign(settings, patch);
    try {
        localStorage.setItem(KEY, JSON.stringify(settings));
    } catch {
        // Storage can be unavailable (private mode); the session still works.
    }
}
