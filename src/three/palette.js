// Scene colours per theme. The frame is a warm walnut in the dark theme so it
// stands apart from the night-blue background, and charcoal in the light one.
export const PALETTES = {
    dark: {
        background: 0x0b0b1c,
        floor: 0x0d0d20,
        frame: 0x8a5a36,
        felt: 0x7f1d1d,
        road: 0x15152e,
        lane: 0x0b0b1c,
        edge: 0x3b3a8f,
        beat: 0x4338ca,
        bar: 0x818cf8,
        outline: 0x05060f,
        sky: 0xc7d2fe,
        ground: 0x12122a,
        hemisphere: 1.1,
        rim: 120,
    },
    light: {
        background: 0xe6eaf5,
        floor: 0xd8deed,
        frame: 0x2b2f45,
        felt: 0x9f1239,
        road: 0xc7cfe6,
        lane: 0xa9b3d3,
        edge: 0x6f7bb0,
        beat: 0x6366f1,
        bar: 0x312e81,
        outline: 0x0f172a,
        sky: 0xffffff,
        ground: 0xb8c0dc,
        hemisphere: 1.6,
        rim: 40,
    },
};

export let palette = PALETTES.dark;

export function setPalette(theme) {
    palette = PALETTES[theme] || PALETTES.dark;
}
