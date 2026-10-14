import * as THREE from 'three';

// Small text decals (note names, typing keys) drawn once to a canvas and cached.
const cache = new Map();
const STYLES = {
    name: { color: '#475569' }, // on white keys
    badge: { color: '#ffffff', fill: '#4338ca' }, // typing key on white keys
    badgeDark: { color: '#0f172a', fill: '#c7d2fe' }, // typing key on black keys
    note: { color: '#0f172a' }, // on falling notes
};
// Longer names (solfège "Sol♯") get a smaller font so they still fit.
const FONT_SIZE = [70, 70, 54, 44, 36, 30];

export function labelTexture(text, style) {
    const key = `${style}|${text}`;
    if (cache.has(key)) return cache.get(key);

    const size = 128;
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = size;
    const g = canvas.getContext('2d');
    const { color, fill } = STYLES[style];
    if (fill) {
        g.fillStyle = fill;
        g.beginPath();
        g.roundRect(16, 16, size - 32, size - 32, 26);
        g.fill();
    }
    g.fillStyle = color;
    g.font = `700 ${FONT_SIZE[Math.min(text.length, 5)]}px Poppins, "Segoe UI", system-ui, sans-serif`;
    g.textAlign = 'center';
    g.textBaseline = 'middle';
    g.fillText(text, size / 2, size / 2 + 4, size - 12);

    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = 4;
    cache.set(key, texture);
    return texture;
}

const plane = new THREE.PlaneGeometry(1, 1).rotateX(-Math.PI / 2);
const materials = new Map();

/** A flat, upward-facing label mesh. Geometry and materials are shared. */
export function makeLabel(text, style, size) {
    const key = `${style}|${text}`;
    let material = materials.get(key);
    if (!material) {
        material = new THREE.MeshBasicMaterial({
            map: labelTexture(text, style),
            transparent: true,
            depthWrite: false,
            toneMapped: false,
        });
        materials.set(key, material);
    }
    const mesh = new THREE.Mesh(plane, material);
    mesh.scale.set(size, 1, size);
    mesh.renderOrder = 2;
    return mesh;
}
