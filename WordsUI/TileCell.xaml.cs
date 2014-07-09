using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WordsLib;

namespace WordsUI
{
    public partial class TileCell : UserControl
    {
        public int X { get; set; }
        public int Y { get; set; }

        public TileCell()
        {
            InitializeComponent();
        }

        public TileCell(LetterInfo letterTile)
        {
            InitializeComponent();

            this.Opacity = 0.85;
            this.Rect.Fill = new SolidColorBrush() { Color = Color.FromRgb(237, 183, 66) };
            this.Text.Foreground = Brushes.Black;
            this.Text.Text = letterTile.Letter.ToString().ToUpper();
            this.Points.Text = letterTile.PointValue > 0 ? letterTile.PointValue.ToString() : "";

            if (letterTile.IsTransient)
            {
                this.Text.Foreground = Brushes.White;
                this.Rect.Stroke = Brushes.DarkRed;
                this.Rect.StrokeThickness = 2;
            }
        }

        public TileCell(BoardCellTypes cellType)
        {
            InitializeComponent();

            this.Text.FontSize = 18;

            switch (cellType)
            {
                case BoardCellTypes.None:
                    this.Text.Text = "";
                    this.Points.Text = "";
                    break;

                case BoardCellTypes.Start:
                    this.Rect.Fill = new SolidColorBrush() { Color = Color.FromRgb(250, 160, 120) };
                    this.Text.Text = "";
                    this.Points.Text = "";
                    break;

                case BoardCellTypes.TripleWord:
                    this.Rect.Fill = new SolidColorBrush() { Color = Color.FromRgb(224, 140, 40) };
                    this.Text.Text = "TW";
                    this.Points.Text = "";
                    break;

                case BoardCellTypes.TripleLetter:
                    this.Rect.Fill = new SolidColorBrush() { Color = Color.FromRgb(80, 156, 72) };
                    this.Text.Text = "TL";
                    this.Points.Text = "";
                    break;

                case BoardCellTypes.DoubleWord:
                    this.Rect.Fill = new SolidColorBrush() { Color = Color.FromRgb(192, 88, 80) };
                    this.Text.Text = "DW";
                    this.Points.Text = "";
                    break;

                case BoardCellTypes.DoubleLetter:
                    this.Rect.Fill = new SolidColorBrush() { Color = Color.FromRgb(0, 128, 192) };
                    this.Text.Text = "DL";
                    this.Points.Text = "";
                    break;
            }
        }
    }
}
