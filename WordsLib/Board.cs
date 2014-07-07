using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class Board
    {
        public const int X_Cell_Count = 15;
        public const int Y_Cell_Count = 15;

        private string boardString = null;
        private BoardCell[][] cells;

        public Board()
        {
            this.InitBlankBoard();
        }

        private Board(bool clone)
        {
            // no init
        }

        public void InitBlankBoard()
        {
            this.cells = new BoardCell[X_Cell_Count][];
            for (int x = 0; x < X_Cell_Count; x++)
            {
                this.cells[x] = new BoardCell[Y_Cell_Count];
                for (int y = 0; y < Y_Cell_Count; y++)
                {
                    this.cells[x][y] = new BoardCell()
                    {
                        X = x,
                        Y = y,
                        LetterTile = null,
                        CellType = this.GetBoardCellType(x, y)
                    };
                }
            }
        }

        public Board Clone()
        {
            Board newBoard = new Board(true); // no init

            newBoard.cells = new BoardCell[X_Cell_Count][];
            for (int x = 0; x < X_Cell_Count; x++)
            {
                newBoard.cells[x] = new BoardCell[Y_Cell_Count];
                for (int y = 0; y < Y_Cell_Count; y++)
                {
                    newBoard.cells[x][y] = this.cells[x][y].Clone();
                }
            }

            return newBoard;
        }

        public void ClearLetter(int x, int y)
        {
            this.GetCell(x, y).LetterTile = null;
        }

        public void SetLetter(int x, int y, LetterTile letterTile)
        {
            BoardCell cell = this.GetCell(x, y);
            cell.LetterTile = letterTile;
            
            // reset board string
            this.boardString = null;
        }

        public BoardCell GetCell(int x, int y)
        {
            return this.cells[x][y];
        }

        public int LetterCount
        {
            get
            {
                int count = 0;
                for (int x = 0; x < X_Cell_Count; x++)
                {
                    for (int y = 0; y < Y_Cell_Count; y++)
                    {
                        if (this.cells[x][y].HasLetter)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }

        public string BoardVisual
        {
            get
            {
                StringBuilder visual = new StringBuilder(2048);
                for (int y = 0; y < Y_Cell_Count; y++)
                {
                    for (int x = 0; x < X_Cell_Count; x++)
                    {
                        visual.Append("- ");
                    }
                    visual.Append(Environment.NewLine);

                    for (int x = 0; x < X_Cell_Count; x++)
                    {
                        visual.Append("|");
                        BoardCell cell = this.cells[x][y];
                        if (cell.HasLetter)
                        {
                            visual.Append(cell.LetterTile.Letter);
                        }
                        else
                        {
                            visual.Append(" ");
                        }
                    }

                    visual.Append(Environment.NewLine);
                }

                return visual.ToString();
            }
        }

        /// <summary>
        /// Gets a comparable string representing the board.  Blanks are '#', letters are upper case, blanks are lower case.
        /// </summary>
        public string BoardString
        {
            get
            {
                if (this.boardString == null)
                {
                    StringBuilder visual = new StringBuilder(512);
                    for (int y = 0; y < Y_Cell_Count; y++)
                    {
                        for (int x = 0; x < X_Cell_Count; x++)
                        {
                            BoardCell cell = this.cells[x][y];
                            char letter = cell.HasLetter ? cell.LetterTile.Letter : '#';
                            if (cell.LetterTile != null && cell.LetterTile.PointValue == 0)
                            {
                                char.ToLower(letter);
                            }
                            else
                            {
                                char.ToUpper(letter);
                            }

                            visual.Append(letter);
                        }
                    }

                    this.boardString = visual.ToString();
                }

                return this.boardString;
            }
        }

        public List<BoardCell> GetTransientCells()
        {
            List<BoardCell> transientCells = new List<BoardCell>();
            for (int y = 0; y < Y_Cell_Count; y++)
            {
                for (int x = 0; x < X_Cell_Count; x++)
                {
                    BoardCell cell = this.cells[x][y];
                    if (cell.IsTransient)
                    {
                        transientCells.Add(cell);
                    }
                }
            }

            return transientCells;
        }

        public TransientScore TransientScore
        {
            get
            {
                List<BoardCell> transientCells = this.GetTransientCells();
                BoardCell cell0 = transientCells[0];
                bool isHorizontal = transientCells.All(x => x.Y == cell0.Y);

                TransientScore transientScore = new TransientScore();
                Action<int, int, int, int> addWord = (int startX, int startY, int endX, int endY) =>
                {
                    UnitScore unit = this.GetUnitScoreOrNull(startX, startY, endX, endY);
                    if (unit != null)
                    {
                        transientScore.AddWord(unit);
                    }
                };

                if (isHorizontal)
                {
                    // count horizontal word
                    int left = this.GetLeftWordBoundary(cell0.X, cell0.Y);
                    int right = this.GetRightWordBoundary(cell0.X, cell0.Y);
                    addWord(left, cell0.Y, right, cell0.Y);

                    // count all vertical words
                    foreach (BoardCell transientCell in transientCells)
                    {
                        int top = this.GetTopWordBoundary(transientCell.X, transientCell.Y);
                        int bottom = this.GetBottomWordBoundary(transientCell.X, transientCell.Y);
                        addWord(transientCell.X, top, transientCell.X, bottom);
                    }
                }
                else
                {
                    // count vertical word
                    int top = this.GetTopWordBoundary(cell0.X, cell0.Y);
                    int bottom = this.GetBottomWordBoundary(cell0.X, cell0.Y);
                    addWord(cell0.X, top, cell0.X, bottom);

                    // count all horizontal words
                    foreach (BoardCell transientCell in transientCells)
                    {
                        int left = this.GetLeftWordBoundary(transientCell.X, transientCell.Y);
                        int right = this.GetRightWordBoundary(transientCell.X, transientCell.Y);
                        addWord(left, transientCell.Y, right, transientCell.Y);
                    }
                }

                return transientScore;
            }
        }

        public int GetLeftWordBoundary(int startX, int startY)
        {
            int leftX = startX;
            while (leftX > 0 && this.GetCell(leftX - 1, startY).HasLetter)
            {
                leftX--;
            }

            return leftX;
        }

        public int GetRightWordBoundary(int startX, int startY)
        {
            int rightX = startX;
            while (rightX + 1 < Board.X_Cell_Count && this.GetCell(rightX + 1, startY).HasLetter)
            {
                rightX++;
            }

            return rightX;
        }

        public int GetTopWordBoundary(int startX, int startY)
        {
            int topY = startY;
            while (topY > 0 && this.GetCell(startX, topY - 1).HasLetter)
            {
                topY--;
            }

            return topY;
        }

        public int GetBottomWordBoundary(int startX, int startY)
        {
            int bottomY = startY;
            while (bottomY + 1 < Board.Y_Cell_Count && this.GetCell(startX, bottomY + 1).HasLetter)
            {
                bottomY++;
            }

            return bottomY;
        }

        public int GetNextAvailableCellToLeft(int startX, int startY)
        {
            int leftX = startX;
            while (leftX > 0 && this.GetCell(leftX, startY).HasLetter)
            {
                leftX--;
            }

            if (this.GetCell(leftX, startY).HasLetter)
            {
                // nothing available to left
                return -1;
            }
            else
            {
                return leftX;
            }
        }

        public int GetNextAvailableCellToRight(int startX, int startY)
        {
            int rightX = startX;
            while (rightX + 1 < Board.X_Cell_Count && this.GetCell(rightX, startY).HasLetter)
            {
                rightX++;
            }

            if (this.GetCell(rightX, startY).HasLetter)
            {
                // nothing available to right
                return -1;
            }
            else
            {
                return rightX;
            }
        }

        public int GetNextAvailableCellAbove(int startX, int startY)
        {
            int topY = startY;
            while (topY > 0 && this.GetCell(startX, topY).HasLetter)
            {
                topY--;
            }

            if (this.GetCell(startX, topY).HasLetter)
            {
                // nothing available above
                return -1;
            }
            else
            {
                return topY;
            }
        }

        public int GetNextAvailableCellBelow(int startX, int startY)
        {
            int bottomY = startY;
            while (bottomY + 1 < Board.Y_Cell_Count && this.GetCell(startX, bottomY).HasLetter)
            {
                bottomY++;
            }

            if (this.GetCell(startX, bottomY).HasLetter)
            {
                // nothing available above
                return -1;
            }
            else
            {
                return bottomY;
            }
        }

        public string GetVerticalSegment(int startX, int startY)
        {
            // move up to top of segment
            int currY = startY;
            while (currY > 0 && this.GetCell(startX, currY - 1).HasLetter)
            {
                currY--;
            }

            string segment = "";

            // record segment from top to bottom
            while (currY < Board.Y_Cell_Count && this.GetCell(startX, currY).HasLetter)
            {
                segment += this.GetCell(startX, currY).LetterTile.Letter;
                currY++;
            }

            return segment;
        }

        public string GetHorizontalSegment(int startX, int startY)
        {
            // move left to end of segment
            int currX = startX;
            while (currX > 0 && this.GetCell(currX - 1, startY).HasLetter)
            {
                currX--;
            }

            string segment = "";

            // record segment from left to right
            while (currX < Board.Y_Cell_Count && this.GetCell(currX, startY).HasLetter)
            {
                segment += this.GetCell(currX, startY).LetterTile.Letter;
                currX++;
            }

            return segment;
        }

        private UnitScore GetUnitScoreOrNull(int leftX, int topY, int rightX, int bottomY)
        {
            if (leftX == rightX && topY == bottomY)
            {
                // single tile, no score
                return null;
            }

            UnitScore unitScore = new UnitScore()
            {
                StartX = leftX,
                EndX = rightX,
                StartY = topY,
                EndY = bottomY,
                Word = string.Empty
            };

            bool isHorizontal = topY == bottomY;
            int wordMultiplier = 1;
            int wordScore = 0;
            int transientLetterCount = 0;
            BoardCell[] cells = null;

            if (isHorizontal)
            {
                cells = new BoardCell[rightX - leftX + 1];
                for(int x = leftX; x <= rightX; x++)
                {
                    cells[x - leftX] = this.GetCell(x, topY);
                }
            }
            else
            {
                // vertical
                cells = new BoardCell[bottomY - topY + 1];
                for (int y = topY; y <= bottomY; y++)
                {
                    cells[y - topY] = this.GetCell(leftX, y);
                }
            }

            // calculate score from cells array
            foreach (BoardCell cell in cells)
            {
                unitScore.Word += cell.LetterTile.Letter;
                wordScore += cell.LetterTile.PointValue * this.GetLetterMultiplier(cell);
                wordMultiplier *= this.GetWordMultiplier(cell);
                if (cell.IsTransient)
                {
                    transientLetterCount++;
                }
            }

            unitScore.Score = wordScore * wordMultiplier;
            if (transientLetterCount == 7)
            {
                unitScore.Score += UnitScore.SEVEN_LETTER_BONUS_VALUE;
                unitScore.IncludesBonus = true;
            }

            return unitScore;
        }

        private BoardCellTypes GetBoardCellType(int rowIndex, int colIndex)
        {
            if (rowIndex == 7 && colIndex == 7)
            {
                return BoardCellTypes.Start;
            }
            else if ((rowIndex == 0 && colIndex == 3)
                || (rowIndex == 0 && colIndex == 11)
                || (rowIndex == 3 && colIndex == 0)
                || (rowIndex == 3 && colIndex == 14)
                || (rowIndex == 11 && colIndex == 0)
                || (rowIndex == 11 && colIndex == 14)
                || (rowIndex == 14 && colIndex == 3)
                || (rowIndex == 14 && colIndex == 11))
            {
                return BoardCellTypes.TripleWord;
            }
            else if ((rowIndex == 0 && colIndex == 6)
                || (rowIndex == 0 && colIndex == 8)
                || (rowIndex == 3 && colIndex == 3)
                || (rowIndex == 3 && colIndex == 11)
                || (rowIndex == 5 && colIndex == 5)
                || (rowIndex == 5 && colIndex == 9)
                || (rowIndex == 6 && colIndex == 0)
                || (rowIndex == 6 && colIndex == 14)
                || (rowIndex == 8 && colIndex == 0)
                || (rowIndex == 8 && colIndex == 14)
                || (rowIndex == 9 && colIndex == 5)
                || (rowIndex == 9 && colIndex == 9)
                || (rowIndex == 11 && colIndex == 3)
                || (rowIndex == 11 && colIndex == 11)
                || (rowIndex == 14 && colIndex == 6)
                || (rowIndex == 14 && colIndex == 8))
            {
                return BoardCellTypes.TripleLetter;
            }
            else if ((rowIndex == 1 && colIndex == 5)
                || (rowIndex == 1 && colIndex == 9)
                || (rowIndex == 3 && colIndex == 7)
                || (rowIndex == 3 && colIndex == 11)
                || (rowIndex == 5 && colIndex == 1)
                || (rowIndex == 5 && colIndex == 13)
                || (rowIndex == 7 && colIndex == 3)
                || (rowIndex == 7 && colIndex == 11)
                || (rowIndex == 9 && colIndex == 1)
                || (rowIndex == 9 && colIndex == 13)
                || (rowIndex == 11 && colIndex == 7)
                || (rowIndex == 13 && colIndex == 5)
                || (rowIndex == 13 && colIndex == 9))
            {
                return BoardCellTypes.DoubleWord;
            }
            else if ((rowIndex == 1 && colIndex == 2)
                || (rowIndex == 1 && colIndex == 12)
                || (rowIndex == 2 && colIndex == 1)
                || (rowIndex == 2 && colIndex == 4)
                || (rowIndex == 2 && colIndex == 10)
                || (rowIndex == 2 && colIndex == 13)
                || (rowIndex == 4 && colIndex == 2)
                || (rowIndex == 4 && colIndex == 6)
                || (rowIndex == 4 && colIndex == 8)
                || (rowIndex == 4 && colIndex == 12)
                || (rowIndex == 6 && colIndex == 4)
                || (rowIndex == 6 && colIndex == 10)
                || (rowIndex == 8 && colIndex == 4)
                || (rowIndex == 8 && colIndex == 10)
                || (rowIndex == 10 && colIndex == 2)
                || (rowIndex == 10 && colIndex == 6)
                || (rowIndex == 10 && colIndex == 8)
                || (rowIndex == 10 && colIndex == 12)
                || (rowIndex == 12 && colIndex == 1)
                || (rowIndex == 12 && colIndex == 4)
                || (rowIndex == 12 && colIndex == 10)
                || (rowIndex == 12 && colIndex == 13)
                || (rowIndex == 13 && colIndex == 2)
                || (rowIndex == 13 && colIndex == 12))
            {
                return BoardCellTypes.DoubleLetter;
            }
            else
            {
                return BoardCellTypes.None;
            }
        }

        private int GetLetterMultiplier(BoardCell cell)
        {
            if (cell.IsTransient && cell.CellType == BoardCellTypes.DoubleLetter)
            {
                return 2;
            }
            else if (cell.IsTransient && cell.CellType == BoardCellTypes.TripleLetter)
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }

        private int GetWordMultiplier(BoardCell cell)
        {
            if (cell.IsTransient && cell.CellType == BoardCellTypes.DoubleWord)
            {
                return 2;
            }
            else if (cell.IsTransient && cell.CellType == BoardCellTypes.TripleWord)
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }
    }
}
