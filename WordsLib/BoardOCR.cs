using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;

namespace WordsLib
{
    public static class BoardOCR
    {
        public static LetterInfo[] HandOCR(string screenImg)
        {
            DateTime start = DateTime.Now;

            Func<Pixel, bool> isBluePixel = (Pixel pixel) =>
            {
                int r = Convert.ToInt32(pixel.GetChannel(0) / 257);
                int g = Convert.ToInt32(pixel.GetChannel(1) / 257);
                int b = Convert.ToInt32(pixel.GetChannel(2) / 257);
                int tolerance = 50;

                return Math.Abs(r - 52) <= tolerance && Math.Abs(g - 95) <= tolerance && Math.Abs(b - 148) <= tolerance;
            };

            Func<Pixel, bool> isOrangePixel = (Pixel pixel) =>
            {
                int r = Convert.ToInt32(pixel.GetChannel(0) / 257);
                int g = Convert.ToInt32(pixel.GetChannel(1) / 257);
                int b = Convert.ToInt32(pixel.GetChannel(2) / 257);
                int tolerance = 50;

                return Math.Abs(r - 170) <= tolerance && Math.Abs(g - 103) <= tolerance && Math.Abs(b - 43) <= tolerance;
            };

            string src = screenImg;
            Console.WriteLine(Path.GetFileName(src));

            int cellTopY = 1084;
            int cellBottomY = 1180;
            int cellMidY = cellTopY + ((cellBottomY - cellTopY) / 2);
            List<LetterInfo> letterTiles = new List<LetterInfo>();
 
            using (MagickImage image = new MagickImage(src))
            {
                int cellLeftX = 0;
                int cellRightX = 0;

                for (int x = 0; x < image.Width - 1; x++)
                {
                    Pixel pix = image.GetReadOnlyPixels()[x, cellMidY];
                    Pixel pixNext = image.GetReadOnlyPixels()[x + 1, cellMidY];
                    if (isBluePixel(pix) && isOrangePixel(pixNext))
                    {
                        cellLeftX = pix.X;
                    }
                    else if (isOrangePixel(pix) && isBluePixel(pixNext))
                    {
                        cellRightX = pixNext.X;
                    }

                    if (cellLeftX < cellRightX)
                    {
                        // tile identified
                        int cellWidth = cellRightX - cellLeftX;
                        int cellHeight = cellBottomY - cellTopY;
                        LetterInfo letterTile = GetLetterTileOrNull(image, cellLeftX, cellTopY, cellWidth, cellHeight, true, LetterOCR.GetHandCharacterMap);

                        ////using (MagickImage image2 = new MagickImage(src))
                        ////{
                        ////    image2.Crop(new MagickGeometry(cellLeftX, cellTopY, cellRightX - cellLeftX, cellBottomY - cellTopY));
                        ////    image2.Write(@"C:\temp\" + "img" + x + Path.GetRandomFileName() + ".png");
                        ////}

                        if (letterTile == null)
                        {
                            // the hand letter are not always consistent
                            // try shifting OCR right and left before concluding this is a blank tile
                            letterTile = GetLetterTileOrNull(image, cellLeftX + 1, cellTopY, cellWidth, cellHeight, true, LetterOCR.GetHandCharacterMap);
                            if (letterTile == null)
                            {
                                letterTile = GetLetterTileOrNull(image, cellLeftX - 1, cellTopY, cellWidth, cellHeight, true, LetterOCR.GetHandCharacterMap);
                            }
                        }

                        if (letterTile != null)
                        {
                            letterTiles.Add(letterTile);
                        }
                        else
                        {
                            // this is a blank tile
                            letterTiles.Add(new LetterInfo(LetterInfo.BLANK, 0, true));

                        }

                        cellRightX = 0;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("$$$$$$ Hand OCR completed in " + DateTime.Now.Subtract(start).TotalSeconds.ToString("0.000") + " seconds");
            return letterTiles.ToArray();
        }

        public static Board OCR(string screenImg)
        {
            DateTime start = DateTime.Now;

            Board board = new Board();
            string src = screenImg;

            Console.WriteLine(Path.GetFileName(src));

            int gridStartX = 0;
            int gridStartY = 327;//248;
            int cellWidth = 48;
            int cellHeight = 48;

            using (MagickImage image = new MagickImage(src))
            {
                for (int x = 0; x < Board.X_Cell_Count; x++)
                {
                    for (int y = 0; y < Board.Y_Cell_Count; y++)
                    {
                        int cellStartX = gridStartX + x * cellWidth;
                        int cellStartY = gridStartY + y * cellHeight;

                        ////// save tile as image for testing slice coordinates
                        ////using (MagickImage imageTile = new MagickImage(src))
                        ////{
                        ////    imageTile.Crop(new MagickGeometry(cellStartX, cellStartY, cellWidth, cellHeight));
                        ////    imageTile.Write("C:\\temp\\" + cellStartY.ToString("0000") + "x" + cellStartX.ToString("0000") + ".png");
                        ////}

                        LetterInfo letterTile = GetLetterTileOrNull(image, cellStartX, cellStartY, cellWidth, cellHeight, false, LetterOCR.GetBoardCharacterMap);
                        if (letterTile != null)
                        {
                            board.SetLetter(x, y, letterTile);
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("$$$$$$ Board OCR completed in " + DateTime.Now.Subtract(start).TotalSeconds.ToString("0.000") + " seconds");
            return board;
        }

        private static LetterInfo GetLetterTileOrNull(MagickImage image, int cellStartX, int cellStartY, int cellWidth, int cellHeight, bool isTransient, Func<char, LetterOCR> getCharacterMap)
        {
            // check in specific order so that for example: 'O' is identified before 'C' because 'C' may be a subset of 'O' pixels.
            foreach (char currChar in new char[] { 
                            'A', 'B', 'D', 'E', 'G', 'H', 'J', 'K', 'U', 'L', 'M', 'N', 'R', 'P', 'Q', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z', 'O', 'C', 'F', 'I' })
            {
                LetterInfo letterTile = GetLetterTileOrNull(image, currChar, cellStartX, cellStartY, cellWidth, cellHeight, isTransient, getCharacterMap);

                // match found
                if (letterTile != null)
                {
                    return letterTile;
                }
            }

            return null;
        }

        private static LetterInfo GetLetterTileOrNull(MagickImage image, char currChar, int cellStartX, int cellStartY, int cellWidth, int cellHeight, bool isTransient, Func<char, LetterOCR> getCharacterMap)
        {
            LetterOCRPoint[] points = getCharacterMap(currChar).Points;
            bool allPointsMatch = true;

            foreach (LetterOCRPoint pnt in points)
            {
                Pixel pixel = image.GetReadOnlyPixels(cellStartX + pnt.x, cellStartY + pnt.y, 1, 1)[0, 0];
                bool pointMatches = IsLetterPixel(pixel);

                if (!pointMatches)
                {
                    allPointsMatch = false;
                    break;
                }
            }

            // all points match
            if (allPointsMatch)
            {
                int letterColorPixelCount = 0;

                // get points value in upper right.  May be blank tile, will not have any letter color pixels in upper right
                foreach (Pixel pixel in image.GetReadOnlyPixels(cellStartX + 32, cellStartY + 0, cellWidth - 32, 13))
                {
                    if (IsLetterPixel(pixel))
                    {
                        letterColorPixelCount++;
                    }
                }

                int letterPointValue = letterColorPixelCount >= 3 ? LetterOCR.GetLetterPointValue(currChar) : 0;

                return new LetterInfo(currChar, letterPointValue, isTransient);
            }
            else
            {
                return null;
            }
        }

        private static bool IsLetterPixel(Pixel pixel)
        {
            // RGBA:
            int red = Convert.ToInt32(pixel.GetChannel(0) / 257);
            int green = Convert.ToInt32(pixel.GetChannel(1) / 257);
            int blue = Convert.ToInt32(pixel.GetChannel(2) / 257);
            //int alpha = Convert.ToInt32(pixel.GetChannel(3) / 257);

            int bitTolerance = Convert.ToInt32(256.0 * 0.20);

            bool isWhite = (red > 248 && green > 248 && blue > 248);
            bool isLetterColor = Math.Abs(red - 78) < bitTolerance && Math.Abs(green - 28) < bitTolerance && Math.Abs(blue - 0) < bitTolerance;
            bool pointMatches = isWhite || isLetterColor;

            return pointMatches;
        }

        /// <summary>
        /// Delete all contents of the folder, but leave the folder itself.
        /// </summary>
        /// <param name="folder">The folder path, relative or absolute.</param>
        /// <param name="logger">The logger, use null to supress logging.</param>
        private static void ClearFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in Directory.GetDirectories(folder, "*", SearchOption.AllDirectories))
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                }
            }
        }
    }
}
