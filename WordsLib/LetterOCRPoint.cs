using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class LetterOCRPoint
    {
        public int x { get; set; }
        public int y { get; set; }

        public static LetterOCRPoint[] Parse(string s)
        {
            List<LetterOCRPoint> points = new List<LetterOCRPoint>();
            string[] parts = s.Split(new char[] { ' ' });
            foreach (string part in parts)
            {
                points.Add(new LetterOCRPoint(part));
            }

            return points.ToArray();
        }

        public LetterOCRPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// string format "x,y"
        /// </summary>
        public LetterOCRPoint(string xy)
        {
            string[] pts = xy.Split(new char[] { ',' });
            this.x = int.Parse(pts[0].Trim());
            this.y = int.Parse(pts[1].Trim());
        }

        public override string ToString()
        {
            return "(" + this.x + "," + this.y + ")";
        }
    }
}
