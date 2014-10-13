using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public static class LetterMapGenerator
    {
        public static void CreateBoardLettersMap()
        {
            List<LetterOCR> letterMaps = new List<LetterOCR>();

            foreach (string file in Directory.GetFiles(@"letters/board/V2"))
            {
                Console.WriteLine();
                string letter = Path.GetFileNameWithoutExtension(file);
                Console.Write(letter);

                LetterOCR letterMap = new LetterOCR() { Letter = letter[0] };
                letterMaps.Add(letterMap);

                using (MagickImage image = new MagickImage(file))
                {
                    foreach (Pixel pixel in image.GetReadOnlyPixels())
                    {
                        if (IsLetterPixel(pixel) && PixelIsSurroundedBySameColor(image, pixel, 0.10))
                        {
                            if ((pixel.X > 27 && pixel.Y > 28)

                                && !(pixel.X == 28 && pixel.Y == 29)
                                && !(pixel.X == 29 && pixel.Y == 29)
                                && !(pixel.X == 30 && pixel.Y == 29)
                                && !(pixel.X == 31 && pixel.Y == 29)
                                && !(pixel.X == 33 && pixel.Y == 29)
                                && !(pixel.X == 34 && pixel.Y == 29)

                                && !(pixel.X == 28 && pixel.Y == 30)
                                && !(pixel.X == 29 && pixel.Y == 30)
                                && !(pixel.X == 30 && pixel.Y == 30)
                                && !(pixel.X == 31 && pixel.Y == 30)
                                && !(pixel.X == 32 && pixel.Y == 30)

                                && !(pixel.X == 28 && pixel.Y == 31)
                                && !(pixel.X == 29 && pixel.Y == 31)
                                && !(pixel.X == 30 && pixel.Y == 31)
                                && !(pixel.X == 31 && pixel.Y == 31)

                                && !(pixel.X == 28 && pixel.Y == 32)
                                && !(pixel.X == 29 && pixel.Y == 32)
                                && !(pixel.X == 30 && pixel.Y == 32)

                                && !(pixel.X == 28 && pixel.Y == 33)
                                && !(pixel.X == 29 && pixel.Y == 33)

                                && !(pixel.X == 28 && pixel.Y == 34))
                            {
                                // lower right corner may contain word score which is not part of the letter map
                                continue;
                            }

                            //if (letterMap.ContainsPoint(pixel.X - 1, pixel.Y - 1)
                            //    || letterMap.ContainsPoint(pixel.X, pixel.Y - 1)
                            //    || letterMap.ContainsPoint(pixel.X + 1, pixel.Y - 1)
                            //    || letterMap.ContainsPoint(pixel.X + 1, pixel.Y)
                            //    || letterMap.ContainsPoint(pixel.X + 1, pixel.Y + 1)
                            //    || letterMap.ContainsPoint(pixel.X, pixel.Y + 1)
                            //    || letterMap.ContainsPoint(pixel.X - 1, pixel.Y + 1)
                            //    || letterMap.ContainsPoint(pixel.X - 1, pixel.Y))
                            //{
                            //    // adjacent point already collected, skip
                            //    continue;
                            //}

                            Console.Write(string.Format(" {0},{1}", pixel.X, pixel.Y));
                            letterMap.AddXYPoint(pixel.X, pixel.Y);
                        }
                    }
                }

                // Print!!!!
                PrintLetterShapeToConsole(letterMap);
            }

            PrintLetterMapsSwitchStatementToConsole(letterMaps, 512);
        }

        public static void CreateHandLettersMap()
        {
            List<LetterOCR> letterMaps = new List<LetterOCR>();

            foreach (string file in Directory.GetFiles(@"letters/hand/V2"))
            {
                Console.WriteLine();
                string letter = Path.GetFileNameWithoutExtension(file);
                Console.Write(letter);

                LetterOCR letterMap = new LetterOCR() { Letter = letter[0] };
                letterMaps.Add(letterMap);

                using (MagickImage image = new MagickImage(file))
                {
                    foreach (Pixel pixel in image.GetReadOnlyPixels())
                    {
                        if (pixel.Y < 30)
                        {
                            // skip top of image where number is located, not needed for recognition
                            continue;
                        }

                        if (IsLetterPixel(pixel) && PixelIsSurroundedBySameColor(image, pixel, 2, 0.05))
                        {
                            Console.Write(string.Format(" {0},{1}", pixel.X, pixel.Y));
                            letterMap.AddXYPoint(pixel.X, pixel.Y);
                        }
                    }
                }

                // show letter shape
                PrintLetterShapeToConsole(letterMap);
            }

            PrintLetterMapsSwitchStatementToConsole(letterMaps, 256);
        }

        static bool PixelIsSurroundedBySameColor(MagickImage image, Pixel pixel, int radius, double percentTolerance)
        {
            if (pixel.X - radius < 0
                || pixel.X + radius >= image.Width
                || pixel.Y - radius < 0
                || pixel.Y + radius >= image.Height)
            {
                return false;
            }

            float valueTolerance = Convert.ToInt32(257 * percentTolerance);
            int red = Convert.ToInt32(pixel.GetChannel(0) / 257);
            int green = Convert.ToInt32(pixel.GetChannel(1) / 257);
            int blue = Convert.ToInt32(pixel.GetChannel(2) / 257);

            foreach (Pixel pixelToCompare in image.GetReadOnlyPixels(pixel.X - radius, pixel.Y - radius, radius * 2 + 1, radius * 2 + 1))
            {
                int redToCompare = Convert.ToInt32(pixelToCompare.GetChannel(0) / 257);
                int greenToCompare = Convert.ToInt32(pixelToCompare.GetChannel(1) / 257);
                int blueToCompare = Convert.ToInt32(pixelToCompare.GetChannel(2) / 257);

                if (Math.Abs(red - redToCompare) > valueTolerance
                    || Math.Abs(green - greenToCompare) > valueTolerance
                    || Math.Abs(blue - blueToCompare) > valueTolerance)
                {
                    return false;
                }
            }

            // matches
            return true;
        }

        static bool PixelIsSurroundedBySameColor(MagickImage image, Pixel pixel, double percentTolerance)
        {
            if (PixelsAreSameColor(pixel, GetPixelToLeftOrNull(image, pixel), percentTolerance)
                && PixelsAreSameColor(pixel, GetPixelToBottomLeftOrNull(image, pixel), percentTolerance)
                && PixelsAreSameColor(pixel, GetPixelToTopLeftOrNull(image, pixel), percentTolerance)

                && PixelsAreSameColor(pixel, GetPixelToRightOrNull(image, pixel), percentTolerance)
                && PixelsAreSameColor(pixel, GetPixelToBottomRightOrNull(image, pixel), percentTolerance)
                && PixelsAreSameColor(pixel, GetPixelToTopRightOrNull(image, pixel), percentTolerance)

                && PixelsAreSameColor(pixel, GetPixelAboveOrNull(image, pixel), percentTolerance)
                && PixelsAreSameColor(pixel, GetPixelBelowOrNull(image, pixel), percentTolerance))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool PixelsAreSameColor(Pixel pixel1, Pixel pixel2, double percentTolerance)
        {
            if (pixel1 != null && pixel2 != null)
            {
                int r1 = Convert.ToInt32(pixel1.GetChannel(0) / 257);
                int g1 = Convert.ToInt32(pixel1.GetChannel(1) / 257);
                int b1 = Convert.ToInt32(pixel1.GetChannel(2) / 257);
                int r2 = Convert.ToInt32(pixel2.GetChannel(0) / 257);
                int g2 = Convert.ToInt32(pixel2.GetChannel(1) / 257);
                int b2 = Convert.ToInt32(pixel2.GetChannel(2) / 257);

                int bitTolerance = Convert.ToInt32(256.0 * percentTolerance);

                return Math.Abs(r1 - r2) < bitTolerance
                    && Math.Abs(g1 - g2) < bitTolerance
                    && Math.Abs(b1 - b2) < bitTolerance;
            }
            else
            {
                return false;
            }
        }

        static Pixel GetPixelToLeftOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.X > 0)
            {
                return image.GetReadOnlyPixels()[pixel.X - 1, pixel.Y];
            }
            else
            {
                return null;
            }
        }

        static Pixel GetPixelToTopLeftOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.X > 0 && pixel.Y > 0)
            {
                return image.GetReadOnlyPixels()[pixel.X - 1, pixel.Y - 1];
            }
            else
            {
                return null;
            }
        }

        static Pixel GetPixelToBottomLeftOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.X > 0 && pixel.Y + 1 < image.Width)
            {
                return image.GetReadOnlyPixels()[pixel.X - 1, pixel.Y + 1];
            }
            else
            {
                return null;
            }
        }

        static Pixel GetPixelToRightOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.X + 1 < image.Width)
            {
                return image.GetReadOnlyPixels()[pixel.X + 1, pixel.Y];
            }
            else
            {
                return null;
            }
        }

        static Pixel GetPixelToTopRightOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.X + 1 < image.Width && pixel.Y > 0)
            {
                return image.GetReadOnlyPixels()[pixel.X + 1, pixel.Y - 1];
            }
            else
            {
                return null;
            }
        }

        static Pixel GetPixelToBottomRightOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.X + 1 < image.Width && pixel.Y + 1 < image.Width)
            {
                return image.GetReadOnlyPixels()[pixel.X + 1, pixel.Y + 1];
            }
            else
            {
                return null;
            }
        }

        static Pixel GetPixelAboveOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.Y > 0)
            {
                return image.GetReadOnlyPixels()[pixel.X, pixel.Y - 1];
            }
            else
            {
                return null;
            }
        }

        static Pixel GetPixelBelowOrNull(MagickImage image, Pixel pixel)
        {
            if (pixel.Y + 1 < image.Height)
            {
                return image.GetReadOnlyPixels()[pixel.X, pixel.Y + 1];
            }
            else
            {
                return null;
            }
        }

        static bool IsLetterPixel(Pixel pixel)
        {
            // RGBA:
            int red = Convert.ToInt32(pixel.GetChannel(0) / 257);
            int green = Convert.ToInt32(pixel.GetChannel(1) / 257);
            int blue = Convert.ToInt32(pixel.GetChannel(2) / 257);
            //int alpha = Convert.ToInt32(pixel.GetChannel(3) / 257);

            bool isWhite = (red == 255 && green == 255 && blue == 255);
            bool isLetterColor = (red == 76 && green == 25 && blue == 0);
            bool isMatch = isWhite || isLetterColor;

            return isMatch;
        }

        static void PrintLetterShapeToConsole(LetterOCR letterMap)
        {
            int previousY = 0;
            int previousX = 0;
            foreach (IGrouping<int, LetterOCRPoint> line in letterMap.Points.GroupBy(x => x.y).OrderBy(x => x.Key))
            {
                foreach (LetterOCRPoint point in line.OrderBy(x => x.x))
                {
                    int yDelta = point.y - previousY;
                    for (int i = 0; i < yDelta; i++)
                    {
                        Console.WriteLine();
                        previousX = 0;
                    }

                    int xDelta = point.x - previousX;
                    for (int i = 0; i < xDelta - 1; i++)
                    {
                        Console.Write(" ");
                    }

                    Console.Write("*");
                    previousX = point.x;
                    previousY = point.y;

                }
            }
        }

        static void PrintLetterMapsSwitchStatementToConsole(List<LetterOCR> letterMaps, int maxPoints)
        {
            Console.WriteLine();
            Console.WriteLine();

            // order maps so that least used points are first
            Dictionary<string, int> pointToCountMap = new Dictionary<string, int>();
            foreach (LetterOCR letterMap in letterMaps)
            {
                foreach (LetterOCRPoint pt in letterMap.Points)
                {
                    string key = string.Format("{0}/{1}", pt.x, pt.y);
                    if (pointToCountMap.ContainsKey(key))
                    {
                        pointToCountMap[key] = pointToCountMap[key] + 1;
                    }
                    else
                    {
                        pointToCountMap[key] = 1;
                    }
                }
            }

            foreach (LetterOCR letterMap in letterMaps)
            {
                letterMap.Points = letterMap.Points.OrderBy(pt => pointToCountMap[string.Format("{0}/{1}", pt.x, pt.y)]).ToArray();
                letterMap.Points = letterMap.Points.Take(maxPoints).ToArray();

                PrintLetterShapeToConsole(letterMap);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            foreach (LetterOCR letterMap in letterMaps)
            {
                Console.WriteLine("case " + "'" + letterMap.Letter.ToString().ToUpper() + "':");
                Console.WriteLine("\t" + "str = " + "\"" + string.Join(" ", letterMap.Points.Select(x => x.x + "," + x.y)) + "\";");
                Console.WriteLine("\t" + "break;");

                Console.WriteLine();
            }
        }
    }
}
