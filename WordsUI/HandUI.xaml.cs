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
    /// <summary>
    /// Interaction logic for HandUI.xaml
    /// </summary>
    public partial class HandUI : UserControl
    {
        private List<LetterTile> LetterTiles;

        public HandUI()
        {
            InitializeComponent();
            this.LetterTiles = new List<LetterTile>();
        }

        public void SetLetters(string letters)
        {
            this.EntryBox.Visibility = System.Windows.Visibility.Collapsed;
            this.Letters.Visibility = System.Windows.Visibility.Visible;

            this.Letters.Children.Clear();
            foreach (char letter in letters)
            {
                LetterTile letterTile = new LetterTile(letter, 0, true);
                if (letter == LetterTile.BLANK)
                {
                    letterTile.Letter = ' ';
                }
                else
                {
                    letterTile.PointValue = LetterOCR.GetLetterPointValue(letter);
                }

                TileCell cell = new TileCell(letterTile);
                this.Letters.Children.Add(cell);
            }
        }
    }
}
