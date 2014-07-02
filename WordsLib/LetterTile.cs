using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class LetterTile
    {
        public const char BLANK = '?';
        public char Letter { get; set; }
        public int PointValue { get; set; }
        public bool IsTransient { get; set; }

        public LetterTile(char letter, int pointValue, bool isTransient)
        {
            this.Letter = letter;
            this.PointValue = pointValue;
            this.IsTransient = isTransient;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", this.Letter, this.PointValue);
        }

        public LetterTile Clone()
        {
            return new LetterTile(this.Letter, this.PointValue, this.IsTransient);
        }
    }
}
