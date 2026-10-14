import { isBlack, noteName, staffStep } from '../core/notes.js';

const BOTTOM_LINE = 66; // y of E4
const HALF_SPACE = 5;
const NOTE_X = 82;

/** Treble staff with one note on it (or empty when midi is null). */
export function staffSvg(midi) {
    let lines = '';
    for (let i = 0; i < 5; i++) {
        const y = BOTTOM_LINE - i * HALF_SPACE * 2;
        lines += `<line x1="6" x2="124" y1="${y}" y2="${y}"/>`;
    }

    let note = '';
    let label = 'Empty staff';
    if (midi != null) {
        const step = staffStep(midi);
        const y = BOTTOM_LINE - step * HALF_SPACE;
        label = `${noteName(midi)} on the treble staff`;
        // Ledger lines for notes below or above the five lines.
        for (let s = -2; s >= step; s -= 2) note += `<line x1="${NOTE_X - 12}" x2="${NOTE_X + 12}" y1="${BOTTOM_LINE - s * HALF_SPACE}" y2="${BOTTOM_LINE - s * HALF_SPACE}"/>`;
        for (let s = 10; s <= step; s += 2) note += `<line x1="${NOTE_X - 12}" x2="${NOTE_X + 12}" y1="${BOTTOM_LINE - s * HALF_SPACE}" y2="${BOTTOM_LINE - s * HALF_SPACE}"/>`;
        const up = step < 4;
        const stemX = up ? NOTE_X + 6.2 : NOTE_X - 6.2;
        note += `<line class="staff__stem" x1="${stemX}" x2="${stemX}" y1="${y}" y2="${up ? y - 30 : y + 30}"/>`;
        note += `<ellipse class="staff__head" cx="${NOTE_X}" cy="${y}" rx="6.8" ry="4.9" transform="rotate(-20 ${NOTE_X} ${y})"/>`;
        if (isBlack(midi)) note += `<text class="staff__sharp" x="${NOTE_X - 24}" y="${y + 6}">♯</text>`;
    }

    return `<svg class="staff" viewBox="0 0 130 100" role="img" aria-label="${label}">
        <g class="staff__lines">${lines}</g>
        <text class="staff__clef" x="8" y="${BOTTOM_LINE + 4}">𝄞</text>
        <g class="staff__note">${note}</g>
    </svg>`;
}

const VALUE_NAMES = { 0.5: 'Eighth note', 1: 'Quarter note', 1.5: 'Dotted quarter', 2: 'Half note', 4: 'Whole note' };
const BEAT_TEXT = { 0.5: 'half a beat', 1: '1 beat', 1.5: '1½ beats', 2: '2 beats', 4: '4 beats' };

/** A small drawing of a note value plus how many beats it lasts. */
export function noteValue(beats) {
    const filled = beats < 2;
    const stem = beats < 4 ? '<line x1="17.5" x2="17.5" y1="26" y2="6"/>' : '';
    const flag = beats === 0.5 ? '<path d="M17.5 6c1 5 6 6 5 12" fill="none"/>' : '';
    const dot = beats === 1.5 ? '<circle cx="24" cy="26" r="1.6" fill="currentColor" stroke="none"/>' : '';
    return `<span class="value">
        <svg viewBox="0 0 30 34" aria-hidden="true"><ellipse cx="12" cy="26" rx="6" ry="4.3" transform="rotate(-20 12 26)" fill="${filled ? 'currentColor' : 'none'}"/>${stem}${flag}${dot}</svg>
        <span><strong>${VALUE_NAMES[beats]}</strong><small>${BEAT_TEXT[beats]}</small></span>
    </span>`;
}
