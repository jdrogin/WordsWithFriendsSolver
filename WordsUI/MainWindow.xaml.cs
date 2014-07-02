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

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.wordLookup = new WordsLookup();
            this.wordLookup.Init();

            //string boardImgSource = System.IO.Path.Combine(Environment.CurrentDirectory, "boards/Screenshot_2014-06-30-09-46-04.png");
            //this.Init(boardImgSource);
        }

        void Init(string boardImgSource)
        {
            
            BoardOCR boardOcr = new BoardOCR();
            this.BoardImage.Source = new BitmapImage(new Uri(boardImgSource));
            Board board = boardOcr.OCR(boardImgSource);
            LetterTile[] hand = boardOcr.HandOCR(boardImgSource);

            this.boardSolver = new BoardSolver(this.wordLookup);

            //LetterTile[] hand = new LetterTile[] { new LetterTile('I', 1, true),
            //    new LetterTile('Z', 10, true),
            //    new LetterTile('I', 1, true),
            //    new LetterTile('N', 2, true),
            //    new LetterTile('E', 1, true),
            //    new LetterTile('G', 3, true),
            //    new LetterTile('H', 3, true) };

            ////this.HandLetters.SetLetters("IZINEGH");
            this.HandLetters.SetLetters(new string(hand.Select(x => x.Letter).ToArray()));

            this.solvedBoards = this.boardSolver.Solve(board, hand);

            this.SetSolvedBoardsList(this.solvedBoards);
            this.SetSelectedSolvedBoard();
        }

        void SetSelectedSolvedBoard()
        {
            Board board = this.solvedBoards[this.solvedBoardSelectedIndex];
            this.StatusText.Text = string.Format("Showing: {0}/{1}, score: {2}", this.solvedBoardSelectedIndex + 1, this.solvedBoards.Count, board.TransientScore.TotalScore);

            this.BoardGrid.Children.Clear();

            for (int rowIndex = 0; rowIndex < Board.X_Cell_Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < Board.Y_Cell_Count; colIndex++)
                {
                    BoardCell boardCell = board.GetCell(colIndex, rowIndex);

                    TileCell cell = new TileCell(boardCell.CellType);
                    cell.X = rowIndex;
                    cell.Y = colIndex;

                    cell.SetValue(Grid.RowProperty, rowIndex);
                    cell.SetValue(Grid.ColumnProperty, colIndex);
                    this.BoardGrid.Children.Add(cell);

                    // add letter if exists
                    if (boardCell.HasLetter)
                    {
                        TileCell tile = new TileCell(boardCell.LetterTile);
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
                string text = string.Format("({0}) - {1}/{2}", score.TotalScore, score.BestWord.Word, score.BestWord.Score);
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
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".png";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                this.Init(filename);
            }
        }
    }
}
