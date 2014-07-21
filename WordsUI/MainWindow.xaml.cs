using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WordsLib;
using System.Linq;

namespace WordsUI
{
    public partial class MainWindow : Window
    {
        private WordsLookup wordLookup;
        private BoardSolver boardSolver;
        private List<Board> solvedBoards;
        private int solvedBoardSelectedIndex = 0;
        private int iterationsPerSolve = 0;
        private TimeSpan timePerSolve;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.wordLookup = new WordsLookup();
            this.wordLookup.Init();
        }

        void Init(string boardImgSource)
        {            
            this.BoardImage.Source = new BitmapImage(new Uri(boardImgSource));
            
            Board board = BoardOCR.OCR(boardImgSource);
            LetterInfo[] hand = BoardOCR.HandOCR(boardImgSource);

            this.boardSolver = new BoardSolver(this.wordLookup);
            this.HandLetters.SetLetters(new string(hand.Select(x => x.Letter).ToArray()));

            IReadOnlyDictionary<char, int> remainingLetters = LetterSet.GetRemaining(board, hand);

            this.solvedBoards = this.boardSolver.Solve(board, hand);
            this.iterationsPerSolve = this.boardSolver.IterationsPerSolve;
            this.timePerSolve = this.boardSolver.TimePerSolve;
            this.solvedBoardSelectedIndex = 0;

            this.SetSolvedBoardsList(this.solvedBoards);
            this.SetSelectedSolvedBoard();
        }

        void SetSelectedSolvedBoard()
        {
            if (this.solvedBoards.Count == 0)
            {
                this.StatusText.Text = string.Format("No solved boards");
                return;
            }

            Board board = this.solvedBoards[this.solvedBoardSelectedIndex];
            this.StatusText.Text = string.Format("Showing: {0}/{1}, score: {2}", this.solvedBoardSelectedIndex + 1, this.solvedBoards.Count, board.TransientScore.TotalScore);
            this.StatusText.Text += string.Format("{0}Iterations: {1}", Environment.NewLine, string.Format("{0:n0}", this.iterationsPerSolve));
            this.StatusText.Text += string.Format("{0}Solve Time: {1} seconds", Environment.NewLine, this.timePerSolve.TotalSeconds.ToString("0.000"));

            this.BoardGrid.Children.Clear();

            for (int y = 0; y < Board.Y_Cell_Count; y++)
            {
                for (int x = 0; x < Board.X_Cell_Count; x++)
                {
                    BoardCell boardCell = board.GetCell(x, y);

                    TileCell cell = new TileCell(boardCell.CellType);
                    cell.X = x;
                    cell.Y = y;

                    cell.SetValue(Grid.RowProperty, y);
                    cell.SetValue(Grid.ColumnProperty, x);
                    this.BoardGrid.Children.Add(cell);

                    // add letter if exists
                    if (boardCell.HasLetter)
                    {
                        TileCell tile = new TileCell(new LetterInfo(boardCell.Letter, boardCell.LetterPointValue, boardCell.LetterIsTransient));
                        tile.X = boardCell.X;
                        tile.Y = boardCell.Y;

                        tile.SetValue(Grid.RowProperty, boardCell.Y);
                        tile.SetValue(Grid.ColumnProperty, boardCell.X);
                        this.BoardGrid.Children.Add(tile);
                    }
                }
            }
        }

        void SetSolvedBoardsList(List<Board> boards)
        {
            this.SolvedList.Items.Clear();
            for (int index = 0; index < boards.Count; index++)
            {
                TransientScore score = boards[index].TransientScore;
                string bonusIndicator = score.IncludesBonus ? "+" : string.Empty;
                string text = string.Format("({0}{1}) - {2}/{3}", score.TotalScore, bonusIndicator, score.BestWord.Word, score.BestWord.Score);
                this.SolvedList.Items.Add(new TextBlock() { Text = text, Tag = index });
            }
        }

        private void SolvedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox box = sender as ListBox;
            TextBlock block = box.SelectedItem as TextBlock;

            if (block != null)
            {
                this.solvedBoardSelectedIndex = (int)block.Tag;
                this.SetSelectedSolvedBoard();
            }
        }

        private void ChooseBoardButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                this.Init(filename);
            }
        }
    }
}
