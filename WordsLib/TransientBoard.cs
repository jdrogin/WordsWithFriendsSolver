using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class TransientBoard
    {
        private const int UNSET = -1;

        private int X;
        private int Y;
        private BoardCell SingleUsedCell;
        private BoardCell[] UsedCells;

        public TransientBoard()
        {
            this.X = UNSET;
            this.Y = UNSET;
        }

        public void SetLetterTile(int x, int y, LetterInfo letterInfo)
        {
            if (this.X == UNSET && this.Y == UNSET)
            {
                // first tile - use single letter variable as we don't know if this is a vertical or horizontal placement yet.
                this.X = x;
                this.Y = y;
                this.SingleUsedCell = Board.GetNewBoardCell(x, y, letterInfo);
            }
            else if (this.X != UNSET && this.Y != UNSET)
            {
                // second tile added, need to check for vertical or horizontal
                if (x == this.X && this.Y == y)
                {
                    // update to existing single tile
                    this.SingleUsedCell = Board.GetNewBoardCell(x, y, letterInfo);
                }
                else if (x == this.X)
                {
                    // same x - second tile is a vertical placement.  Move single tile to array.
                    this.UsedCells = new BoardCell[Board.Y_Cell_Count];
                    this.UsedCells[this.Y] = this.SingleUsedCell;
                    this.SingleUsedCell = null;
                    this.Y = UNSET;
                    this.UsedCells[y] = Board.GetNewBoardCell(x, y, letterInfo);
                }
                else if (y == this.Y)
                {
                    // same y - second tile is a horizontal placement.  Move single tile to array.
                    this.UsedCells = new BoardCell[Board.X_Cell_Count];
                    this.UsedCells[this.X] = this.SingleUsedCell;
                    this.SingleUsedCell = null;
                    this.X = UNSET;
                    this.UsedCells[x] = Board.GetNewBoardCell(x, y, letterInfo);
                }
                else
                {
                    throw new ArgumentException("Transient tiles must be in a single row or column.");
                }
            }
            else if (this.X == x && this.Y == UNSET)
            {
                // adding along existing vertical
                this.UsedCells[y] = Board.GetNewBoardCell(x, y, letterInfo);
            }
            else if (this.Y == y && this.X == UNSET)
            {
                // adding along existing horizontal
                this.UsedCells[x] = Board.GetNewBoardCell(x, y, letterInfo);
            }
            else
            {
                throw new ArgumentException("Transient tiles must be in a single row or column.");
            }
        }

        public BoardCell GetBoardCellOrNull(int x, int y)
        {
            if (x == this.X && y == this.Y)
            {
                return this.SingleUsedCell;
            }
            if (this.UsedCells != null && x == this.X)
            {
                return this.UsedCells[y];
            }
            else if (this.UsedCells != null && y == this.Y)
            {
                return this.UsedCells[x];
            }
            else
            {
                return null;
            }
        }

        public TransientBoard Clone()
        {
            TransientBoard newBoard = new TransientBoard();
            newBoard.X = this.X;
            newBoard.Y = this.Y;
            newBoard.SingleUsedCell = this.SingleUsedCell;
            if (this.UsedCells != null)
            {
                newBoard.UsedCells = new BoardCell[this.UsedCells.Length];
                for (int i = 0; i < this.UsedCells.Length; i++)
                {
                    newBoard.UsedCells[i] = this.UsedCells[i];
                }
            }

            return newBoard;
        }
    }
}
