using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class BoardCell
    {
        public BoardCellTypes CellType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public LetterTile LetterTile { get; set; }

        public override string ToString()
        {
            return string.Format("[{0},{1}]{2}", this.X, this.Y, this.LetterTile != null ? " " + this.LetterTile.ToString() : "");
        }

        public bool HasLetter
        {
            get
            {
                return this.LetterTile != null;
            }
        }

        public bool IsTransient
        {
            get
            {
                return this.LetterTile != null && this.LetterTile.IsTransient;
            }
        }

        public BoardCell Clone()
        {
            return new BoardCell()
            {
                X = this.X,
                Y = this.Y,
                LetterTile = this.LetterTile != null ? this.LetterTile.Clone() : null,
                CellType = this.CellType
            };
        }
    }
}
