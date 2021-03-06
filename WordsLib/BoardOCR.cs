﻿using ImageMagick;
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

            Func<Pixel, bool> isBackgroundPixel = (Pixel pixel) =>
            {
                int r = Convert.ToInt32(pixel.GetChannel(0) / 257);
                int g = Convert.ToInt32(pixel.GetChannel(1) / 257);
                int b = Convert.ToInt32(pixel.GetChannel(2) / 257);
                int tolerance = 50;

                return Math.Abs(r - 133) <= tolerance && Math.Abs(g - 158) <= tolerance && Math.Abs(b - 183) <= tolerance;
            };

            Func<Pixel, bool> isTileEdgePixel = (Pixel pixel) =>
            {
                int r = Convert.ToInt32(pixel.GetChannel(0) / 257);
                int g = Convert.ToInt32(pixel.GetChannel(1) / 257);
                int b = Convert.ToInt32(pixel.GetChannel(2) / 257);
                int tolerance = 50;
                
                return (Math.Abs(r - 216) <= tolerance && Math.Abs(g - 143) <= tolerance && Math.Abs(b - 27) <= tolerance)
                        || (Math.Abs(r - 168) <= tolerance && Math.Abs(g - 151) <= tolerance && Math.Abs(b - 110) <= tolerance);
            };

            string src = screenImg;
            Console.WriteLine(Path.GetFileName(src));

            int cellTopY = 1054;
            int cellBottomY = 1152;
            int cellMidY = cellTopY + ((cellBottomY - cellTopY) / 2);
            int cellHeight = cellBottomY - cellTopY;
            int cellWidth = cellHeight; // square
            List<LetterInfo> letterTiles = new List<LetterInfo>();

            // x coordinates of the left edge each of the 7 tile slots.
            List<int> leftXs = new List<int>() { 2, 105, 208, 311, 414, 517, 619 };
 
            using (MagickImage image = new MagickImage(src))
            {
                int cellLeftX = 0;
                int cellRightX = 0;

                for (int x = 0; x < image.Width - 1; x++)
                {
                    Pixel pix = image.GetReadOnlyPixels()[x, cellMidY];
                    Pixel pixNext = image.GetReadOnlyPixels()[x + 1, cellMidY];

                    if (leftXs.Contains(pix.X))
                    {
                        cellLeftX = pix.X;
                        cellRightX = pix.X + cellWidth;
                    }

                    if (cellLeftX < cellRightX)
                    {
                        Pixel backgroundTestPixel = image.GetReadOnlyPixels()[cellLeftX + 3, cellMidY];
                        if (isTileEdgePixel(backgroundTestPixel))
                        {
                            ////// debug - save sliced tile
                            ////using (MagickImage image2 = new MagickImage(src))
                            ////{
                            ////    image2.Crop(new MagickGeometry(cellLeftX, cellTopY, cellRightX - cellLeftX, cellBottomY - cellTopY));
                            ////    image2.Write(@"C:\temp\" + "img" + x + Path.GetRandomFileName() + ".png");
                            ////}

                            LetterInfo letterTile = GetLetterTileOrNull(image, cellLeftX, cellTopY, cellWidth, cellHeight, true, LetterOCR.GetHandCharacterMap);

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
                        }

                        // reset markers
                        cellRightX = 0;
                        cellLeftX = image.Width;
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

                        ////// debug - save tile as image for testing slice coordinates
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
            LetterOCRPoint[] letterOcrPoints = getCharacterMap(currChar).Points;
            bool allPointsMatch = true;

            // when score badge is present need to also allow for red as letter color
            bool isLowerRightPixelRedOrWhite = false;
            int cornerScanLength = 3;
            foreach (Pixel lowerRightPixel in image.GetReadOnlyPixels(cellStartX + cellWidth - cornerScanLength, cellStartY + cellHeight - cornerScanLength, cornerScanLength, cornerScanLength))
            {
                isLowerRightPixelRedOrWhite = IsRedPixel(lowerRightPixel) || IsWhitePixel(lowerRightPixel);
                if (isLowerRightPixelRedOrWhite)
                {
                    break;
                }
            }

            foreach (LetterOCRPoint ocrPt in letterOcrPoints)
            {
                Pixel pixel = image.GetReadOnlyPixels(cellStartX + ocrPt.x, cellStartY + ocrPt.y, 1, 1)[0, 0];
                bool pointMatches = IsBrownPixel(pixel) || IsWhitePixel(pixel) || (isLowerRightPixelRedOrWhite && IsRedPixel(pixel));

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
                foreach (Pixel pixel in image.GetReadOnlyPixels(cellStartX + cellWidth - 35, cellStartY, 35, 14))
                {
                    if (IsBrownPixel(pixel) || IsWhitePixel(pixel))
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

        private static bool IsBrownPixel(Pixel pixel)
        {
            // RGBA:
            int red = Convert.ToInt32(pixel.GetChannel(0) / 257);
            int green = Convert.ToInt32(pixel.GetChannel(1) / 257);
            int blue = Convert.ToInt32(pixel.GetChannel(2) / 257);
            //int alpha = Convert.ToInt32(pixel.GetChannel(3) / 257);

            int bitTolerance = Convert.ToInt32(256.0 * 0.20);

            bool isLetterColor = Math.Abs(red - 78) < bitTolerance && Math.Abs(green - 28) < bitTolerance && Math.Abs(blue - 0) < bitTolerance;
            return isLetterColor;
        }

        private static bool IsWhitePixel(Pixel pixel)
        {
            // RGBA:
            int red = Convert.ToInt32(pixel.GetChannel(0) / 257);
            int green = Convert.ToInt32(pixel.GetChannel(1) / 257);
            int blue = Convert.ToInt32(pixel.GetChannel(2) / 257);
            //int alpha = Convert.ToInt32(pixel.GetChannel(3) / 257);

            bool isWhiteColor = red > 240 && green > 230 && blue > 210;
            return isWhiteColor;
        }

        private static bool IsRedPixel(Pixel pixel)
        {
            // RGBA:
            int red = Convert.ToInt32(pixel.GetChannel(0) / 257);
            int green = Convert.ToInt32(pixel.GetChannel(1) / 257);
            int blue = Convert.ToInt32(pixel.GetChannel(2) / 257);
            //int alpha = Convert.ToInt32(pixel.GetChannel(3) / 257);

            // RED => red is high, greater than both green and blue and green and blue are roughly equal
            bool isRedColor = red > 150 && red - green > 20 && red - blue > 20 && Math.Abs(green - blue) < 20;
            return isRedColor;
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
