using System.Collections.Generic;
using UnityEngine;

namespace Keystage.Scene
{
    /// <summary>Meshes, materials and text decals shared by the 3D scene.</summary>
    public static class SceneKit
    {
        static Material litBase, unlitBase, labelBase;
        static Font labelFont;
        static readonly Dictionary<string, Mesh> RoundedBoxes = new Dictionary<string, Mesh>();
        static Mesh cube, quad, roundedRect;

        public static Font LabelFont => labelFont != null ? labelFont : labelFont = Resources.Load<Font>("Fonts/Poppins-Bold");

        static Material LitBase => litBase != null ? litBase : litBase = Resources.Load<Material>("Materials/Lit");
        static Material UnlitBase => unlitBase != null ? unlitBase : unlitBase = Resources.Load<Material>("Materials/Unlit");
        static Material LabelBase => labelBase != null ? labelBase : labelBase = Resources.Load<Material>("Materials/Label");

        /// <summary>A lit (URP/Lit) material. roughness follows the three.js version.</summary>
        public static Material Lit(Color color, float roughness = 0.5f, float metalness = 0f)
        {
            var m = new Material(LitBase);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", 1f - roughness);
            m.SetFloat("_Metallic", metalness);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            m.SetColor("_EmissionColor", Color.black);
            return m;
        }

        public static Material Unlit(Color color)
        {
            var m = new Material(UnlitBase);
            m.SetColor("_BaseColor", color);
            return m;
        }

        public static void SetColor(Material m, Color color) => m.SetColor("_BaseColor", color);
        public static void SetEmission(Material m, Color color) => m.SetColor("_EmissionColor", color);

        public static Mesh Cube
        {
            get
            {
                if (cube != null) return cube;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = go.GetComponent<MeshFilter>().sharedMesh;
                Object.Destroy(go);
                return cube;
            }
        }

        /// <summary>A flat 1x1 quad lying in the XZ plane, facing up.</summary>
        public static Mesh FlatQuad
        {
            get
            {
                if (quad != null) return quad;
                quad = new Mesh { name = "FlatQuad" };
                quad.vertices = new[] { new Vector3(-0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, -0.5f) };
                quad.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                quad.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
                quad.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                return quad;
            }
        }

        /// <summary>A flat 1x1 rounded rectangle in the XZ plane (badges behind typing letters).</summary>
        public static Mesh RoundedRect
        {
            get
            {
                if (roundedRect != null) return roundedRect;
                const float half = 0.5f, r = 0.2f;
                const int segments = 6;
                var verts = new List<Vector3> { Vector3.zero };
                Vector2[] centers = { new Vector2(half - r, half - r), new Vector2(-half + r, half - r), new Vector2(-half + r, -half + r), new Vector2(half - r, -half + r) };
                for (int c = 0; c < 4; c++)
                    for (int s = 0; s <= segments; s++)
                    {
                        float a = (c * 90f + s * 90f / segments) * Mathf.Deg2Rad;
                        verts.Add(new Vector3(centers[c].x + Mathf.Cos(a) * r, 0, centers[c].y + Mathf.Sin(a) * r));
                    }
                var tris = new List<int>();
                for (int i = 1; i < verts.Count; i++)
                {
                    int next = i + 1 < verts.Count ? i + 1 : 1;
                    tris.AddRange(new[] { 0, next, i });
                }
                roundedRect = new Mesh { name = "RoundedRect", vertices = verts.ToArray(), triangles = tris.ToArray() };
                roundedRect.RecalculateNormals();
                return roundedRect;
            }
        }

        /// <summary>
        /// A box with rounded edges (like three.js RoundedBoxGeometry). The box spans
        /// x: ±w/2, y: 0..h, z: -d..0 so keys hinge at their back edge (z = 0).
        /// </summary>
        public static Mesh RoundedBox(float w, float h, float d, float radius)
        {
            string key = $"{w}|{h}|{d}|{radius}";
            if (RoundedBoxes.TryGetValue(key, out var cached) && cached != null) return cached;

            var half = new Vector3(w / 2, h / 2, d / 2);
            var inner = half - Vector3.one * radius;
            // Grid coordinates per axis: the face edges, two points on the rounding, the flat middle.
            float[] Axis(float h2) => new[] { -h2, -h2 + radius * 0.29f, -h2 + radius, h2 - radius, h2 - radius * 0.29f, h2 };

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var tris = new List<int>();
            Vector3[] faceNormals = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (var n in faceNormals)
            {
                // Two axes spanning the face.
                Vector3 u = Mathf.Abs(n.x) > 0 ? Vector3.forward : Vector3.right;
                Vector3 v = Vector3.Cross(n, u);
                float[] us = Axis(Vector3.Scale(u, half).magnitude);
                float[] vs = Axis(Vector3.Scale(v, half).magnitude);
                float depth = Vector3.Scale(n, half).magnitude;
                int start = verts.Count;
                for (int i = 0; i < us.Length; i++)
                {
                    for (int j = 0; j < vs.Length; j++)
                    {
                        Vector3 p = n * depth + u * us[i] + v * vs[j];
                        var clamped = new Vector3(Mathf.Clamp(p.x, -inner.x, inner.x), Mathf.Clamp(p.y, -inner.y, inner.y), Mathf.Clamp(p.z, -inner.z, inner.z));
                        Vector3 dir = p - clamped;
                        Vector3 normal = dir.sqrMagnitude > 1e-8f ? dir.normalized : n;
                        verts.Add(clamped + normal * radius + new Vector3(0, half.y, -half.z));
                        normals.Add(normal);
                    }
                }
                int cols = vs.Length;
                for (int i = 0; i < us.Length - 1; i++)
                {
                    for (int j = 0; j < cols - 1; j++)
                    {
                        int a = start + i * cols + j, b = a + 1, c = a + cols, e = c + 1;
                        tris.AddRange(new[] { a, b, e, a, e, c });
                    }
                }
            }

            var mesh = new Mesh { name = "RoundedBox" };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);
            // Make every triangle face outward whatever the face axes' handedness.
            FixWinding(mesh);
            mesh.RecalculateBounds();
            RoundedBoxes[key] = mesh;
            return mesh;
        }

        static void FixWinding(Mesh mesh)
        {
            var v = mesh.vertices;
            var n = mesh.normals;
            var t = mesh.triangles;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 faceNormal = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
                Vector3 avg = n[t[i]] + n[t[i + 1]] + n[t[i + 2]];
                if (Vector3.Dot(faceNormal, avg) < 0) (t[i + 1], t[i + 2]) = (t[i + 2], t[i + 1]);
            }
            mesh.triangles = t;
        }

        public static GameObject MeshObject(string name, Mesh mesh, Material material, Transform parent, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = shadows;
            return go;
        }

        // Longer names (solfège "Sol♯") get a smaller font so they still fit.
        static readonly float[] FontPx = { 70, 70, 54, 44, 36, 30 };

        /// <summary>Capital-letter height for text on a label tile of the given size (matches the web canvas labels).</summary>
        public static float CapHeight(string text, float tile) => tile * FontPx[Mathf.Min(text.Length, 5)] / 128f * 0.72f;

        /// <summary>
        /// A flat, upward-facing text decal (note names, typing keys). size is the
        /// height of a capital letter in world units.
        /// </summary>
        public static TextMesh Label(string text, Color color, float size, Transform parent)
        {
            var go = new GameObject("Label " + text);
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var tm = go.AddComponent<TextMesh>();
            tm.font = LabelFont;
            tm.fontSize = 96;
            tm.characterSize = size * 0.135f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.text = text;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = LabelMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return tm;
        }

        static Material labelMaterial;

        static Material LabelMaterial
        {
            get
            {
                if (labelMaterial != null) return labelMaterial;
                labelMaterial = new Material(LabelBase) { mainTexture = LabelFont.material.mainTexture };
                // The dynamic font atlas is rebuilt when new characters appear; follow it.
                Font.textureRebuilt += f =>
                {
                    if (f == LabelFont && labelMaterial != null) labelMaterial.mainTexture = f.material.mainTexture;
                };
                return labelMaterial;
            }
        }
    }
}
