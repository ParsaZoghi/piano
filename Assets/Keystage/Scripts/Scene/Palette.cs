using UnityEngine;

namespace Keystage.Scene
{
    /// <summary>
    /// Scene colours per theme (port of three/palette.js). The frame is a warm
    /// walnut in the dark theme so it stands apart from the night-blue
    /// background, and charcoal in the light one.
    /// </summary>
    public class Palette
    {
        public Color background, floor, frame, felt, road, lane, edge, beat, bar, outline, sky, ground;
        public float ambient, rim;

        public static readonly Palette Dark = new Palette
        {
            background = Hex(0x0b0b1c),
            floor = Hex(0x07071a),
            frame = Hex(0x8a5a36),
            felt = Hex(0x7f1d1d),
            road = Hex(0x15152e),
            lane = Hex(0x0b0b1c),
            edge = Hex(0x3b3a8f),
            beat = Hex(0x4338ca),
            bar = Hex(0x818cf8),
            outline = Hex(0x05060f),
            sky = Hex(0xc7d2fe),
            ground = Hex(0x12122a),
            ambient = 0.45f,
            rim = 3f,
        };

        public static readonly Palette Light = new Palette
        {
            background = Hex(0xe6eaf5),
            floor = Hex(0xc9d0e3),
            frame = Hex(0x2b2f45),
            felt = Hex(0x9f1239),
            road = Hex(0xc7cfe6),
            lane = Hex(0xa9b3d3),
            edge = Hex(0x6f7bb0),
            beat = Hex(0x6366f1),
            bar = Hex(0x312e81),
            outline = Hex(0x0f172a),
            sky = Hex(0xffffff),
            ground = Hex(0xb8c0dc),
            ambient = 0.6f,
            rim = 1f,
        };

        public static Palette Current { get; private set; } = Dark;

        public static void Set(string theme) => Current = theme == "light" ? Light : Dark;

        public static Color Hex(int rgb) => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);
    }
}
