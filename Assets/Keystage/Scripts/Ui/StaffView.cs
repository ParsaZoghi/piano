using Keystage.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Keystage.Ui
{
    /// <summary>
    /// A treble staff with one note on it, drawn with Painter2D (port of
    /// ui/staff.js). Beats pick the written value: 4 whole, 3 dotted half,
    /// 2 half, 1.5 dotted quarter, 1 quarter, 0.5 eighth. Coordinates use the
    /// web version's 130 x 100 viewBox and are scaled to the element.
    /// </summary>
    public class StaffView : VisualElement
    {
        const float BottomLine = 66f; // y of E4
        const float HalfSpace = 5f;
        const float NoteX = 82f;
        const float StemLength = 30f;
        static readonly Color Ink = new Color(0.059f, 0.09f, 0.165f);
        static readonly Color LineInk = new Color(0.2f, 0.255f, 0.333f);

        int? midi;
        float beats = 1f;
        readonly Label clef;
        readonly Label sharp;

        public StaffView()
        {
            AddToClassList("staff");
            pickingMode = PickingMode.Ignore;
            clef = Glyph("𝄞");
            sharp = Glyph("♯");
            generateVisualContent += Draw;
            RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        Label Glyph(string text)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("staff__glyph");
            label.style.unityFontDefinition = Fonts.SymbolDefinition;
            Add(label);
            return label;
        }

        /// <summary>Show a note (null for an empty staff).</summary>
        public void Set(int? note, float noteBeats = 1f)
        {
            midi = note;
            beats = noteBeats;
            Layout();
            MarkDirtyRepaint();
        }

        float Scale => Mathf.Min(contentRect.width / 130f, contentRect.height / 100f);
        Vector2 Origin => new Vector2((contentRect.width - 130f * Scale) / 2, (contentRect.height - 100f * Scale) / 2);
        Vector2 P(float x, float y) => Origin + new Vector2(x, y) * Scale;

        void Layout()
        {
            float s = Scale;
            if (s <= 0) return;
            clef.style.fontSize = 46 * s;
            var c = P(-4, BottomLine - 52);
            clef.style.left = c.x;
            clef.style.top = c.y;
            clef.style.width = 40 * s;
            clef.style.height = 70 * s;

            bool showSharp = midi.HasValue && Notes.IsBlack(midi.Value);
            sharp.style.display = showSharp ? DisplayStyle.Flex : DisplayStyle.None;
            if (showSharp)
            {
                float y = BottomLine - Notes.StaffStep(midi.Value) * HalfSpace;
                sharp.style.fontSize = 18 * s;
                var p = P(NoteX - 28, y - 12);
                sharp.style.left = p.x;
                sharp.style.top = p.y;
                sharp.style.width = 12 * s;
                sharp.style.height = 24 * s;
            }
        }

        void Draw(MeshGenerationContext ctx)
        {
            float s = Scale;
            if (s <= 0) return;
            var p = ctx.painter2D;
            p.lineCap = LineCap.Butt;

            p.strokeColor = LineInk;
            p.lineWidth = 1.2f * s;
            for (int i = 0; i < 5; i++)
            {
                float y = BottomLine - i * HalfSpace * 2;
                Line(p, P(6, y), P(124, y));
            }
            if (!midi.HasValue) return;

            int step = Notes.StaffStep(midi.Value);
            float ny = BottomLine - step * HalfSpace;
            // Ledger lines for notes below or above the five lines.
            for (int st = -2; st >= step; st -= 2) Line(p, P(NoteX - 12, BottomLine - st * HalfSpace), P(NoteX + 12, BottomLine - st * HalfSpace));
            for (int st = 10; st <= step; st += 2) Line(p, P(NoteX - 12, BottomLine - st * HalfSpace), P(NoteX + 12, BottomLine - st * HalfSpace));

            bool hollow = beats >= 2;
            if (beats < 4)
            {
                bool up = step < 4;
                float stemX = up ? NoteX + 6.2f : NoteX - 6.2f;
                float end = up ? ny - StemLength : ny + StemLength;
                p.strokeColor = LineInk;
                p.lineWidth = 1.6f * s;
                Line(p, P(stemX, ny), P(stemX, end));
                if (Mathf.Approximately(beats, 0.5f))
                {
                    // The flag: a short curve off the end of the stem.
                    p.strokeColor = Ink;
                    p.lineWidth = 2.2f * s;
                    p.lineCap = LineCap.Round;
                    p.BeginPath();
                    p.MoveTo(P(stemX, end));
                    float dir = up ? 1 : -1;
                    p.BezierCurveTo(P(stemX + 1, end + 7 * dir), P(stemX + 9, end + 8 * dir), P(stemX + 7.5f, end + 18 * dir));
                    p.Stroke();
                    p.lineCap = LineCap.Butt;
                }
            }

            // Note head: an ellipse rotated -20 degrees.
            p.BeginPath();
            const int segments = 28;
            float rot = -20f * Mathf.Deg2Rad;
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2;
                float ex = 6.8f * Mathf.Cos(a), ey = 4.9f * Mathf.Sin(a);
                var pt = P(NoteX + ex * Mathf.Cos(rot) - ey * Mathf.Sin(rot), ny + ex * Mathf.Sin(rot) + ey * Mathf.Cos(rot));
                if (i == 0) p.MoveTo(pt);
                else p.LineTo(pt);
            }
            p.ClosePath();
            if (hollow)
            {
                p.fillColor = new Color(0.973f, 0.98f, 0.988f);
                p.Fill();
                p.strokeColor = Ink;
                p.lineWidth = 2f * s;
                p.Stroke();
            }
            else
            {
                p.fillColor = Ink;
                p.Fill();
            }

            if (Mathf.Approximately(beats, 1.5f) || Mathf.Approximately(beats, 3f))
            {
                // The dot sits in a space, never on a line.
                float dotY = step % 2 == 0 ? ny - HalfSpace : ny;
                p.fillColor = Ink;
                p.BeginPath();
                p.Arc(P(NoteX + 12, dotY), 1.9f * s, 0f, 360f);
                p.Fill();
            }
        }

        static void Line(Painter2D p, Vector2 a, Vector2 b)
        {
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(b);
            p.Stroke();
        }
    }
}
