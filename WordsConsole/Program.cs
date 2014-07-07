using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordsLib;

namespace WordsConsole
{
    class Program
    {
        static string LINE_OF_STARS = "***********************************************************************************************";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(LINE_OF_STARS);
                Console.WriteLine("Start");
                Console.WriteLine(LINE_OF_STARS + Environment.NewLine + Environment.NewLine);

                // run
                //LetterMapGenerator.CreateBoardLettersMap();
                LetterMapGenerator.CreateHandLettersMap();

                //WordsLookup dict = new WordsLookup();
                //dict.Init();

                //foreach (string file in Directory.GetFiles(@"C:\projects\WordsWithFriendsSolver.git\trunk\WordsLib\boards", "Screenshot_2014-07-07-14-52-34.png"))
                //{
                //    BoardOCR.HandOCR(file);
                //}
            }
            catch (Exception ex)
            {
                ConsoleColor currColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Environment.NewLine + Environment.NewLine + LINE_OF_STARS);
                Console.WriteLine(ex.ToString());
                Console.WriteLine(LINE_OF_STARS + Environment.NewLine + Environment.NewLine);
                Console.ForegroundColor = currColor;
            }

            Console.WriteLine(Environment.NewLine + Environment.NewLine + LINE_OF_STARS);
            Console.WriteLine("Done.  Press any key to exit..");
            Console.ReadKey();
        }

    }
}
