using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Keystage.Ui
{
    /// <summary>
    /// Lucide-style stroke icons (24px grid, 2px stroke), the same SVG markup as
    /// the web version's ui/dom.js. Each icon is drawn once with Painter2D into a
    /// white VectorImage; USS tints it (-unity-background-image-tint-color).
    /// </summary>
    public static class Icons
    {
        static readonly Dictionary<string, string> Svg = new Dictionary<string, string>
        {
            ["back"] = "<path d=\"m12 19-7-7 7-7\"/><path d=\"M19 12H5\"/>",
            ["play"] = "<polygon points=\"6 3 20 12 6 21 6 3\"/>",
            ["music"] = "<path d=\"M9 18V5l12-2v13\"/><circle cx=\"6\" cy=\"18\" r=\"3\"/><circle cx=\"18\" cy=\"16\" r=\"3\"/>",
            ["book"] = "<path d=\"M12 7v14\"/><path d=\"M3 18a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h5a4 4 0 0 1 4 4 4 4 0 0 1 4-4h5a1 1 0 0 1 1 1v13a1 1 0 0 1-1 1h-6a3 3 0 0 0-3 3 3 3 0 0 0-3-3z\"/>",
            ["phone"] = "<rect width=\"14\" height=\"20\" x=\"5\" y=\"2\" rx=\"2\" ry=\"2\"/><path d=\"M12 18h.01\"/>",
            ["lock"] = "<rect width=\"18\" height=\"11\" x=\"3\" y=\"11\" rx=\"2\" ry=\"2\"/><path d=\"M7 11V7a5 5 0 0 1 10 0v4\"/>",
            ["star"] = "<polygon points=\"12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2\"/>",
            ["restart"] = "<path d=\"M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8\"/><path d=\"M3 3v5h5\"/>",
            ["close"] = "<path d=\"M18 6 6 18\"/><path d=\"m6 6 12 12\"/>",
            ["check"] = "<path d=\"M20 6 9 17l-5-5\"/>",
            ["volume"] = "<polygon points=\"11 5 6 9 2 9 2 15 6 15 11 19 11 5\"/><path d=\"M15.54 8.46a5 5 0 0 1 0 7.07\"/><path d=\"M19.07 4.93a10 10 0 0 1 0 14.14\"/>",
            ["plug"] = "<path d=\"M12 22v-5\"/><path d=\"M9 8V2\"/><path d=\"M15 8V2\"/><path d=\"M18 8v5a4 4 0 0 1-4 4h-4a4 4 0 0 1-4-4V8Z\"/>",
            ["next"] = "<path d=\"m9 18 6-6-6-6\"/>",
            ["listen"] = "<path d=\"M3 14h3a2 2 0 0 1 2 2v3a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-7a9 9 0 0 1 18 0v7a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3\"/>",
            ["keyboard"] = "<path d=\"M10 8h.01\"/><path d=\"M12 12h.01\"/><path d=\"M14 8h.01\"/><path d=\"M16 12h.01\"/><path d=\"M18 8h.01\"/><path d=\"M6 8h.01\"/><path d=\"M7 16h10\"/><path d=\"M8 12h.01\"/><rect width=\"20\" height=\"16\" x=\"2\" y=\"4\" rx=\"2\"/>",
            ["minus"] = "<path d=\"M5 12h14\"/>",
            ["pause"] = "<rect x=\"14\" y=\"4\" width=\"4\" height=\"16\" rx=\"1\"/><rect x=\"6\" y=\"4\" width=\"4\" height=\"16\" rx=\"1\"/>",
            ["sun"] = "<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2\"/><path d=\"M12 20v2\"/><path d=\"m4.93 4.93 1.41 1.41\"/><path d=\"m17.66 17.66 1.41 1.41\"/><path d=\"M2 12h2\"/><path d=\"M20 12h2\"/><path d=\"m6.34 17.66-1.41 1.41\"/><path d=\"m19.07 4.93-1.41 1.41\"/>",
            ["moon"] = "<path d=\"M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z\"/>",
            ["gear"] = "<path d=\"M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>",
            ["move"] = "<path d=\"M12 2v20\"/><path d=\"m15 19-3 3-3-3\"/><path d=\"m19 9 3 3-3 3\"/><path d=\"M2 12h20\"/><path d=\"m5 9-3 3 3 3\"/><path d=\"m9 5 3-3 3 3\"/>",
            ["up"] = "<path d=\"m18 15-6-6-6 6\"/>",
            ["down"] = "<path d=\"m6 9 6 6 6-6\"/>",
            ["left"] = "<path d=\"m15 18-6-6 6-6\"/>",
            ["right"] = "<path d=\"m9 18 6-6-6-6\"/>",
            ["zoomIn"] = "<circle cx=\"11\" cy=\"11\" r=\"8\"/><path d=\"m21 21-4.3-4.3\"/><path d=\"M11 8v6\"/><path d=\"M8 11h6\"/>",
            ["zoomOut"] = "<circle cx=\"11\" cy=\"11\" r=\"8\"/><path d=\"m21 21-4.3-4.3\"/><path d=\"M8 11h6\"/>",
            ["home"] = "<path d=\"M15 21v-8a1 1 0 0 0-1-1h-4a1 1 0 0 0-1 1v8\"/><path d=\"M3 10a2 2 0 0 1 .709-1.528l7-5.999a2 2 0 0 1 2.582 0l7 5.999A2 2 0 0 1 21 10v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z\"/>",
            ["plus"] = "<path d=\"M5 12h14\"/><path d=\"M12 5v14\"/>",
        };

        static readonly Dictionary<string, VectorImage> Cache = new Dictionary<string, VectorImage>();
        static readonly Regex Element = new Regex(@"<(\w+)([^>]*)/>");
        static readonly Regex Attribute = new Regex(@"([\w-]+)=""([^""]*)""");
        static readonly Regex Token = new Regex(@"[MmLlHhVvCcSsQqTtAaZz]|-?(?:\d+\.?\d*|\.\d+)(?:[eE][-+]?\d+)?");

        /// <summary>An icon element; "back" and "next" mirror in right-to-left layouts.</summary>
        public static VisualElement Create(string name, string classes = null)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList("icon");
            if (!string.IsNullOrEmpty(classes))
                foreach (var c in classes.Split(' ')) e.AddToClassList(c);
            e.style.backgroundImage = new StyleBackground(Image(name));
            if ((name == "back" || name == "next") && Core.I18n.IsRtl) e.style.scale = new Scale(new Vector3(-1, 1, 1));
            return e;
        }

        /// <summary>A filled star (on) keeps its own gold colours; tint stays white.</summary>
        public static VisualElement Star(bool on)
        {
            var e = Create(on ? "starFilled" : "star", on ? "star star--on" : "star");
            if (on) e.style.unityBackgroundImageTintColor = Color.white;
            return e;
        }

        public static VectorImage Image(string name)
        {
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;
            bool filledStar = name == "starFilled";
            var painter = new Painter2D
            {
                lineWidth = 2f,
                lineCap = LineCap.Round,
                lineJoin = LineJoin.Round,
                strokeColor = filledStar ? new Color(0.96f, 0.62f, 0.04f) : Color.white,
                fillColor = new Color(0.984f, 0.749f, 0.141f),
            };

            // Pin the drawing to the full 24x24 box so every icon scales the same way.
            painter.fillColor = new Color(1, 1, 1, 0.004f);
            foreach (var corner in new[] { new Vector2(0, 0), new Vector2(24, 24) })
            {
                painter.BeginPath();
                painter.Arc(corner, 0.01f, 0f, 360f);
                painter.Fill();
            }
            painter.fillColor = new Color(0.984f, 0.749f, 0.141f);

            foreach (Match m in Element.Matches(Svg[filledStar ? "star" : name]))
            {
                var attrs = new Dictionary<string, string>();
                foreach (Match a in Attribute.Matches(m.Groups[2].Value)) attrs[a.Groups[1].Value] = a.Groups[2].Value;
                painter.BeginPath();
                switch (m.Groups[1].Value)
                {
                    case "path":
                        DrawPath(painter, attrs["d"]);
                        break;
                    case "circle":
                        painter.Arc(new Vector2(F(attrs, "cx"), F(attrs, "cy")), F(attrs, "r"), 0f, 360f);
                        painter.ClosePath();
                        break;
                    case "rect":
                        DrawRect(painter, F(attrs, "x"), F(attrs, "y"), F(attrs, "width"), F(attrs, "height"), attrs.ContainsKey("rx") ? F(attrs, "rx") : 0);
                        break;
                    case "polygon":
                        var nums = Numbers(attrs["points"]);
                        painter.MoveTo(new Vector2(nums[0], nums[1]));
                        for (int i = 2; i + 1 < nums.Count; i += 2) painter.LineTo(new Vector2(nums[i], nums[i + 1]));
                        painter.ClosePath();
                        break;
                }
                if (filledStar) painter.Fill();
                painter.Stroke();
            }

            var image = ScriptableObject.CreateInstance<VectorImage>();
            painter.SaveToVectorImage(image);
            painter.Dispose();
            Cache[name] = image;
            return image;
        }

        static float F(Dictionary<string, string> a, string key) => float.Parse(a[key], CultureInfo.InvariantCulture);

        static List<float> Numbers(string s)
        {
            var list = new List<float>();
            foreach (Match m in Token.Matches(s)) list.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
            return list;
        }

        static void DrawRect(Painter2D p, float x, float y, float w, float h, float r)
        {
            p.MoveTo(new Vector2(x + r, y));
            p.LineTo(new Vector2(x + w - r, y));
            if (r > 0) p.ArcTo(new Vector2(x + w, y), new Vector2(x + w, y + r), r);
            p.LineTo(new Vector2(x + w, y + h - r));
            if (r > 0) p.ArcTo(new Vector2(x + w, y + h), new Vector2(x + w - r, y + h), r);
            p.LineTo(new Vector2(x + r, y + h));
            if (r > 0) p.ArcTo(new Vector2(x, y + h), new Vector2(x, y + h - r), r);
            p.LineTo(new Vector2(x, y + r));
            if (r > 0) p.ArcTo(new Vector2(x, y), new Vector2(x + r, y), r);
            p.ClosePath();
        }

        /// <summary>Draw an SVG path "d" string (M L H V C S Q T A Z, absolute and relative).</summary>
        static void DrawPath(Painter2D p, string d)
        {
            var tokens = new List<string>();
            foreach (Match m in Token.Matches(d)) tokens.Add(m.Value);
            int i = 0;
            char cmd = 'M';
            Vector2 cur = Vector2.zero, start = Vector2.zero, lastControl = Vector2.zero;
            char lastCmd = ' ';

            bool IsCommand(int k) => k < tokens.Count && char.IsLetter(tokens[k][0]);
            float Next() => float.Parse(tokens[i++], CultureInfo.InvariantCulture);

            while (i < tokens.Count)
            {
                if (IsCommand(i)) cmd = tokens[i++][0];
                bool rel = char.IsLower(cmd);
                Vector2 Off(Vector2 v) => rel ? cur + v : v;
                switch (char.ToUpperInvariant(cmd))
                {
                    case 'M':
                        cur = Off(new Vector2(Next(), Next()));
                        start = cur;
                        p.MoveTo(cur);
                        cmd = rel ? 'l' : 'L'; // following pairs are line-tos
                        break;
                    case 'L':
                        cur = Off(new Vector2(Next(), Next()));
                        p.LineTo(cur);
                        break;
                    case 'H':
                        cur = new Vector2(rel ? cur.x + Next() : Next(), cur.y);
                        p.LineTo(cur);
                        break;
                    case 'V':
                        cur = new Vector2(cur.x, rel ? cur.y + Next() : Next());
                        p.LineTo(cur);
                        break;
                    case 'C':
                    {
                        var c1 = Off(new Vector2(Next(), Next()));
                        var c2 = Off(new Vector2(Next(), Next()));
                        var end = Off(new Vector2(Next(), Next()));
                        p.BezierCurveTo(c1, c2, end);
                        lastControl = c2;
                        cur = end;
                        break;
                    }
                    case 'S':
                    {
                        var c1 = "CcSs".IndexOf(lastCmd) >= 0 ? 2 * cur - lastControl : cur;
                        var c2 = Off(new Vector2(Next(), Next()));
                        var end = Off(new Vector2(Next(), Next()));
                        p.BezierCurveTo(c1, c2, end);
                        lastControl = c2;
                        cur = end;
                        break;
                    }
                    case 'Q':
                    {
                        var c = Off(new Vector2(Next(), Next()));
                        var end = Off(new Vector2(Next(), Next()));
                        p.QuadraticCurveTo(c, end);
                        lastControl = c;
                        cur = end;
                        break;
                    }
                    case 'A':
                    {
                        float rx = Next(), ry = Next(), rotation = Next();
                        bool large = Next() != 0, sweep = Next() != 0;
                        var end = Off(new Vector2(Next(), Next()));
                        ArcToBeziers(p, cur, end, rx, ry, rotation, large, sweep);
                        cur = end;
                        break;
                    }
                    case 'Z':
                        p.ClosePath();
                        cur = start;
                        break;
                    default:
                        i++;
                        break;
                }
                lastCmd = cmd;
            }
        }

        /// <summary>SVG elliptical arc to cubic Béziers (SVG 1.1 appendix F.6).</summary>
        static void ArcToBeziers(Painter2D p, Vector2 from, Vector2 to, float rx, float ry, float rotationDeg, bool large, bool sweep)
        {
            if (rx == 0 || ry == 0 || from == to)
            {
                p.LineTo(to);
                return;
            }
            rx = Mathf.Abs(rx);
            ry = Mathf.Abs(ry);
            float phi = rotationDeg * Mathf.Deg2Rad, cos = Mathf.Cos(phi), sin = Mathf.Sin(phi);
            var d = (from - to) / 2;
            float x1 = cos * d.x + sin * d.y, y1 = -sin * d.x + cos * d.y;
            float lambda = x1 * x1 / (rx * rx) + y1 * y1 / (ry * ry);
            if (lambda > 1)
            {
                rx *= Mathf.Sqrt(lambda);
                ry *= Mathf.Sqrt(lambda);
            }
            float num = rx * rx * ry * ry - rx * rx * y1 * y1 - ry * ry * x1 * x1;
            float den = rx * rx * y1 * y1 + ry * ry * x1 * x1;
            float coef = (large != sweep ? 1 : -1) * Mathf.Sqrt(Mathf.Max(0, num / den));
            float cx1 = coef * rx * y1 / ry, cy1 = -coef * ry * x1 / rx;
            var mid = (from + to) / 2;
            var center = new Vector2(cos * cx1 - sin * cy1 + mid.x, sin * cx1 + cos * cy1 + mid.y);

            float Angle(Vector2 u, Vector2 v)
            {
                float a = Mathf.Atan2(u.x * v.y - u.y * v.x, Vector2.Dot(u, v));
                return a;
            }
            var v1 = new Vector2((x1 - cx1) / rx, (y1 - cy1) / ry);
            var v2 = new Vector2((-x1 - cx1) / rx, (-y1 - cy1) / ry);
            float theta = Angle(Vector2.right, v1);
            float delta = Angle(v1, v2);
            if (!sweep && delta > 0) delta -= 2 * Mathf.PI;
            else if (sweep && delta < 0) delta += 2 * Mathf.PI;

            int segments = Mathf.CeilToInt(Mathf.Abs(delta) / (Mathf.PI / 2));
            float step = delta / segments;
            float k = 4f / 3f * Mathf.Tan(step / 4);
            Vector2 Point(float a) => center + new Vector2(cos * rx * Mathf.Cos(a) - sin * ry * Mathf.Sin(a), sin * rx * Mathf.Cos(a) + cos * ry * Mathf.Sin(a));
            Vector2 Deriv(float a) => new Vector2(-cos * rx * Mathf.Sin(a) - sin * ry * Mathf.Cos(a), -sin * rx * Mathf.Sin(a) + cos * ry * Mathf.Cos(a));
            for (int s = 0; s < segments; s++)
            {
                float a1 = theta + s * step, a2 = a1 + step;
                var p1 = Point(a1);
                var p2 = s == segments - 1 ? to : Point(a2);
                p.BezierCurveTo(p1 + k * Deriv(a1), p2 - k * Deriv(a2), p2);
            }
        }
    }
}
