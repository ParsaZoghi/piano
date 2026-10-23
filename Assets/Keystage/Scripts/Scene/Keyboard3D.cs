using System.Collections.Generic;
using Keystage.Core;
using UnityEngine;

namespace Keystage.Scene
{
    /// <summary>
    /// The 3D keys (port of three/Keyboard3D.js). Units: one white key is `unit`
    /// wide. Keys run toward the player along -Z with their back edge at z = 0,
    /// so they can hinge there when pressed.
    /// </summary>
    public class Keyboard3D
    {
        public const float WhiteLength = 6f;
        public const float WhiteTop = 0.9f;
        const float BlackLength = 3.8f;
        const float BlackTop = 1.2f;
        const float BlackHeight = 0.64f;
        // Black keys are not centred between whites on a real piano.
        static readonly Dictionary<int, float> BlackNudge = new Dictionary<int, float> { [1] = -0.08f, [3] = 0.08f, [6] = -0.1f, [8] = 0f, [10] = 0.1f };

        static readonly Color Pressed = Palette.Hex(0x22c55e);
        static readonly Color HintColor = Palette.Hex(0x22d3ee);
        static readonly Color Good = Palette.Hex(0x4ade80);
        static readonly Color Bad = Palette.Hex(0xef4444);
        static readonly Color WhiteBase = Palette.Hex(0xf5f2ea);
        static readonly Color BlackBase = Palette.Hex(0x16161f);
        static readonly Color NameColor = Palette.Hex(0x475569);
        static readonly Color BadgeFill = Palette.Hex(0x4338ca);
        static readonly Color BadgeDarkFill = Palette.Hex(0xc7d2fe);
        static readonly Color BadgeDarkText = Palette.Hex(0x0f172a);

        public class Key
        {
            public int midi;
            public bool black;
            public float x;
            public Transform pivot;
            public Material material;
            public Color baseColor;
            public Color tint = Color.black;
            public bool pressed;
            public float angle;
            public Color flash;
            public float flashLeft;
            public float glow;
            public readonly List<GameObject> labels = new List<GameObject>();
        }

        public readonly Transform Root;
        public readonly Dictionary<int, Key> Keys = new Dictionary<int, Key>();
        public float Width { get; private set; }
        public float Unit { get; private set; } = 1f;
        public int Lo { get; private set; }
        public int Hi { get; private set; }
        public int? HintMidi { get; set; }

        readonly Material caseMaterial;
        readonly Material feltMaterial;
        readonly List<Material> badgeMaterials = new List<Material>();
        readonly Material badgeMaterial, badgeDarkMaterial;

        public Keyboard3D(Transform parent)
        {
            Root = new GameObject("Keyboard").transform;
            Root.SetParent(parent, false);
            caseMaterial = SceneKit.Lit(Palette.Current.frame, 0.5f, 0.05f);
            feltMaterial = SceneKit.Lit(Palette.Current.felt, 1f);
            badgeMaterial = SceneKit.Unlit(BadgeFill);
            badgeDarkMaterial = SceneKit.Unlit(BadgeDarkFill);
        }

        public void ApplyPalette()
        {
            SceneKit.SetColor(caseMaterial, Palette.Current.frame);
            SceneKit.SetColor(feltMaterial, Palette.Current.felt);
        }

        /// <summary>Build keys for the inclusive MIDI range, widened to end on white keys.</summary>
        public void Build(int lo, int hi, float unit = 1f)
        {
            Clear();
            while (Notes.IsBlack(lo)) lo--;
            while (Notes.IsBlack(hi)) hi++;
            Lo = lo;
            Hi = hi;
            Unit = unit;

            int whites = 0;
            for (int m = lo; m <= hi; m++)
                if (!Notes.IsBlack(m)) whites++;
            Width = whites * unit;
            float origin = -(whites - 1) / 2f;

            var whiteMesh = SceneKit.RoundedBox(0.95f, WhiteTop, WhiteLength, 0.07f);
            var blackMesh = SceneKit.RoundedBox(0.56f, BlackHeight, BlackLength, 0.06f);
            int whiteIndex = 0;
            for (int midi = lo; midi <= hi; midi++)
            {
                bool black = Notes.IsBlack(midi);
                float x = (black ? origin + whiteIndex - 0.5f + BlackNudge[midi % 12] : origin + whiteIndex++) * unit;
                var baseColor = black ? BlackBase : WhiteBase;
                var material = black ? SceneKit.Lit(baseColor, 0.35f, 0.1f) : SceneKit.Lit(baseColor, 0.3f);

                var pivot = new GameObject("Key " + midi).transform;
                pivot.SetParent(Root, false);
                pivot.localPosition = new Vector3(x, 0, 0);
                var mesh = SceneKit.MeshObject("Mesh", black ? blackMesh : whiteMesh, material, pivot);
                mesh.transform.localScale = new Vector3(unit, 1, 1);
                if (black) mesh.transform.localPosition = new Vector3(0, BlackTop - BlackHeight, 0);
                var collider = mesh.AddComponent<BoxCollider>();
                collider.center = black ? new Vector3(0, BlackHeight / 2, -BlackLength / 2) : new Vector3(0, WhiteTop / 2, -WhiteLength / 2);
                collider.size = black ? new Vector3(0.56f, BlackHeight, BlackLength) : new Vector3(0.95f, WhiteTop, WhiteLength);
                mesh.AddComponent<KeyTag>().midi = midi;

                Keys[midi] = new Key { midi = midi, black = black, x = x, pivot = pivot, material = material, baseColor = baseColor };
            }
            BuildCase();
        }

        void BuildCase()
        {
            float w = Width;
            void Add(Material m, Vector3 size, Vector3 center)
            {
                var go = SceneKit.MeshObject("Case", SceneKit.Cube, m, Root);
                go.transform.localPosition = center;
                go.transform.localScale = size;
            }
            Add(caseMaterial, new Vector3(w + 1.3f, 0.5f, WhiteLength + 0.9f), new Vector3(0, -0.27f, -(WhiteLength / 2 - 0.15f)));
            Add(feltMaterial, new Vector3(w + 0.1f, 0.1f, 0.22f), new Vector3(0, WhiteTop + 0.03f, 0.1f));
            foreach (int side in new[] { -1, 1 })
                Add(caseMaterial, new Vector3(0.55f, 1.25f, WhiteLength + 0.9f), new Vector3(side * (w / 2 + 0.33f), 0.36f, -(WhiteLength / 2 - 0.15f)));
        }

        public void Clear()
        {
            foreach (var key in Keys.Values) Object.Destroy(key.material);
            for (int i = Root.childCount - 1; i >= 0; i--) Object.Destroy(Root.GetChild(i).gameObject);
            Keys.Clear();
            HintMidi = null;
        }

        public bool Has(int midi) => Keys.ContainsKey(midi);
        public float XOf(int midi) => Keys.TryGetValue(midi, out var k) ? k.x : 0f;

        /// <summary>names: note names on white keys. typingMap: midi to typing-key character, or null.</summary>
        public void SetLabels(bool names, Dictionary<int, string> typingMap)
        {
            float grow = Mathf.Min(Unit, 1.3f);
            foreach (var key in Keys.Values)
            {
                foreach (var label in key.labels) Object.Destroy(label);
                key.labels.Clear();

                if (names && !key.black)
                {
                    bool isC = key.midi % 12 == 0;
                    string text = isC ? Notes.PitchName(key.midi) + Notes.OctaveOf(key.midi) : Notes.PitchName(key.midi);
                    AddText(key, text, NameColor, (isC ? 0.82f : 0.74f) * grow, WhiteTop + 0.006f, -5.35f);
                }
                if (typingMap != null && typingMap.TryGetValue(key.midi, out var ch))
                {
                    if (key.black) AddBadge(key, ch, badgeDarkMaterial, BadgeDarkText, 0.52f * grow, BlackTop + 0.006f, -3.3f);
                    else AddBadge(key, ch, badgeMaterial, Color.white, 0.7f * grow, WhiteTop + 0.006f, -4.4f);
                }
            }
        }

        void AddText(Key key, string text, Color color, float plane, float y, float z)
        {
            var tm = SceneKit.Label(text, color, SceneKit.CapHeight(text, plane), key.pivot);
            tm.transform.localPosition = new Vector3(0, y + 0.002f, z);
            key.labels.Add(tm.gameObject);
        }

        void AddBadge(Key key, string text, Material fill, Color textColor, float plane, float y, float z)
        {
            var badge = SceneKit.MeshObject("Badge", SceneKit.RoundedRect, fill, key.pivot, false);
            badge.transform.localPosition = new Vector3(0, y, z);
            badge.transform.localScale = Vector3.one * plane * 0.75f;
            key.labels.Add(badge);
            AddText(key, text, textColor, plane, y, z);
        }

        public void Press(int midi, bool on)
        {
            if (Keys.TryGetValue(midi, out var key)) key.pressed = on;
        }

        public void ReleaseAll()
        {
            foreach (var key in Keys.Values) key.pressed = false;
        }

        /// <summary>Flash a key green (good) or red (bad).</summary>
        public void Flash(int midi, bool good)
        {
            if (!Keys.TryGetValue(midi, out var key)) return;
            key.flash = good ? Good : Bad;
            key.flashLeft = 0.35f;
        }

        public void Update(float dt, float time)
        {
            float ease = Mathf.Min(1, dt * 28);
            float pulse = 0.55f + 0.25f * Mathf.Sin(time * 6);
            foreach (var key in Keys.Values)
            {
                float targetAngle = key.pressed ? (key.black ? 0.075f : 0.052f) : 0f;
                key.angle += (targetAngle - key.angle) * ease;
                key.pivot.localRotation = Quaternion.Euler(-key.angle * Mathf.Rad2Deg, 0, 0);

                Color color = Color.black;
                float strength = 0f;
                if (key.flashLeft > 0)
                {
                    key.flashLeft -= dt;
                    color = key.flash;
                    strength = 0.9f;
                }
                else if (key.pressed)
                {
                    color = Pressed;
                    strength = 0.6f;
                }
                else if (HintMidi == key.midi)
                {
                    color = HintColor;
                    strength = pulse;
                }
                if (strength > 0) key.tint = Color.Lerp(key.tint, color, Mathf.Min(1, dt * 40));
                key.glow += (strength - key.glow) * Mathf.Min(1, dt * 18);
                // White keys are too bright for a glow to show, so tint the surface too.
                SceneKit.SetColor(key.material, Color.Lerp(key.baseColor, key.tint, key.glow * (key.black ? 0.5f : 0.8f)));
                SceneKit.SetEmission(key.material, key.tint * key.glow * (key.black ? 1f : 0.35f));
            }
        }
    }
}
