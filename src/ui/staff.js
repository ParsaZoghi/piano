import { t } from '../core/i18n.js';
import { isBlack, noteName, staffStep } from '../core/notes.js';

const BOTTOM_LINE = 66; // y of E4
const HALF_SPACE = 5;
const NOTE_X = 82;
const STEM = 30;

/** "0_5" style key for note values (dots are path separators in i18n keys). */
export const valueKey = (beats) => String(beats).replace('.', '_');

/**
 * Treble staff with one note on it (or empty when midi is null). `beats`
 * picks the written value: 4 whole, 3 dotted half, 2 half, 1.5 dotted
 * quarter, 1 quarter, 0.5 eighth.
 */
export function staffSvg(midi, beats = 1) {
    let lines = '';
    for (let i = 0; i < 5; i++) {
        const y = BOTTOM_LINE - i * HALF_SPACE * 2;
        lines += `<line x1="6" x2="124" y1="${y}" y2="${y}"/>`;
    }

    let note = '';
    let label = t('staff.empty');
    if (midi != null) {
        const step = staffStep(midi);
        const y = BOTTOM_LINE - step * HALF_SPACE;
        label = `${t('staff.note', { note: noteName(midi) })}. ${t(`value.${valueKey(beats)}`)}`;
        // Ledger lines for notes below or above the five lines.
        const ledger = (s) => `<line x1="${NOTE_X - 12}" x2="${NOTE_X + 12}" y1="${BOTTOM_LINE - s * HALF_SPACE}" y2="${BOTTOM_LINE - s * HALF_SPACE}"/>`;
        for (let s = -2; s >= step; s -= 2) note += ledger(s);
        for (let s = 10; s <= step; s += 2) note += ledger(s);

        const hollow = beats >= 2;
        if (beats < 4) {
            const up = step < 4;
            const stemX = up ? NOTE_X + 6.2 : NOTE_X - 6.2;
            const end = up ? y - STEM : y + STEM;
            note += `<line class="staff__stem" x1="${stemX}" x2="${stemX}" y1="${y}" y2="${end}"/>`;
            if (beats === 0.5) {
                const d = up ? `M${stemX} ${end} c1 7 9 8 7.5 18` : `M${stemX} ${end} c1 -7 9 -8 7.5 -18`;
                note += `<path class="staff__flag" d="${d}"/>`;
            }
        }
        note += `<ellipse class="staff__head${hollow ? ' staff__head--hollow' : ''}" cx="${NOTE_X}" cy="${y}" rx="6.8" ry="4.9" transform="rotate(-20 ${NOTE_X} ${y})"/>`;
        if (beats === 1.5 || beats === 3) {
            // The dot sits in a space, never on a line.
            const dotY = step % 2 === 0 ? y - HALF_SPACE : y;
            note += `<circle class="staff__dot" cx="${NOTE_X + 12}" cy="${dotY}" r="1.9"/>`;
        }
        if (isBlack(midi)) note += `<text class="staff__sharp" x="${NOTE_X - 24}" y="${y + 6}">♯</text>`;
    }

    return `<svg class="staff" viewBox="0 0 130 100" role="img" aria-label="${label}">
        <g class="staff__lines">${lines}</g>
        <text class="staff__clef" x="8" y="${BOTTOM_LINE + 4}">𝄞</text>
        <g class="staff__note">${note}</g>
    </svg>`;
}
