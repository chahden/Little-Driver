using System.Collections.Generic;
using System.Text;

namespace LittleDriver.UI
{
    /// <summary>
    /// Turns Arabic as typed (separate letters, read right to left) into what TextMeshPro can draw:
    /// each letter replaced by its joined form (start / middle / end / alone), lam-alef ligatures,
    /// and the line reversed so it reads right to left. Harakat stay on their letter.
    /// The font must contain the Arabic Presentation Forms-B block (Lalezar does).
    /// </summary>
    public static class ArabicShaper
    {
        private enum Joining { None, Right, Dual }

        private struct Letter
        {
            public readonly char Isolated;
            public readonly Joining Joining;
            public Letter(char isolated, Joining joining) { Isolated = isolated; Joining = joining; }
        }

        private static readonly Dictionary<char, Letter> letters = BuildTable();

        private static Dictionary<char, Letter> BuildTable()
        {
            var t = new Dictionary<char, Letter>
            {
                ['ء'] = new Letter('ﺀ', Joining.None),  // ء
                ['آ'] = new Letter('ﺁ', Joining.Right), // آ
                ['أ'] = new Letter('ﺃ', Joining.Right), // أ
                ['ؤ'] = new Letter('ﺅ', Joining.Right), // ؤ
                ['إ'] = new Letter('ﺇ', Joining.Right), // إ
                ['ئ'] = new Letter('ﺉ', Joining.Dual),  // ئ
                ['ا'] = new Letter('ﺍ', Joining.Right), // ا
                ['ب'] = new Letter('ﺏ', Joining.Dual),  // ب
                ['ة'] = new Letter('ﺓ', Joining.Right), // ة
                ['ت'] = new Letter('ﺕ', Joining.Dual),  // ت
                ['ث'] = new Letter('ﺙ', Joining.Dual),  // ث
                ['ج'] = new Letter('ﺝ', Joining.Dual),  // ج
                ['ح'] = new Letter('ﺡ', Joining.Dual),  // ح
                ['خ'] = new Letter('ﺥ', Joining.Dual),  // خ
                ['د'] = new Letter('ﺩ', Joining.Right), // د
                ['ذ'] = new Letter('ﺫ', Joining.Right), // ذ
                ['ر'] = new Letter('ﺭ', Joining.Right), // ر
                ['ز'] = new Letter('ﺯ', Joining.Right), // ز
                ['س'] = new Letter('ﺱ', Joining.Dual),  // س
                ['ش'] = new Letter('ﺵ', Joining.Dual),  // ش
                ['ص'] = new Letter('ﺹ', Joining.Dual),  // ص
                ['ض'] = new Letter('ﺽ', Joining.Dual),  // ض
                ['ط'] = new Letter('ﻁ', Joining.Dual),  // ط
                ['ظ'] = new Letter('ﻅ', Joining.Dual),  // ظ
                ['ع'] = new Letter('ﻉ', Joining.Dual),  // ع
                ['غ'] = new Letter('ﻍ', Joining.Dual),  // غ
                ['ف'] = new Letter('ﻑ', Joining.Dual),  // ف
                ['ق'] = new Letter('ﻕ', Joining.Dual),  // ق
                ['ك'] = new Letter('ﻙ', Joining.Dual),  // ك
                ['ل'] = new Letter('ﻝ', Joining.Dual),  // ل
                ['م'] = new Letter('ﻡ', Joining.Dual),  // م
                ['ن'] = new Letter('ﻥ', Joining.Dual),  // ن
                ['ه'] = new Letter('ﻩ', Joining.Dual),  // ه
                ['و'] = new Letter('ﻭ', Joining.Right), // و
                ['ى'] = new Letter('ﻯ', Joining.Right), // ى
                ['ي'] = new Letter('ﻱ', Joining.Dual),  // ي
            };
            return t;
        }

        // Lam followed by these alefs becomes one ligature glyph (isolated form; +1 = final form).
        private static readonly Dictionary<char, char> lamAlef = new Dictionary<char, char>
        {
            ['آ'] = 'ﻵ', ['أ'] = 'ﻷ', ['إ'] = 'ﻹ', ['ا'] = 'ﻻ',
        };

        private const char Lam = 'ل';
        private const char Tatweel = 'ـ';

        /// <summary>Harakat and other marks that sit on a letter and do not break joining.</summary>
        public static bool IsMark(char c) => (c >= 'ً' && c <= 'ٟ') || c == 'ٰ' || (c >= 'ۖ' && c <= 'ۭ');

        private static bool IsArabic(char c) => (c >= '؀' && c <= 'ۿ') || (c >= 'ﭐ' && c <= '﷿') || (c >= 'ﹰ' && c <= '﻿');

        private static Joining JoiningOf(char c)
        {
            if (c == Tatweel) return Joining.Dual;
            return letters.TryGetValue(c, out Letter l) ? l.Joining : Joining.None;
        }

        /// <summary>
        /// Shapes every line of <paramref name="logical"/> and lays it out right to left.
        /// <paramref name="advanceEm"/> gives a glyph's width in em; with it, harakat are centred on their
        /// letter using TextMeshPro &lt;space&gt; tags (rich text must be on). Without it they are placed approximately.
        /// </summary>
        public static string Shape(string logical, System.Func<char, float> advanceEm = null)
        {
            if (string.IsNullOrEmpty(logical)) return logical ?? string.Empty;
            string[] lines = logical.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++) lines[i] = Reverse(Join(lines[i]), advanceEm);
            return string.Join("\n", lines);
        }

        // Step 1: pick the joined form of each letter (still in typing order).
        private static string Join(string s)
        {
            var output = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!letters.ContainsKey(c))
                {
                    output.Append(c);
                    continue;
                }

                int prev = PreviousLetter(s, i);
                bool joinsBefore = prev >= 0 && JoiningOf(s[prev]) == Joining.Dual && !IsLamAlefAlef(s, prev);

                if (c == Lam)
                {
                    int next = NextLetter(s, i);
                    if (next >= 0 && lamAlef.TryGetValue(s[next], out char ligature))
                    {
                        output.Append((char)(ligature + (joinsBefore ? 1 : 0)));
                        for (int m = i + 1; m < next; m++) output.Append(s[m]); // marks on the lam
                        i = next;                                               // the alef is now part of the ligature
                        continue;
                    }
                }

                Letter letter = letters[c];
                int after = NextLetter(s, i);
                bool joinsAfter = letter.Joining == Joining.Dual && after >= 0 && JoiningOf(s[after]) != Joining.None;
                int offset = letter.Joining == Joining.None ? 0
                    : joinsBefore && joinsAfter ? 3
                    : joinsBefore ? 1
                    : joinsAfter ? 2
                    : 0;
                // A letter standing alone keeps its normal code: fonts such as Lalezar draw it the same
                // and do not always include the separate "isolated" presentation form.
                output.Append(offset == 0 ? c : (char)(letter.Isolated + offset));
            }
            return output.ToString();
        }

        // An alef that was merged into a lam-alef ligature does not join to the next letter.
        private static bool IsLamAlefAlef(string s, int index)
        {
            if (!lamAlef.ContainsKey(s[index])) return false;
            int before = PreviousLetter(s, index);
            return before >= 0 && s[before] == Lam;
        }

        private static int PreviousLetter(string s, int i)
        {
            for (int k = i - 1; k >= 0; k--)
            {
                if (IsMark(s[k])) continue;
                return JoiningOf(s[k]) != Joining.None || letters.ContainsKey(s[k]) ? k : -1;
            }
            return -1;
        }

        private static int NextLetter(string s, int i)
        {
            for (int k = i + 1; k < s.Length; k++)
            {
                if (IsMark(s[k])) continue;
                return JoiningOf(s[k]) != Joining.None || letters.ContainsKey(s[k]) ? k : -1;
            }
            return -1;
        }

        // Step 2: lay the line out right to left. Each letter keeps its harakat after it,
        // and runs of Latin letters or digits keep their own left-to-right order.
        private static string Reverse(string shaped, System.Func<char, float> advanceEm)
        {
            var clusters = new List<string>();
            int i = 0;
            while (i < shaped.Length)
            {
                int start = i;
                if (IsLtr(shaped[i]))
                {
                    while (i < shaped.Length && (IsLtr(shaped[i]) || (shaped[i] == ' ' && i + 1 < shaped.Length && IsLtr(shaped[i + 1])))) i++;
                }
                else
                {
                    i++;
                    while (i < shaped.Length && IsMark(shaped[i])) i++;
                    if (i - start > 1)
                    {
                        // Marks have no width and are drawn centred on the pen position. Move the pen to the
                        // middle of their letter, draw the marks, move back, then draw the letter itself.
                        string cluster = shaped.Substring(start, i - start);
                        char letter = cluster[0];
                        string marks = cluster.Substring(1);
                        float half = advanceEm != null ? advanceEm(letter) * 0.5f : 0f;
                        clusters.Add(half > 0f
                            ? Space(half) + marks + Space(-half) + letter
                            : marks + letter);
                        continue;
                    }
                }
                clusters.Add(Mirror(shaped.Substring(start, i - start)));
            }
            clusters.Reverse();
            return string.Concat(clusters);
        }

        private static string Space(float em) =>
            "<space=" + em.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "em>";

        private static bool IsLtr(char c) => char.IsLetterOrDigit(c) && !IsArabic(c);

        private static string Mirror(string cluster)
        {
            if (cluster.Length != 1) return cluster;
            switch (cluster[0])
            {
                case '(': return ")";
                case ')': return "(";
                case '[': return "]";
                case ']': return "[";
                case '<': return ">";
                case '>': return "<";
                default: return cluster;
            }
        }
    }
}
