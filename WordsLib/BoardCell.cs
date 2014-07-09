using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class BoardCell
    {
        public const char NO_LETTER = '#';

        public BoardCellTypes CellType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public char Letter { get; private set; }
        public int LetterPointValue { get; private set; }
        public bool LetterIsTransient { get; private set; }

        public BoardCell()
        {
            this.Letter = NO_LETTER;
            this.LetterPointValue = 0;
            this.LetterIsTransient = false;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]{2}", this.X, this.Y, this.Letter != NO_LETTER ? " " + this.Letter + "/" + this.LetterPointValue : "");
        }

        public bool HasLetter
        {
            get
            {
                return this.Letter != NO_LETTER;
            }
        }

        public bool IsTransient
        {
            get
            {
                return this.Letter != NO_LETTER && this.LetterIsTransient;
            }
        }

        public void SetLetter(char letter, int pointValue, bool isTransient)
        {
            this.Letter = letter;
            this.LetterPointValue = pointValue;
            this.LetterIsTransient = isTransient;
        }

        public BoardCell Clone()
        {
            return new BoardCell()
            {
                X = this.X,
                Y = this.Y,
                Letter = this.Letter,
                LetterPointValue = this.LetterPointValue,
                LetterIsTransient = this.LetterIsTransient,
                CellType = this.CellType
            };
        }
    }
}
