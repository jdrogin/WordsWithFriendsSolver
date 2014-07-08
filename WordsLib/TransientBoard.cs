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
        private LetterTile SingleLetterTile;
        private LetterTile[] LetterTiles;

        public TransientBoard()
        {
            this.X = UNSET;
            this.Y = UNSET;
        }

        public void SetLetterTile(int x, int y, LetterTile tile)
        {
            if (this.X == UNSET && this.Y == UNSET)
            {
                // first tile - use single letter variable as we don't know if this is a vertical or horizontal placement yet.
                this.X = x;
                this.Y = y;
                this.SingleLetterTile = tile;
            }
            else if (this.X != UNSET && this.Y != UNSET)
            {
                // second tile added, need to check for vertical or horizontal
                if (x == this.X && this.Y == y)
                {
                    // update to existing single tile
                    this.SingleLetterTile = tile;
                }
                else if (x == this.X)
                {
                    // same x - second tile is a vertical placement.  Move single tile to array.
                    this.LetterTiles = new LetterTile[Board.Y_Cell_Count];
                    this.LetterTiles[this.Y] = this.SingleLetterTile;
                    this.SingleLetterTile = null;
                    this.Y = UNSET;
                    this.LetterTiles[y] = tile;
                }
                else if (y == this.Y)
                {
                    // same y - second tile is a horizontal placement.  Move single tile to array.
                    this.LetterTiles = new LetterTile[Board.X_Cell_Count];
                    this.LetterTiles[this.X] = this.SingleLetterTile;
                    this.SingleLetterTile = null;
                    this.X = UNSET;
                    this.LetterTiles[x] = tile;
                }
                else
                {
                    throw new ArgumentException("Transient tiles must be in a single row or column.");
                }
            }
            else if (this.X == x && this.Y == UNSET)
            {
                // adding along existing vertical
                this.LetterTiles[y] = tile;
            }
            else if (this.Y == y && this.X == UNSET)
            {
                // adding along existing horizontal
                this.LetterTiles[x] = tile;
            }
            else
            {
                throw new ArgumentException("Transient tiles must be in a single row or column.");
            }
        }

        public LetterTile GetTileOrNull(int x, int y)
        {
            if (x == this.X && y == this.Y)
            {
                return this.SingleLetterTile;
            }
            if (this.LetterTiles != null && x == this.X)
            {
                return this.LetterTiles[y];
            }
            else if (this.LetterTiles != null && y == this.Y)
            {
                return this.LetterTiles[x];
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
            newBoard.SingleLetterTile = this.SingleLetterTile;
            if (this.LetterTiles != null)
            {
                newBoard.LetterTiles = new LetterTile[this.LetterTiles.Length];
                for (int i = 0; i < this.LetterTiles.Length; i++)
                {
                    newBoard.LetterTiles[i] = this.LetterTiles[i];
                }
            }

            return newBoard;
        }
    }
}
