using System;
using System.Collections.Generic;
using System.Text;

namespace Keystage.Remote
{
    /// <summary>
    /// A small QR Code encoder (byte mode, error correction level M) used to show
    /// the phone pairing link. Follows ISO/IEC 18004; the structure mirrors
    /// Project Nayuki's public QR generator. Has no Unity dependencies.
    /// </summary>
    public sealed class QrCode
    {
        public readonly int Version;
        public readonly int Size;
        public readonly int Mask;
        readonly bool[,] modules;
        readonly bool[,] isFunction;

        public bool this[int x, int y] => modules[y, x];

        // Level M tables, indexed by version (index 0 unused).
        static readonly int[] EccPerBlock =
        {
            -1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26,
            26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
        };

        static readonly int[] NumBlocks =
        {
            -1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16,
            17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49,
        };

        const int LevelBitsM = 0; // format bits for level M

        /// <summary>Encode text as UTF-8 bytes. forceVersion/forceMask are for tests.</summary>
        public static QrCode Encode(string text, int forceVersion = 0, int forceMask = -1)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            int version = forceVersion;
            if (version == 0)
            {
                for (version = 1; version <= 40; version++)
                    if (DataBitsNeeded(data.Length, version) <= DataCodewords(version) * 8) break;
                if (version > 40) throw new ArgumentException("Text too long for a QR code");
            }

            // Bit stream: byte mode, character count, data, terminator, padding.
            var bits = new List<bool>();
            AppendBits(bits, 0x4, 4);
            AppendBits(bits, data.Length, version <= 9 ? 8 : 16);
            foreach (byte b in data) AppendBits(bits, b, 8);
            int capacity = DataCodewords(version) * 8;
            AppendBits(bits, 0, Math.Min(4, capacity - bits.Count));
            AppendBits(bits, 0, (8 - bits.Count % 8) % 8);
            for (int pad = 0xEC; bits.Count < capacity; pad ^= 0xEC ^ 0x11) AppendBits(bits, pad, 8);

            var codewords = new byte[bits.Count / 8];
            for (int i = 0; i < bits.Count; i++)
                if (bits[i]) codewords[i >> 3] |= (byte)(1 << (7 - (i & 7)));

            return new QrCode(version, AddEccAndInterleave(codewords, version), forceMask);
        }

        QrCode(int version, byte[] allCodewords, int forceMask)
        {
            Version = version;
            Size = version * 4 + 17;
            modules = new bool[Size, Size];
            isFunction = new bool[Size, Size];
            DrawFunctionPatterns();
            DrawCodewords(allCodewords);

            int mask = forceMask;
            if (mask < 0)
            {
                int best = int.MaxValue;
                for (int m = 0; m < 8; m++)
                {
                    ApplyMask(m);
                    DrawFormatBits(m);
                    int penalty = Penalty();
                    if (penalty < best)
                    {
                        best = penalty;
                        mask = m;
                    }
                    ApplyMask(m); // undo (XOR)
                }
            }
            Mask = mask;
            ApplyMask(mask);
            DrawFormatBits(mask);
        }

        static int DataBitsNeeded(int byteCount, int version) => 4 + (version <= 9 ? 8 : 16) + byteCount * 8;

        static int RawDataModules(int ver)
        {
            int result = (16 * ver + 128) * ver + 64;
            if (ver >= 2)
            {
                int numAlign = ver / 7 + 2;
                result -= (25 * numAlign - 10) * numAlign - 55;
                if (ver >= 7) result -= 36;
            }
            return result;
        }

        static int DataCodewords(int ver) => RawDataModules(ver) / 8 - EccPerBlock[ver] * NumBlocks[ver];

        static void AppendBits(List<bool> bits, int value, int length)
        {
            for (int i = length - 1; i >= 0; i--) bits.Add(((value >> i) & 1) != 0);
        }

        static byte[] AddEccAndInterleave(byte[] data, int ver)
        {
            int numBlocks = NumBlocks[ver];
            int eccLen = EccPerBlock[ver];
            int rawCodewords = RawDataModules(ver) / 8;
            int numShortBlocks = numBlocks - rawCodewords % numBlocks;
            int shortBlockLen = rawCodewords / numBlocks;

            var blocks = new List<byte[]>();
            byte[] divisor = ReedSolomonDivisor(eccLen);
            for (int i = 0, k = 0; i < numBlocks; i++)
            {
                int dataLen = shortBlockLen - eccLen + (i < numShortBlocks ? 0 : 1);
                var block = new byte[shortBlockLen + 1];
                Array.Copy(data, k, block, 0, dataLen);
                k += dataLen;
                var dat = new byte[dataLen];
                Array.Copy(data, k - dataLen, dat, 0, dataLen);
                byte[] ecc = ReedSolomonRemainder(dat, divisor);
                // Short blocks keep a dummy byte at the long-block data length, skipped below.
                int eccStart = shortBlockLen + 1 - eccLen;
                Array.Copy(ecc, 0, block, eccStart, eccLen);
                blocks.Add(block);
            }

            var result = new byte[rawCodewords];
            int n = 0;
            for (int i = 0; i < shortBlockLen + 1; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    if (i != shortBlockLen - eccLen || j >= numShortBlocks) result[n++] = blocks[j][i];
                }
            }
            return result;
        }

        static byte[] ReedSolomonDivisor(int degree)
        {
            var result = new byte[degree];
            result[degree - 1] = 1;
            int root = 1;
            for (int i = 0; i < degree; i++)
            {
                for (int j = 0; j < degree; j++)
                {
                    result[j] = Multiply(result[j], root);
                    if (j + 1 < degree) result[j] ^= result[j + 1];
                }
                root = Multiply(root, 0x02);
            }
            return result;
        }

        static byte[] ReedSolomonRemainder(byte[] data, byte[] divisor)
        {
            var result = new byte[divisor.Length];
            foreach (byte b in data)
            {
                int factor = b ^ result[0];
                Array.Copy(result, 1, result, 0, result.Length - 1);
                result[result.Length - 1] = 0;
                for (int i = 0; i < result.Length; i++) result[i] ^= Multiply(divisor[i], factor);
            }
            return result;
        }

        static byte Multiply(int x, int y)
        {
            int z = 0;
            for (int i = 7; i >= 0; i--)
            {
                z = (z << 1) ^ ((z >> 7) * 0x11D);
                z ^= ((y >> i) & 1) * x;
            }
            return (byte)z;
        }

        void SetFunction(int x, int y, bool dark)
        {
            modules[y, x] = dark;
            isFunction[y, x] = true;
        }

        void DrawFunctionPatterns()
        {
            for (int i = 0; i < Size; i++)
            {
                SetFunction(6, i, i % 2 == 0);
                SetFunction(i, 6, i % 2 == 0);
            }
            DrawFinder(3, 3);
            DrawFinder(Size - 4, 3);
            DrawFinder(3, Size - 4);

            int[] align = AlignmentPositions();
            int n = align.Length;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (!((i == 0 && j == 0) || (i == 0 && j == n - 1) || (i == n - 1 && j == 0)))
                        DrawAlignment(align[i], align[j]);

            DrawFormatBits(0); // reserve the area; real bits are drawn after masking
            DrawVersion();
        }

        int[] AlignmentPositions()
        {
            if (Version == 1) return new int[0];
            int numAlign = Version / 7 + 2;
            int step = Version == 32 ? 26 : (Version * 4 + numAlign * 2 + 1) / (numAlign * 2 - 2) * 2;
            var result = new int[numAlign];
            result[0] = 6;
            for (int i = numAlign - 1, pos = Size - 7; i >= 1; i--, pos -= step) result[i] = pos;
            return result;
        }

        void DrawFinder(int x, int y)
        {
            for (int dy = -4; dy <= 4; dy++)
            {
                for (int dx = -4; dx <= 4; dx++)
                {
                    int dist = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    int xx = x + dx, yy = y + dy;
                    if (xx >= 0 && xx < Size && yy >= 0 && yy < Size) SetFunction(xx, yy, dist != 2 && dist != 4);
                }
            }
        }

        void DrawAlignment(int x, int y)
        {
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                    SetFunction(x + dx, y + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
        }

        void DrawFormatBits(int mask)
        {
            int data = LevelBitsM << 3 | mask;
            int rem = data;
            for (int i = 0; i < 10; i++) rem = (rem << 1) ^ ((rem >> 9) * 0x537);
            int bits = (data << 10 | rem) ^ 0x5412;

            for (int i = 0; i <= 5; i++) SetFunction(8, i, Bit(bits, i));
            SetFunction(8, 7, Bit(bits, 6));
            SetFunction(8, 8, Bit(bits, 7));
            SetFunction(7, 8, Bit(bits, 8));
            for (int i = 9; i < 15; i++) SetFunction(14 - i, 8, Bit(bits, i));

            for (int i = 0; i < 8; i++) SetFunction(Size - 1 - i, 8, Bit(bits, i));
            for (int i = 8; i < 15; i++) SetFunction(8, Size - 15 + i, Bit(bits, i));
            SetFunction(8, Size - 8, true); // always-dark module
        }

        void DrawVersion()
        {
            if (Version < 7) return;
            int rem = Version;
            for (int i = 0; i < 12; i++) rem = (rem << 1) ^ ((rem >> 11) * 0x1F25);
            int bits = Version << 12 | rem;
            for (int i = 0; i < 18; i++)
            {
                bool bit = Bit(bits, i);
                int a = Size - 11 + i % 3, b = i / 3;
                SetFunction(a, b, bit);
                SetFunction(b, a, bit);
            }
        }

        void DrawCodewords(byte[] data)
        {
            int i = 0;
            for (int right = Size - 1; right >= 1; right -= 2)
            {
                if (right == 6) right = 5;
                for (int vert = 0; vert < Size; vert++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int x = right - j;
                        bool upward = ((right + 1) & 2) == 0;
                        int y = upward ? Size - 1 - vert : vert;
                        if (!isFunction[y, x] && i < data.Length * 8)
                        {
                            modules[y, x] = Bit(data[i >> 3], 7 - (i & 7));
                            i++;
                        }
                    }
                }
            }
        }

        void ApplyMask(int mask)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool invert;
                    switch (mask)
                    {
                        case 0: invert = (x + y) % 2 == 0; break;
                        case 1: invert = y % 2 == 0; break;
                        case 2: invert = x % 3 == 0; break;
                        case 3: invert = (x + y) % 3 == 0; break;
                        case 4: invert = (x / 3 + y / 2) % 2 == 0; break;
                        case 5: invert = x * y % 2 + x * y % 3 == 0; break;
                        case 6: invert = (x * y % 2 + x * y % 3) % 2 == 0; break;
                        default: invert = ((x + y) % 2 + x * y % 3) % 2 == 0; break;
                    }
                    modules[y, x] ^= invert && !isFunction[y, x];
                }
            }
        }

        /// <summary>The standard mask penalty: runs, 2x2 blocks, finder-like patterns, balance.</summary>
        int Penalty()
        {
            int result = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int a = 0; a < Size; a++)
                {
                    int run = 1;
                    for (int b = 1; b < Size; b++)
                    {
                        bool cur = pass == 0 ? modules[a, b] : modules[b, a];
                        bool prev = pass == 0 ? modules[a, b - 1] : modules[b - 1, a];
                        if (cur == prev) run++;
                        else
                        {
                            if (run >= 5) result += 3 + run - 5;
                            run = 1;
                        }
                    }
                    if (run >= 5) result += 3 + run - 5;

                    for (int b = 0; b + 10 < Size; b++)
                    {
                        bool Get(int k) => pass == 0 ? modules[a, b + k] : modules[b + k, a];
                        bool p1 = Get(0) && !Get(1) && Get(2) && Get(3) && Get(4) && !Get(5) && Get(6) && !Get(7) && !Get(8) && !Get(9) && !Get(10);
                        bool p2 = !Get(0) && !Get(1) && !Get(2) && !Get(3) && Get(4) && !Get(5) && Get(6) && Get(7) && Get(8) && !Get(9) && Get(10);
                        if (p1 || p2) result += 40;
                    }
                }
            }
            int dark = 0;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (modules[y, x]) dark++;
                    if (x < Size - 1 && y < Size - 1 && modules[y, x] == modules[y, x + 1] && modules[y, x] == modules[y + 1, x] && modules[y, x] == modules[y + 1, x + 1])
                        result += 3;
                }
            }
            int total = Size * Size;
            result += Math.Abs(dark * 20 - total * 10) / total * 10;
            return result;
        }

        static bool Bit(int value, int i) => ((value >> i) & 1) != 0;
    }
}
