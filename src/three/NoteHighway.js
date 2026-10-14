import * as THREE from 'three';
import { isBlack } from '../core/notes.js';
import { WHITE_TOP } from './Keyboard3D.js';
import { makeLabel } from './labels.js';

// The runway behind the keys. Notes travel along it toward the hit line at the
// back edge of the keyboard; a note's length is how long it should be held.
export const HIGHWAY_LENGTH = 22;
const HIT_Z = -0.3;
const Y = WHITE_TOP + 0.02;
const NOTE_HEIGHT = 0.24;
const BEAT_LINES = 10;

const box = new THREE.BoxGeometry(1, 1, 1);
const STATE_COLORS = {
    white: 0x38bdf8,
    black: 0xa78bfa,
    hit: 0x4ade80,
    miss: 0x64748b,
};

export class NoteHighway {
    constructor(keyboard) {
        this.keyboard = keyboard;
        this.group = new THREE.Group();
        this.group.visible = false;
        this.staticParts = new THREE.Group();
        this.noteParts = new THREE.Group();
        this.group.add(this.staticParts, this.noteParts);
        this.items = [];
        this.beatLines = [];
        this.unitsPerSecond = 6;
        this.secondsPerBeat = 0.75;
        this.beatsPerBar = 4;
    }

    /** Rebuild the lanes to match the current keyboard. */
    layout() {
        this.disposeGroup(this.staticParts);
        const w = this.keyboard.width;
        const add = (geometry, material, x, y, z) => {
            const mesh = new THREE.Mesh(geometry, material);
            mesh.position.set(x, y, z);
            this.staticParts.add(mesh);
            return mesh;
        };
        const zMid = HIT_Z - HIGHWAY_LENGTH / 2;

        const floor = add(
            new THREE.BoxGeometry(w + 0.5, 0.1, HIGHWAY_LENGTH),
            new THREE.MeshStandardMaterial({ color: 0x15152e, roughness: 0.85 }),
            0,
            Y - 0.07,
            zMid,
        );
        floor.receiveShadow = true;

        const laneMaterial = new THREE.MeshBasicMaterial({ color: 0x0b0b1c });
        const edgeMaterial = new THREE.MeshBasicMaterial({ color: 0x312e81 });
        for (const key of this.keyboard.keys.values()) {
            if (key.black) add(new THREE.PlaneGeometry(0.56, HIGHWAY_LENGTH).rotateX(-Math.PI / 2), laneMaterial, key.x, Y - 0.015, zMid);
            else add(new THREE.PlaneGeometry(0.03, HIGHWAY_LENGTH).rotateX(-Math.PI / 2), edgeMaterial, key.x - 0.5, Y - 0.012, zMid);
        }
        add(new THREE.PlaneGeometry(0.03, HIGHWAY_LENGTH).rotateX(-Math.PI / 2), edgeMaterial, w / 2, Y - 0.012, zMid);

        add(new THREE.BoxGeometry(w + 0.5, 0.07, 0.14), new THREE.MeshBasicMaterial({ color: 0x22c55e, toneMapped: false }), 0, Y + 0.02, HIT_Z);

        this.beatLines = [];
        const beatGeometry = new THREE.PlaneGeometry(w, 0.05).rotateX(-Math.PI / 2);
        this.beatMaterial = new THREE.MeshBasicMaterial({ color: 0x4338ca, transparent: true, opacity: 0.55 });
        this.barMaterial = new THREE.MeshBasicMaterial({ color: 0x818cf8, transparent: true, opacity: 0.9 });
        for (let i = 0; i < BEAT_LINES; i++) this.beatLines.push(add(beatGeometry, this.beatMaterial, 0, Y - 0.008, 0));
    }

    /**
     * notes: [{ midi, time, dur, label }] in seconds of song time. The same
     * objects are read every frame, so the engine can just set note.state.
     */
    setNotes(notes, { secondsPerBeat, beatsPerBar = 4, unitsPerBeat = 3.2 }) {
        this.disposeGroup(this.noteParts);
        this.secondsPerBeat = secondsPerBeat;
        this.beatsPerBar = beatsPerBar;
        this.unitsPerSecond = unitsPerBeat / secondsPerBeat;
        this.items = notes.map((note) => {
            const black = isBlack(note.midi);
            const material = new THREE.MeshStandardMaterial({ roughness: 0.4, transparent: true });
            const mesh = new THREE.Mesh(box, material);
            mesh.position.x = this.keyboard.xOf(note.midi);
            mesh.castShadow = true;
            const label = makeLabel(note.label, 'note', black ? 0.5 : 0.72);
            label.position.x = mesh.position.x;
            this.noteParts.add(mesh, label);
            return { note, mesh, material, label, width: black ? 0.5 : 0.84, base: black ? 'black' : 'white', shown: null };
        });
    }

    update(time) {
        const v = this.unitsPerSecond;
        for (const item of this.items) {
            const { note, mesh, label } = item;
            const front = HIT_Z + (time - note.time) * v; // moves toward +z
            const back = front - Math.max(0.35, note.dur * v - 0.12);
            const clampedFront = Math.min(front, HIT_Z);
            const clampedBack = Math.max(back, HIT_Z - HIGHWAY_LENGTH);
            const visible = clampedFront - clampedBack > 0.02 && note.state !== 'gone';
            mesh.visible = visible;
            label.visible = visible && front <= HIT_Z + 0.01;
            if (!visible) continue;

            mesh.scale.set(item.width, NOTE_HEIGHT, clampedFront - clampedBack);
            mesh.position.set(mesh.position.x, Y + NOTE_HEIGHT / 2, (clampedFront + clampedBack) / 2);
            label.position.set(mesh.position.x, Y + NOTE_HEIGHT + 0.012, clampedFront - 0.42);

            const state = note.state === 'hit' || note.state === 'miss' ? note.state : item.base;
            if (state !== item.shown) {
                item.shown = state;
                item.material.color.set(STATE_COLORS[state]);
                item.material.emissive.set(STATE_COLORS[state]);
                item.material.emissiveIntensity = state === 'hit' ? 0.9 : state === 'miss' ? 0.05 : 0.4;
                item.material.opacity = state === 'miss' ? 0.45 : 1;
            }
        }

        const spb = this.secondsPerBeat;
        const firstBeat = Math.ceil(time / spb);
        this.beatLines.forEach((line, i) => {
            const beat = firstBeat + i;
            const z = HIT_Z - (beat * spb - time) * v;
            line.visible = z > HIT_Z - HIGHWAY_LENGTH;
            line.position.z = z;
            const isBar = ((beat % this.beatsPerBar) + this.beatsPerBar) % this.beatsPerBar === 0;
            line.material = isBar ? this.barMaterial : this.beatMaterial;
        });
    }

    disposeGroup(group) {
        if (group === this.noteParts) {
            // Notes share the box and label geometry; only their materials are unique.
            for (const item of this.items) item.material.dispose();
            this.items = [];
        } else {
            for (const child of group.children) {
                child.geometry.dispose();
                child.material.dispose();
            }
        }
        group.clear();
    }
}
