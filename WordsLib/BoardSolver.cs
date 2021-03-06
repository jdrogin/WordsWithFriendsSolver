﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class BoardSolver
    {
        private int iterationsPerSolve = 0;
        private TimeSpan timePerSolve;
        private WordsLookup lookup;

        public BoardSolver(WordsLookup lookup)
        {
            this.lookup = lookup;
        }

        public List<Board> Solve(Board board, LetterInfo[] hand)
        {
            DateTime start = DateTime.Now;
            this.iterationsPerSolve = 0;
            List<BoardCell> startCells = this.GetStartCells(board);

            List<Board> savedBoards = new List<Board>();
            foreach (BoardCell startCell in startCells)
            {
                foreach (LetterInfo[] handWithoutBlanks in this.ExpandBlanks(hand))
                {
                    this.TryHandRecursive(savedBoards, board, handWithoutBlanks, startCell.X, startCell.Y, PlacementOrientation.Horizontal);
                    this.TryHandRecursive(savedBoards, board, handWithoutBlanks, startCell.X, startCell.Y, PlacementOrientation.Vertical);
                }
            }

            savedBoards = savedBoards.OrderByDescending(x => x.TransientScore.TotalScore).ToList();
            this.timePerSolve = DateTime.Now.Subtract(start);

            System.Diagnostics.Debug.WriteLine("$$$$$$ Board solver completed in " + this.timePerSolve.TotalSeconds.ToString("0.000") + " seconds"
                + " / with iterations: " + this.iterationsPerSolve);

            return savedBoards;
        }

        public int IterationsPerSolve
        {
            get
            {
                return this.iterationsPerSolve;
            }
        }

        public TimeSpan TimePerSolve
        {
            get
            {
                return this.timePerSolve;
            }
        }

        private List<LetterInfo[]> ExpandBlanks(LetterInfo[] hand)
        {
            if (!hand.Any(x => x.Letter == LetterInfo.BLANK))
            {
                // no blanks
                return new List<LetterInfo[]>() { hand };
            }

            List<LetterInfo[]> allHands = new List<LetterInfo[]>();
            int blankCount = hand.Count(x => x.Letter == LetterInfo.BLANK);
            LetterInfo[] handWithoutBlanks = hand.Where(x => x.Letter != LetterInfo.BLANK).ToArray();

            // initialize all hands with the letters in the hand without the blanks.
            allHands.Add(hand.Where(x => x.Letter != LetterInfo.BLANK).ToArray());

            for (int blankIndex = 0; blankIndex < blankCount; blankIndex++)
            {
                foreach (LetterInfo[] handToAddTo in allHands.ToList())
                {
                    allHands.Remove(handToAddTo);
                    foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
                    {
                        List<LetterInfo> newHand = handToAddTo.ToList();
                        newHand.Add(new LetterInfo(c, 0, true));
                        allHands.Add(newHand.ToArray());
                    }
                }
            }

            return allHands;
        }

        private void TryHandRecursive(List<Board> savedBoards, Board board, LetterInfo[] hand, int startX, int startY, PlacementOrientation placementOrientation)
        {
            if (hand.Length == 0)
            {
                return;
            }

            this.iterationsPerSolve++;

            if (placementOrientation == PlacementOrientation.Horizontal)
            {
                foreach (LetterInfo letterInHand in hand)
                {
                    // go left
                    int left = board.GetNextAvailableCellToLeft(startX, startY);
                    if (left != -1)
                    {
                        Board cloneBoard = board.CloneTransient();
                        cloneBoard.SetLetter(left, startY, letterInHand);

                        if (this.IsPlayedTileScorable(cloneBoard, left, startY))
                        {
                            // word created, save board
                            this.AddNewBoardIfDistinct(savedBoards, cloneBoard);
                        }

                        if (this.IsPlayedTileValid(cloneBoard, left, startY, placementOrientation))
                        {
                            // continue recursion
                            LetterInfo[] remainingHand = this.GetNewHandWithoutTile(hand, letterInHand);
                            this.TryHandRecursive(savedBoards, cloneBoard, remainingHand, startX, startY, placementOrientation);
                        }
                    }

                    // at the start of the loop we can skip go right as this would be a duplicate of go left
                    if (board.GetCell(startX, startY).HasLetter)
                    {
                        // go right
                        int right = board.GetNextAvailableCellToRight(startX, startY);
                        if (right != -1)
                        {
                            Board cloneBoard = board.CloneTransient();
                            cloneBoard.SetLetter(right, startY, letterInHand);

                            if (this.IsPlayedTileScorable(cloneBoard, right, startY))
                            {
                                // word created, save board
                                this.AddNewBoardIfDistinct(savedBoards, cloneBoard);
                            }

                            if (this.IsPlayedTileValid(cloneBoard, right, startY, placementOrientation))
                            {
                                // continue recursion
                                LetterInfo[] remainingHand = this.GetNewHandWithoutTile(hand, letterInHand);
                                this.TryHandRecursive(savedBoards, cloneBoard, remainingHand, startX, startY, placementOrientation);
                            }
                        }
                    }
                }
            }

            if (placementOrientation == PlacementOrientation.Vertical)
            {
                foreach (LetterInfo letterInHand in hand)
                {
                    // go up
                    int top = board.GetNextAvailableCellAbove(startX, startY);
                    if (top != -1)
                    {
                        Board cloneBoard = board.CloneTransient();
                        cloneBoard.SetLetter(startX, top, letterInHand);

                        if (this.IsPlayedTileScorable(cloneBoard, startX, top))
                        {
                            // word created, save board
                            this.AddNewBoardIfDistinct(savedBoards, cloneBoard);
                        }

                        if (this.IsPlayedTileValid(cloneBoard, startX, top, placementOrientation))
                        {
                            // continue recursion
                            LetterInfo[] remainingHand = this.GetNewHandWithoutTile(hand, letterInHand);
                            this.TryHandRecursive(savedBoards, cloneBoard, remainingHand, startX, startY, placementOrientation);
                        }
                    }

                    // at the start of the loop we can skip go down as this would be a duplicate of go up
                    if (board.GetCell(startX, startY).HasLetter)
                    {
                        // go down
                        int bottom = board.GetNextAvailableCellBelow(startX, startY);
                        if (bottom != -1)
                        {
                            Board cloneBoard = board.CloneTransient();
                            cloneBoard.SetLetter(startX, bottom, letterInHand);

                            if (this.IsPlayedTileScorable(cloneBoard, startX, bottom))
                            {
                                // word created, save board
                                this.AddNewBoardIfDistinct(savedBoards, cloneBoard);
                            }

                            if (this.IsPlayedTileValid(cloneBoard, startX, bottom, placementOrientation))
                            {
                                // continue recursion
                                LetterInfo[] remainingHand = this.GetNewHandWithoutTile(hand, letterInHand);
                                this.TryHandRecursive(savedBoards, cloneBoard, remainingHand, startX, startY, placementOrientation);
                            }
                        }
                    }
                }
            }
        }

        public bool IsPlayedTileValid(Board board, int playedX, int playedY, PlacementOrientation placementOrientation)
        {
            string verticalSegment = board.GetVerticalSegment(playedX, playedY);
            if (verticalSegment.Length > 1 && !this.lookup.IsWordOrSegment(verticalSegment))
            {
                return false;
            }

            // vertical is valid, check new horizontal
            string horizontalSegment = board.GetHorizontalSegment(playedX, playedY);
            if (horizontalSegment.Length > 1 && !this.lookup.IsWordOrSegment(horizontalSegment))
            {
                return false;
            }

            if (placementOrientation == PlacementOrientation.Vertical && horizontalSegment.Length > 1 && !this.lookup.IsWord(horizontalSegment))
            {
                // require horizontal to be a word
                return false;
            }

            if (placementOrientation == PlacementOrientation.Horizontal && verticalSegment.Length > 1 && !this.lookup.IsWord(verticalSegment))
            {
                // require vertical to be a word
                return false;
            }

            // valid
            return true;
        }

        public bool IsPlayedTileScorable(Board board, int playedX, int playedY)
        {
            string verticalSegment = board.GetVerticalSegment(playedX, playedY);
            if (verticalSegment.Length > 1 && !this.lookup.IsWord(verticalSegment))
            {
                return false;
            }

            // vertical is valid, check new horizontal
            string horizontalSegment = board.GetHorizontalSegment(playedX, playedY);
            if (horizontalSegment.Length > 1 && !this.lookup.IsWord(horizontalSegment))
            {
                return false;
            }

            // valid
            return true;
        }

        private void AddNewBoardIfDistinct(List<Board> boards, Board newBoard)
        {
            if (!boards.Any(x => x.BoardString.Equals(newBoard.BoardString)))
            {
                boards.Add(newBoard);
            }
        }

        private LetterInfo[] GetNewHandWithoutTile(LetterInfo[] hand, LetterInfo tileToRemove)
        {
            LetterInfo[] newHand = new LetterInfo[hand.Length - 1];

            int newHandIndex = 0;
            for (int i = 0; i < hand.Length; i++)
            {
                if (hand[i] != tileToRemove)
                {
                    newHand[newHandIndex] = hand[i];
                    newHandIndex++;
                }
            }

            return newHand;
        }

        /// <summary>
        /// Get a list of all empty cells which have a letter cell above, below, right or left.
        /// These are the cell from which we can play the start of a hand.
        /// </summary>
        private List<BoardCell> GetStartCells(Board board)
        {
            List<BoardCell> startCells = new List<BoardCell>();

            for (int x = 0; x < Board.X_Cell_Count; x++)
            {
                for (int y = 0; y < Board.Y_Cell_Count; y++)
                {
                    BoardCell checkCell = board.GetCell(x, y);
                    if (!checkCell.HasLetter && this.IsNextToCellWithLetter(board, x, y))
                    {
                        startCells.Add(new BoardCell() { X = x, Y = y });
                    }
                }
            }

            return startCells;
        }

        private bool IsNextToCellWithLetter(Board board, int x, int y)
        {
            // true if cell to right, left, above or below has a letter, false otherwise
            return (x > 0 && board.GetCell(x - 1, y).HasLetter)
                || (x + 1 < Board.X_Cell_Count && board.GetCell(x + 1, y).HasLetter)
                || (y > 0 && board.GetCell(x, y - 1).HasLetter)
                || (y + 1 < Board.Y_Cell_Count && board.GetCell(x, y + 1).HasLetter);
        }
    }

    public enum PlacementOrientation
    {
        None = 0,
        Vertical,
        Horizontal,
    }
}
