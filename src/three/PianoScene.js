import * as THREE from 'three';
import { input } from '../core/input.js';
import { PIANO_MAX, PIANO_MIN } from '../core/notes.js';
import { Keyboard3D } from './Keyboard3D.js';
import { HIGHWAY_LENGTH, NoteHighway } from './NoteHighway.js';

const BACKGROUND = 0x0b0b1c;
const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

// Camera presets. `depth` is the stretch of floor (far z, near z) that must
// stay on screen; `top` / `bottom` are the fractions of the screen kept clear
// for the HUD (`portrait` overrides them on tall screens, where the HUD wraps);
// `zoom` < 1 lets the keyboard run off the sides (menu backdrop).
const VIEWS = {
    menu: { pitch: 20, depth: [0, 6.6], top: 0.6, bottom: 0.08, zoom: 0.42 },
    free: { pitch: 52, depth: [-0.4, 6.6], top: 0.16, bottom: 0.14 },
    find: { pitch: 46, depth: [-0.4, 6.6], top: 0.44, bottom: 0.16, minDistance: 16, portrait: { top: 0.42, bottom: 0.2 } },
    lesson: { pitch: 36, depth: [-HIGHWAY_LENGTH * 0.6, 6.6], top: 0.12, bottom: 0.15, minDistance: 20, portrait: { top: 0.34, bottom: 0.25 } },
};
const corner = new THREE.Vector3();

export class PianoScene {
    constructor(canvas) {
        this.canvas = canvas;
        this.frameListeners = new Set();
        this.view = 'menu';
        this.time = 0;
        this.pointers = new Map(); // pointerId -> midi

        this.renderer = new THREE.WebGLRenderer({ canvas, antialias: true, powerPreference: 'high-performance' });
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFShadowMap;
        this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
        this.renderer.toneMappingExposure = 1.05;

        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(BACKGROUND);
        this.scene.fog = new THREE.Fog(BACKGROUND, 40, 120);

        this.camera = new THREE.PerspectiveCamera(34, 1, 0.5, 400);
        this.cameraTarget = new THREE.Vector3();
        this.cameraHome = new THREE.Vector3();

        this.scene.add(new THREE.HemisphereLight(0xc7d2fe, 0x12122a, 1.1));
        this.sun = new THREE.DirectionalLight(0xffffff, 2.4);
        this.sun.position.set(-8, 22, 14);
        this.sun.castShadow = true;
        this.sun.shadow.mapSize.set(2048, 2048);
        this.sun.shadow.bias = -0.0004;
        this.scene.add(this.sun, this.sun.target);
        const rim = new THREE.PointLight(0x6366f1, 120, 60);
        rim.position.set(0, 6, -10);
        this.scene.add(rim);

        const floor = new THREE.Mesh(new THREE.PlaneGeometry(600, 600).rotateX(-Math.PI / 2), new THREE.MeshStandardMaterial({ color: 0x0d0d20, roughness: 0.9 }));
        floor.position.y = -0.52;
        floor.receiveShadow = true;
        this.scene.add(floor);

        this.keyboard = new Keyboard3D();
        this.highway = new NoteHighway(this.keyboard);
        this.scene.add(this.keyboard.group, this.highway.group);

        this.raycaster = new THREE.Raycaster();
        this.bindPointer();

        this.resizeObserver = new ResizeObserver(() => this.resize());
        this.resizeObserver.observe(canvas);
        this.resize();

        this.lastFrame = 0;
        this.loop = this.loop.bind(this);
        this.renderer.setAnimationLoop(this.loop);
        // Pause rendering in a hidden tab; the dt clamp absorbs the gap on return.
        document.addEventListener('visibilitychange', () => {
            this.renderer.setAnimationLoop(document.hidden ? null : this.loop);
        });
    }

    /** Show the keys lo..hi with the chosen camera preset. */
    configure({ lo = PIANO_MIN, hi = PIANO_MAX, view = 'free', highway = false }) {
        input.releaseAll();
        this.keyboard.build(lo, hi);
        this.highway.group.visible = highway;
        if (highway) this.highway.layout();
        this.view = view;
        this.fit();
    }

    onFrame(fn) {
        this.frameListeners.add(fn);
        return () => this.frameListeners.delete(fn);
    }

    resize() {
        const { clientWidth: w, clientHeight: h } = this.canvas;
        if (!w || !h) return;
        this.renderer.setSize(w, h, false);
        this.camera.aspect = w / h;
        this.camera.updateProjectionMatrix();
        this.fit();
    }

    fit() {
        const preset = VIEWS[this.view];
        const { pitch, depth, zoom = 1, minDistance = 10 } = preset;
        const { top, bottom } = this.camera.aspect < 0.8 && preset.portrait ? preset.portrait : preset;
        const [zFar, zNear] = depth;
        const angle = THREE.MathUtils.degToRad(pitch);
        const halfWidth = (this.keyboard.width / 2 + 0.9) * zoom;
        const corners = [
            [-halfWidth, 0, zNear],
            [halfWidth, 0, zNear],
            [-halfWidth, 1.2, zFar],
            [halfWidth, 1.2, zFar],
        ];
        this.cameraTarget.set(0, 0.5, (zFar + zNear) / 2);

        // Slide the picture so its centre lands in the middle of the clear band
        // between the top and bottom HUD, instead of the middle of the canvas.
        const { clientWidth: w, clientHeight: h } = this.canvas;
        if (w && h) this.camera.setViewOffset(w, h, 0, Math.round(((bottom - top) * h) / 2), w, h);

        // Back the camera away until every corner sits inside the clear area.
        let distance = minDistance;
        for (let i = 0; i < 160; i++) {
            this.camera.position.set(0, this.cameraTarget.y + distance * Math.sin(angle), this.cameraTarget.z + distance * Math.cos(angle));
            this.camera.lookAt(this.cameraTarget);
            this.camera.updateMatrixWorld(true);
            const fits = corners.every(([x, y, z]) => {
                corner.set(x, y, z).project(this.camera);
                return Math.abs(corner.x) <= 0.96 && corner.y >= -1 + bottom * 2 && corner.y <= 1 - top * 2;
            });
            if (fits) break;
            distance *= 1.04;
        }
        this.cameraHome.copy(this.camera.position);

        const far = distance + HIGHWAY_LENGTH + 30;
        this.scene.fog.near = far * 0.75;
        this.scene.fog.far = far * 1.6;

        const cam = this.sun.shadow.camera;
        const reach = this.keyboard.width / 2 + 5;
        cam.left = -reach;
        cam.right = reach;
        cam.top = 26;
        cam.bottom = -12;
        cam.far = 80;
        cam.updateProjectionMatrix();
    }

    loop(now) {
        const dt = Math.min((now - (this.lastFrame || now)) / 1000, 0.1);
        this.lastFrame = now;
        this.time += dt;
        if (this.view === 'menu' && !reducedMotion.matches) {
            this.camera.position.x = this.cameraHome.x + Math.sin(this.time * 0.16) * this.keyboard.width * 0.12;
            this.camera.lookAt(this.cameraTarget);
        }
        for (const fn of this.frameListeners) fn(dt, this.time);
        this.keyboard.update(dt, this.time);
        this.renderer.render(this.scene, this.camera);
    }

    // --- mouse / touch on the 3D keys --------------------------------------

    keyAt(event) {
        const rect = this.canvas.getBoundingClientRect();
        const point = new THREE.Vector2(((event.clientX - rect.left) / rect.width) * 2 - 1, -((event.clientY - rect.top) / rect.height) * 2 + 1);
        this.raycaster.setFromCamera(point, this.camera);
        const hit = this.raycaster.intersectObjects(this.keyboard.meshes, false)[0];
        return hit ? hit.object.userData.midi : null;
    }

    bindPointer() {
        const release = (e) => {
            const midi = this.pointers.get(e.pointerId);
            if (midi != null) input.noteOff(midi, `pointer${e.pointerId}`);
            this.pointers.delete(e.pointerId);
        };
        this.canvas.addEventListener('pointerdown', (e) => {
            if (this.view === 'menu' || e.button > 0) return;
            const midi = this.keyAt(e);
            if (midi == null) return;
            e.preventDefault();
            this.canvas.setPointerCapture(e.pointerId);
            this.pointers.set(e.pointerId, midi);
            input.noteOn(midi, 0.8, `pointer${e.pointerId}`);
        });
        // Dragging across keys plays a glissando.
        this.canvas.addEventListener('pointermove', (e) => {
            if (!this.pointers.has(e.pointerId)) return;
            const midi = this.keyAt(e);
            const current = this.pointers.get(e.pointerId);
            if (midi == null || midi === current) return;
            input.noteOff(current, `pointer${e.pointerId}`);
            this.pointers.set(e.pointerId, midi);
            input.noteOn(midi, 0.8, `pointer${e.pointerId}`);
        });
        this.canvas.addEventListener('pointerup', release);
        this.canvas.addEventListener('pointercancel', release);
    }
}
