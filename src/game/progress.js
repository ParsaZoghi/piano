import { LESSONS } from './lessons.js';

const KEY = 'keystage.progress';

function load() {
    try {
        return JSON.parse(localStorage.getItem(KEY) || '{}');
    } catch {
        return {};
    }
}

const data = load();

export function starsFor(accuracy) {
    if (accuracy >= 0.9) return 3;
    if (accuracy >= 0.75) return 2;
    if (accuracy >= 0.5) return 1;
    return 0;
}

export const progress = {
    stars: (id) => data[id]?.stars ?? 0,
    best: (id) => data[id]?.best ?? 0,

    /** A lesson opens once the one before it has at least one star. */
    isUnlocked(index) {
        return index <= 0 || this.stars(LESSONS[index - 1].id) > 0;
    },

    totalStars() {
        return LESSONS.reduce((sum, lesson) => sum + this.stars(lesson.id), 0);
    },

    record(id, accuracy) {
        const stars = starsFor(accuracy);
        const previous = data[id] || { stars: 0, best: 0 };
        data[id] = { stars: Math.max(previous.stars, stars), best: Math.max(previous.best, accuracy) };
        try {
            localStorage.setItem(KEY, JSON.stringify(data));
        } catch {
            // Progress simply lasts for this session if storage is unavailable.
        }
        return stars;
    },
};
