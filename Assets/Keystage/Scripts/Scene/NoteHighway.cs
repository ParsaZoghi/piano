using System.Collections.Generic;
using Keystage.Core;
using UnityEngine;

namespace Keystage.Scene
{
    /// <summary>A note travelling down the runway. The engine sets State; the runway reads it.</summary>
    public class RunwayNote
    {
        public int midi;
        public float beats;
        public float time; // seconds of song time
        public float dur;
        public string label;
        public string state = "pending"; // pending | hit | miss
        public float hitAt;
    }

    /// <summary>
    /// The runway behind the keys (port of three/NoteHighway.js). Notes travel
    /// toward the hit line at the back edge of the keyboard (z = 0.3 here, the
    /// far side is +Z); a note's length is how long it should be held.
    /// </summary>
    public class NoteHighway
    {
        public const float Length = 22f;
        const float HitZ = 0.3f;
        const float Y = Keyboard3D.WhiteTop + 0.02f;
        const float NoteHeight = 0.24f;
        const float Gap = 0.16f; // space left between back-to-back notes
        const float Rim = 0.07f; // dark outline around every block
        const int BeatLines = 12;

        static readonly Dictionary<string, Color> StateColors = new Dictionary<string, Color>
        {
            ["white"] = Palette.Hex(0x38bdf8),
            ["black"] = Palette.Hex(0xa78bfa),
            ["hit"] = Palette.Hex(0x4ade80),
            ["miss"] = Palette.Hex(0x64748b),
        };
        static readonly Color NoteText = Palette.Hex(0x0f172a);

        class Item
        {
            public RunwayNote note;
            public Transform mesh, outline, label;
            public Material material;
            public float width;
            public string baseState, shown;
        }

        public readonly Transform Root;
        readonly Keyboard3D keyboard;
        readonly Transform staticParts, noteParts;
        readonly List<Item> items = new List<Item>();
        readonly List<Transform> beatLines = new List<Transform>();
        readonly Material outlineMaterial;
        Material beatMaterial, barMaterial;
        readonly List<Material> staticMaterials = new List<Material>();
        float unitsPerSecond = 6f, secondsPerBeat = 0.75f;
        int beatsPerBar = 4;

        public bool Visible
        {
            get => Root.gameObject.activeSelf;
            set => Root.gameObject.SetActive(value);
        }

        public NoteHighway(Keyboard3D keyboard, Transform parent)
        {
            this.keyboard = keyboard;
            Root = new GameObject("Runway").transform;
            Root.SetParent(parent, false);
            staticParts = new GameObject("Lanes").transform;
            staticParts.SetParent(Root, false);
            noteParts = new GameObject("Notes").transform;
            noteParts.SetParent(Root, false);
            outlineMaterial = SceneKit.Lit(Palette.Current.outline, 0.6f);
            Visible = false;
        }

        public void ApplyPalette()
        {
            SceneKit.SetColor(outlineMaterial, Palette.Current.outline);
            if (Visible) Layout();
        }

        /// <summary>Rebuild the lanes to match the current keyboard.</summary>
        public void Layout()
        {
            ClearChildren(staticParts);
            foreach (var m in staticMaterials) Object.Destroy(m);
            staticMaterials.Clear();
            beatLines.Clear();

            var p = Palette.Current;
            float w = keyboard.Width, unit = keyboard.Unit;
            float zMid = HitZ + Length / 2;

            Material Track(Material m)
            {
                staticMaterials.Add(m);
                return m;
            }

            var floor = SceneKit.MeshObject("Road", SceneKit.Cube, Track(SceneKit.Lit(p.road, 0.85f)), staticParts);
            floor.transform.localPosition = new Vector3(0, Y - 0.07f, zMid);
            floor.transform.localScale = new Vector3(w + 0.5f, 0.1f, Length);
            floor.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var laneMat = Track(SceneKit.Unlit(p.lane));
            var edgeMat = Track(SceneKit.Unlit(p.edge));
            void Flat(Material m, float x, float width, float y)
            {
                var go = SceneKit.MeshObject("Lane", SceneKit.FlatQuad, m, staticParts, false);
                go.transform.localPosition = new Vector3(x, y, zMid);
                go.transform.localScale = new Vector3(width, 1, Length);
            }
            foreach (var key in keyboard.Keys.Values)
            {
                if (key.black) Flat(laneMat, key.x, 0.56f * unit, Y - 0.015f);
                else Flat(edgeMat, key.x - 0.5f * unit, 0.03f, Y - 0.012f);
            }
            Flat(edgeMat, w / 2, 0.03f, Y - 0.012f);

            var hit = SceneKit.MeshObject("HitLine", SceneKit.Cube, Track(SceneKit.Unlit(Palette.Hex(0x22c55e))), staticParts, false);
            hit.transform.localPosition = new Vector3(0, Y + 0.02f, HitZ);
            hit.transform.localScale = new Vector3(w + 0.5f, 0.07f, 0.14f);

            // Beat lines are drawn as solid colours mixed toward the road (no transparency needed).
            beatMaterial = Track(SceneKit.Unlit(Color.Lerp(p.road, p.beat, 0.55f)));
            barMaterial = Track(SceneKit.Unlit(Color.Lerp(p.road, p.bar, 0.9f)));
            for (int i = 0; i < BeatLines; i++)
            {
                var line = SceneKit.MeshObject("Beat", SceneKit.FlatQuad, beatMaterial, staticParts, false);
                line.transform.localScale = new Vector3(w, 1, 0.05f);
                beatLines.Add(line.transform);
            }
        }

        public void SetNotes(List<RunwayNote> notes, float secondsPerBeat, int beatsPerBar = 4, float unitsPerBeat = 3.2f)
        {
            ClearNotes();
            this.secondsPerBeat = secondsPerBeat;
            this.beatsPerBar = beatsPerBar;
            unitsPerSecond = unitsPerBeat / secondsPerBeat;
            float unit = keyboard.Unit;
            foreach (var note in notes)
            {
                bool black = Notes.IsBlack(note.midi);
                var material = SceneKit.Lit(StateColors[black ? "black" : "white"], 0.4f);
                float x = keyboard.XOf(note.midi);
                var outline = SceneKit.MeshObject("Outline", SceneKit.Cube, outlineMaterial, noteParts, false);
                var mesh = SceneKit.MeshObject("Note", SceneKit.Cube, material, noteParts);
                mesh.GetComponent<MeshRenderer>().receiveShadows = false;
                var label = SceneKit.Label(note.label, NoteText, SceneKit.CapHeight(note.label, (black ? 0.75f : 1.15f) * unit), noteParts);
                outline.transform.localPosition = mesh.transform.localPosition = new Vector3(x, 0, 0);
                items.Add(new Item
                {
                    note = note,
                    mesh = mesh.transform,
                    outline = outline.transform,
                    label = label.transform,
                    material = material,
                    width = (black ? 0.5f : 0.84f) * unit,
                    baseState = black ? "black" : "white",
                });
            }
        }

        public void ClearNotes()
        {
            foreach (var item in items) Object.Destroy(item.material);
            items.Clear();
            ClearChildren(noteParts);
        }

        public void Update(float time)
        {
            if (!Visible) return;
            float v = unitsPerSecond;
            foreach (var item in items)
            {
                var note = item.note;
                float front = HitZ - (time - note.time) * v; // moves toward the keys (-Z)
                float back = front + Mathf.Max(0.35f, note.dur * v - Gap);
                float clampedFront = Mathf.Max(front, HitZ);
                float clampedBack = Mathf.Min(back, HitZ + Length);
                float length = clampedBack - clampedFront;
                bool visible = length > 0.02f;
                SetActive(item.mesh, visible);
                SetActive(item.outline, visible);
                SetActive(item.label, visible && front >= HitZ - 0.01f);
                if (!visible) continue;

                float z = (clampedFront + clampedBack) / 2;
                float x = item.mesh.localPosition.x;
                // The coloured block sits inside a slightly larger, lower dark block,
                // so back-to-back notes always show a dark border between them.
                item.outline.localScale = new Vector3(item.width + Rim, NoteHeight - 0.03f, length);
                item.outline.localPosition = new Vector3(x, Y + (NoteHeight - 0.03f) / 2, z);
                item.mesh.localScale = new Vector3(item.width - Rim, NoteHeight, Mathf.Max(0.01f, length - Rim * 2));
                item.mesh.localPosition = new Vector3(x, Y + NoteHeight / 2, z);
                item.label.localPosition = new Vector3(x, Y + NoteHeight + 0.012f, clampedFront + 0.5f * keyboard.Unit);

                string state = note.state == "hit" || note.state == "miss" ? note.state : item.baseState;
                if (state != item.shown)
                {
                    item.shown = state;
                    var c = StateColors[state];
                    SceneKit.SetColor(item.material, state == "miss" ? Color.Lerp(c, Palette.Current.road, 0.45f) : c);
                    SceneKit.SetEmission(item.material, c * (state == "hit" ? 0.9f : state == "miss" ? 0.05f : 0.4f));
                }
            }

            float firstBeat = Mathf.Ceil(time / secondsPerBeat);
            for (int i = 0; i < beatLines.Count; i++)
            {
                float beat = firstBeat + i;
                float z = HitZ + (beat * secondsPerBeat - time) * v;
                var line = beatLines[i];
                SetActive(line, z < HitZ + Length);
                line.localPosition = new Vector3(0, Y - 0.008f, z);
                bool isBar = ((int)beat % beatsPerBar + beatsPerBar) % beatsPerBar == 0;
                line.GetComponent<MeshRenderer>().sharedMaterial = isBar ? barMaterial : beatMaterial;
            }
        }

        static void SetActive(Transform t, bool on)
        {
            if (t.gameObject.activeSelf != on) t.gameObject.SetActive(on);
        }

        static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}
