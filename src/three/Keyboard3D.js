import * as THREE from 'three';
import { RoundedBoxGeometry } from 'three/addons/geometries/RoundedBoxGeometry.js';
import { isBlack, octaveOf, pitchName } from '../core/notes.js';
import { makeLabel } from './labels.js';

// Units: one white key is 1 wide. Keys run along +Z toward the player, with
// their back edge at z = 0 so they can hinge there when pressed.
export const WHITE_LENGTH = 6;
export const WHITE_TOP = 0.9;
const BLACK_LENGTH = 3.8;
const BLACK_TOP = 1.2;
// Black keys are not centred between whites on a real piano.
const BLACK_NUDGE = { 1: -0.08, 3: 0.08, 6: -0.1, 8: 0, 10: 0.1 };

const whiteGeometry = new RoundedBoxGeometry(0.95, WHITE_TOP, WHITE_LENGTH, 3, 0.07).translate(0, WHITE_TOP / 2, WHITE_LENGTH / 2);
const blackGeometry = new RoundedBoxGeometry(0.56, 0.64, BLACK_LENGTH, 3, 0.06).translate(0, BLACK_TOP - 0.32, BLACK_LENGTH / 2);

const COLORS = {
    pressed: new THREE.Color(0x22c55e),
    hint: new THREE.Color(0x22d3ee),
    good: new THREE.Color(0x4ade80),
    bad: new THREE.Color(0xef4444),
    none: new THREE.Color(0x000000),
};

export class Keyboard3D {
    constructor() {
        this.group = new THREE.Group();
        this.keys = new Map(); // midi -> key record
        this.meshes = []; // for raycasting
        this.width = 0;
        this.hintMidi = null;
        this.caseMaterial = new THREE.MeshStandardMaterial({ color: 0x15152a, roughness: 0.45, metalness: 0.3 });
        this.feltMaterial = new THREE.MeshStandardMaterial({ color: 0x7f1d1d, roughness: 1 });
    }

    /** Build keys for the inclusive MIDI range, widened to end on white keys. */
    build(lo, hi) {
        this.clear();
        while (isBlack(lo)) lo--;
        while (isBlack(hi)) hi++;
        this.lo = lo;
        this.hi = hi;

        let whites = 0;
        for (let m = lo; m <= hi; m++) if (!isBlack(m)) whites++;
        this.width = whites;
        const origin = -(whites - 1) / 2;

        let whiteIndex = 0;
        for (let midi = lo; midi <= hi; midi++) {
            const black = isBlack(midi);
            const x = black ? origin + whiteIndex - 0.5 + BLACK_NUDGE[midi % 12] : origin + whiteIndex++;
            const material = new THREE.MeshStandardMaterial(
                black ? { color: 0x16161f, roughness: 0.35, metalness: 0.1 } : { color: 0xf5f2ea, roughness: 0.3, metalness: 0 },
            );
            const mesh = new THREE.Mesh(black ? blackGeometry : whiteGeometry, material);
            mesh.castShadow = true;
            mesh.receiveShadow = true;
            mesh.userData.midi = midi;

            const pivot = new THREE.Group();
            pivot.position.x = x;
            pivot.add(mesh);
            this.group.add(pivot);
            this.meshes.push(mesh);
            this.keys.set(midi, { midi, black, x, pivot, material, base: material.color.clone(), tint: new THREE.Color(), pressed: false, angle: 0, flash: null, flashLeft: 0, glow: 0, labels: [] });
        }
        this.buildCase();
    }

    buildCase() {
        const w = this.width;
        const add = (geometry, material, x, y, z) => {
            const mesh = new THREE.Mesh(geometry, material);
            mesh.position.set(x, y, z);
            mesh.receiveShadow = true;
            mesh.castShadow = true;
            this.group.add(mesh);
            this.caseParts.push(mesh);
        };
        this.caseParts = [];
        add(new THREE.BoxGeometry(w + 1.3, 0.5, WHITE_LENGTH + 0.9), this.caseMaterial, 0, -0.27, WHITE_LENGTH / 2 - 0.15);
        add(new THREE.BoxGeometry(w + 0.1, 0.1, 0.22), this.feltMaterial, 0, WHITE_TOP + 0.03, -0.1);
        for (const side of [-1, 1]) {
            add(new THREE.BoxGeometry(0.55, 1.25, WHITE_LENGTH + 0.9), this.caseMaterial, side * (w / 2 + 0.33), 0.36, WHITE_LENGTH / 2 - 0.15);
        }
    }

    clear() {
        for (const key of this.keys.values()) key.material.dispose();
        for (const part of this.caseParts || []) part.geometry.dispose();
        this.group.clear();
        this.keys.clear();
        this.meshes = [];
        this.hintMidi = null;
    }

    has(midi) {
        return this.keys.has(midi);
    }

    xOf(midi) {
        return this.keys.get(midi)?.x ?? 0;
    }

    /**
     * names: show note letters on white keys.
     * typingMap: Map(midi -> char) of the typing keys to show, or null.
     */
    setLabels({ names, typingMap }) {
        for (const key of this.keys.values()) {
            for (const label of key.labels) key.pivot.remove(label);
            key.labels = [];
            const add = (text, style, size, y, z) => {
                const label = makeLabel(text, style, size);
                label.position.set(0, y, z);
                key.pivot.add(label);
                key.labels.push(label);
            };
            if (names && !key.black) {
                const isC = key.midi % 12 === 0;
                add(isC ? pitchName(key.midi) + octaveOf(key.midi) : pitchName(key.midi), 'name', isC ? 0.78 : 0.7, WHITE_TOP + 0.006, 5.35);
            }
            const char = typingMap?.get(key.midi);
            if (char) {
                if (key.black) add(char, 'badgeDark', 0.52, BLACK_TOP + 0.006, 3.3);
                else add(char, 'badge', 0.7, WHITE_TOP + 0.006, 4.5);
            }
        }
    }

    press(midi, on) {
        const key = this.keys.get(midi);
        if (key) key.pressed = on;
    }

    releaseAll() {
        for (const key of this.keys.values()) key.pressed = false;
    }

    /** Glow one key as "play this next" (null clears it). */
    setHint(midi) {
        this.hintMidi = midi;
    }

    flash(midi, kind) {
        const key = this.keys.get(midi);
        if (!key) return;
        key.flash = COLORS[kind];
        key.flashLeft = 0.35;
    }

    update(dt, time) {
        const ease = Math.min(1, dt * 28);
        const pulse = 0.55 + 0.25 * Math.sin(time * 6);
        for (const key of this.keys.values()) {
            const targetAngle = key.pressed ? (key.black ? 0.075 : 0.052) : 0;
            key.angle += (targetAngle - key.angle) * ease;
            key.pivot.rotation.x = key.angle;

            let color = COLORS.none;
            let strength = 0;
            if (key.flashLeft > 0) {
                key.flashLeft -= dt;
                color = key.flash;
                strength = 0.9;
            } else if (key.pressed) {
                color = COLORS.pressed;
                strength = 0.6;
            } else if (key.midi === this.hintMidi) {
                color = COLORS.hint;
                strength = pulse;
            }
            if (strength > 0) key.tint.lerp(color, Math.min(1, dt * 40));
            key.glow += (strength - key.glow) * Math.min(1, dt * 18);
            // White keys are too bright for a glow to show, so tint the surface too.
            key.material.color.copy(key.base).lerp(key.tint, key.glow * (key.black ? 0.5 : 0.8));
            key.material.emissive.copy(key.tint);
            key.material.emissiveIntensity = key.glow * (key.black ? 1 : 0.35);
        }
    }
}
