using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public static class LetterSet
    {
        private static Dictionary<char, int> quantities;
        private static CharacterComparerIgnoreCase charIgnoreCaseComparer = new CharacterComparerIgnoreCase();

        public static IReadOnlyDictionary<char, int> Quantities
        {
            get
            {
                if (quantities == null)
                {
                    Dictionary<char, int> map = new Dictionary<char, int>(charIgnoreCaseComparer);
                    foreach (char letter in LetterInfo.BLANK + "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
                    {
                        map.Add(letter, GetQuantity(letter));
                    }

                    quantities = map;
                }

                return quantities;
            }
        }

        public static int GetQuantity(char letter)
        {
            switch (Char.ToUpper(letter))
            {
                case LetterInfo.BLANK:
                    return 2;
                case 'A':
                    return 9;
                case 'B':
                    return 2;
                case 'C':
                    return 2;
                case 'D':
                    return 5;
                case 'E':
                    return 13;
                case 'F':
                    return 2;
                case 'G':
                    return 3;
                case 'H':
                    return 4;
                case 'I':
                    return 8;
                case 'J':
                    return 1;
                case 'K':
                    return 1;
                case 'L':
                    return 4;
                case 'M':
                    return 2;
                case 'N':
                    return 5;
                case 'O':
                    return 8;
                case 'P':
                    return 2;
                case 'Q':
                    return 1;
                case 'R':
                    return 6;
                case 'S':
                    return 5;
                case 'T':
                    return 7;
                case 'U':
                    return 4;
                case 'V':
                    return 2;
                case 'W':
                    return 2;
                case 'X':
                    return 1;
                case 'Y':
                    return 2;
                case 'Z':
                    return 1;
                default:
                    throw new ArgumentException("what letter is this?!@#$!@# :: " + letter);
            }
        }

        public static IReadOnlyDictionary<char, int> GetRemaining(Board board, LetterInfo[] hand)
        {
            Dictionary<char, int> map = new Dictionary<char, int>(charIgnoreCaseComparer);

            foreach (char letter in Quantities.Keys)
            {
                map.Add(letter, GetQuantity(letter));
            }

            // subtract all letters in hand
            foreach (LetterInfo letter in hand)
            {
                map[letter.Letter] = map[letter.Letter] - 1;
            }

            // subtract all letters on board
            for (int y = 0; y < Board.Y_Cell_Count; y++)
            {
                for (int x = 0; x < Board.X_Cell_Count; x++)
                {
                    BoardCell cell = board.GetCell(x, y);
                    if (cell.HasLetter)
                    {
                        bool isBlank = cell.LetterPointValue == 0;
                        if (isBlank)
                        {
                            map[LetterInfo.BLANK] = map[LetterInfo.BLANK] - 1;
                        }
                        else
                        {
                            char letter = cell.Letter;
                            map[letter] = map[letter] - 1;
                        }
                    }
                }
            }

            return map;
        }

        private class CharacterComparerIgnoreCase : IEqualityComparer<char>
        {
            public bool Equals(char x, char y)
            {
                return Char.ToUpper(x).Equals(Char.ToUpper(y));
            }

            public int GetHashCode(char c)
            {
                return c.GetHashCode();
            }
        }
    }
}
