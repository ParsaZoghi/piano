using System;
using System.Collections.Generic;
using Keystage.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Keystage.Scene
{
    public enum View { Menu, Free, Find, Lesson }

    /// <summary>
    /// The 3D stage: camera, lights, floor, keyboard and runway, plus the
    /// learner's camera controls (port of three/PianoScene.js). Unity is
    /// left-handed, so the web version's z values are mirrored: the player
    /// sits at -Z and the runway stretches toward +Z.
    /// </summary>
    public class PianoStage : MonoBehaviour
    {
        // Camera presets. depth is the stretch of floor (near z, far z) that must
        // stay on screen; top / bottom are the fractions of the screen kept clear
        // for the HUD (portrait overrides them on tall screens); zoom below 1 lets
        // the keyboard run off the sides (menu backdrop).
        struct Preset
        {
            public float pitch, near, far, top, bottom, zoom, minDistance;
            public float portraitTop, portraitBottom;
        }

        static readonly Dictionary<View, Preset> Views = new Dictionary<View, Preset>
        {
            [View.Menu] = new Preset { pitch = 20, near = -6.6f, far = 0, top = 0.6f, bottom = 0.08f, zoom = 0.42f, minDistance = 10, portraitTop = -1 },
            [View.Free] = new Preset { pitch = 52, near = -6.6f, far = 0.4f, top = 0.16f, bottom = 0.14f, zoom = 1, minDistance = 10, portraitTop = -1 },
            [View.Find] = new Preset { pitch = 46, near = -6.6f, far = 0.4f, top = 0.5f, bottom = 0.16f, zoom = 1, minDistance = 16, portraitTop = 0.44f, portraitBottom = 0.2f },
            [View.Lesson] = new Preset { pitch = 36, near = -6.6f, far = NoteHighway.Length * 0.6f, top = 0.12f, bottom = 0.15f, zoom = 1, minDistance = 20, portraitTop = 0.4f, portraitBottom = 0.25f },
        };

        const float ZoomMin = 0.3f;
        const float ZoomMax = 2.6f;
        const float Fov = 34f;

        public Camera Camera { get; private set; }
        public Keyboard3D Keyboard { get; private set; }
        public NoteHighway Highway { get; private set; }
        public View View { get; private set; } = View.Menu;
        public bool Interactive => View != View.Menu;
        public float Time { get; private set; }

        /// <summary>Called every frame with (deltaTime, time) before the keys animate.</summary>
        public event Action<float, float> Frame;

        Light sun, rim;
        Material floorMaterial;
        Vector3 target, home, camRight, camUp;
        float homeDistance = 20f;
        float zoom = 1f, panX, panY;
        int lastWidth, lastHeight;

        public void Init(Camera cam)
        {
            Camera = cam;
            Camera.fieldOfView = Fov;
            Camera.nearClipPlane = 0.5f;
            Camera.farClipPlane = 400f;
            Camera.clearFlags = CameraClearFlags.SolidColor;

            // Shadows must reach the far end of an 88-key keyboard.
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp) urp.shadowDistance = 160f;

            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(transform, false);
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            sun.transform.rotation = Quaternion.LookRotation(-new Vector3(-8, 22, -14).normalized);

            var rimGo = new GameObject("Rim");
            rimGo.transform.SetParent(transform, false);
            rimGo.transform.position = new Vector3(0, 6, 10);
            rim = rimGo.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.color = Palette.Hex(0x6366f1);
            rim.range = 60f;

            floorMaterial = SceneKit.Lit(Palette.Current.floor, 0.9f);
            var floor = SceneKit.MeshObject("Floor", SceneKit.FlatQuad, floorMaterial, transform);
            floor.transform.localPosition = new Vector3(0, -0.52f, 0);
            floor.transform.localScale = new Vector3(600, 1, 600);
            floor.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

            Keyboard = new Keyboard3D(transform);
            Highway = new NoteHighway(Keyboard, transform);
            ApplyPalette();
        }

        public void SetTheme(string theme)
        {
            Palette.Set(theme);
            ApplyPalette();
        }

        void ApplyPalette()
        {
            var p = Palette.Current;
            Camera.backgroundColor = p.background;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = p.background;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            // No sky: keep sky reflections from washing out the glossy black keys.
            RenderSettings.skybox = null;
            RenderSettings.reflectionIntensity = 0.15f;
            RenderSettings.ambientSkyColor = p.sky * p.ambient;
            RenderSettings.ambientEquatorColor = Color.Lerp(p.sky, p.ground, 0.5f) * p.ambient;
            RenderSettings.ambientGroundColor = p.ground * p.ambient;
            rim.intensity = p.rim;
            SceneKit.SetColor(floorMaterial, p.floor);
            Keyboard.ApplyPalette();
            Highway.ApplyPalette();
        }

        /// <summary>Show the keys lo..hi with the chosen camera preset.</summary>
        public void Configure(int lo = Notes.PianoMin, int hi = Notes.PianoMax, View view = View.Free, bool highway = false, float unit = 1f)
        {
            InputBus.ReleaseAll();
            Keyboard.Build(lo, hi, unit);
            Highway.ClearNotes();
            Highway.Visible = highway;
            if (highway) Highway.Layout();
            View = view;
            zoom = 1f;
            panX = panY = 0f;
            Fit();
        }

        void Update()
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight) Fit();
            float dt = Mathf.Min(UnityEngine.Time.unscaledDeltaTime, 0.1f);
            Time += dt;
            if (View == View.Menu && !Ui.Motion.Reduced)
            {
                Camera.transform.position = home + new Vector3(Mathf.Sin(Time * 0.16f) * Keyboard.Width * 0.12f, 0, 0);
                Camera.transform.LookAt(target);
            }
            Frame?.Invoke(dt, Time);
            Keyboard.Update(dt, Time);
        }

        /// <summary>Back the camera away until every corner of the important area sits inside the clear band.</summary>
        public void Fit()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            float aspect = Screen.height > 0 ? Screen.width / (float)Screen.height : 1.6f;
            var preset = Views[View];
            float top = preset.top, bottom = preset.bottom;
            if (aspect < 0.8f && preset.portraitTop >= 0)
            {
                top = preset.portraitTop;
                bottom = preset.portraitBottom;
            }

            // Shift the picture so its centre lands in the middle of the band between
            // the top and bottom HUD, instead of the middle of the screen.
            var projection = Matrix4x4.Perspective(Fov, aspect, Camera.nearClipPlane, Camera.farClipPlane);
            projection[1, 2] = top - bottom;
            Camera.projectionMatrix = projection;

            float angle = preset.pitch * Mathf.Deg2Rad;
            float halfWidth = (Keyboard.Width / 2 + 0.9f) * preset.zoom;
            Vector3[] corners =
            {
                new Vector3(-halfWidth, 0, preset.near), new Vector3(halfWidth, 0, preset.near),
                new Vector3(-halfWidth, 1.2f, preset.far), new Vector3(halfWidth, 1.2f, preset.far),
            };
            target = new Vector3(0, 0.5f, (preset.near + preset.far) / 2);
            var direction = new Vector3(0, Mathf.Sin(angle), -Mathf.Cos(angle));

            float distance = preset.minDistance;
            for (int i = 0; i < 160; i++)
            {
                Camera.transform.position = target + direction * distance;
                Camera.transform.LookAt(target);
                bool fits = true;
                foreach (var c in corners)
                {
                    var vp = Camera.WorldToViewportPoint(c);
                    float x = vp.x * 2 - 1, y = vp.y * 2 - 1;
                    if (Mathf.Abs(x) > 0.96f || y < -1 + bottom * 2 || y > 1 - top * 2 || vp.z <= 0) fits = false;
                }
                if (fits) break;
                distance *= 1.04f;
            }
            home = Camera.transform.position;
            homeDistance = distance;
            camRight = Camera.transform.right;
            camUp = Camera.transform.up;

            ApplyCamera();
        }

        /// <summary>Home position, then the learner's pan (in the screen plane) and zoom.</summary>
        void ApplyCamera()
        {
            var t = target + camRight * panX + camUp * panY;
            Camera.transform.position = (home - target) * zoom + t;
            Camera.transform.LookAt(t, Vector3.up);
            // Fog starts just past the keyboard so the floor melts into the background at the horizon.
            float distance = homeDistance * zoom;
            RenderSettings.fogStartDistance = distance + 8f;
            RenderSettings.fogEndDistance = distance * 3.2f + NoteHighway.Length;
        }

        public void ZoomBy(float factor)
        {
            if (!Interactive) return;
            zoom = Mathf.Clamp(zoom * factor, ZoomMin, ZoomMax);
            ApplyCamera();
        }

        /// <summary>Move the view by a screen distance in pixels (content follows the pointer).</summary>
        public void PanByPixels(float dx, float dy)
        {
            if (!Interactive) return;
            float tanHalf = Mathf.Tan(Fov * Mathf.Deg2Rad / 2);
            float perPixel = 2 * homeDistance * zoom * tanHalf / Mathf.Max(1, Screen.height);
            PanBy(-dx * perPixel, dy * perPixel);
        }

        /// <summary>Step the camera: dirX/dirY are -1, 0 or 1 (right / up are positive).</summary>
        public void Step(int dirX, int dirY)
        {
            float tanHalf = Mathf.Tan(Fov * Mathf.Deg2Rad / 2);
            float step = 0.25f * homeDistance * zoom * tanHalf;
            PanBy(dirX * step, dirY * step);
        }

        void PanBy(float x, float y)
        {
            if (!Interactive) return;
            float limitX = Keyboard.Width / 2 + 8;
            panX = Mathf.Clamp(panX + x, -limitX, limitX);
            panY = Mathf.Clamp(panY + y, -16, 16);
            ApplyCamera();
        }

        public void ResetCamera()
        {
            zoom = 1f;
            panX = panY = 0f;
            ApplyCamera();
        }

        /// <summary>The key under a screen position (pixels, origin bottom-left), or -1.</summary>
        public int KeyAt(Vector2 screen)
        {
            var ray = Camera.ScreenPointToRay(screen);
            if (Physics.Raycast(ray, out var hit, 1000f) && hit.collider.TryGetComponent<KeyTag>(out var tag)) return tag.midi;
            return -1;
        }
    }
}
