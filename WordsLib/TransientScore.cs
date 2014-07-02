using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class TransientScore
    {
        public List<UnitScore> Words { get; set; }

        public int TotalScore
        {
            get
            {
                int total = 0;
                if (this.Words != null)
                {
                    foreach (UnitScore unit in this.Words)
                    {
                        total += unit.Score;
                    }
                }
                return total;
            }
        }

        public void AddWord(UnitScore unit)
        {
            if (this.Words == null)
            {
                this.Words = new List<UnitScore>();
            }

            this.Words.Add(unit);
        }

        public UnitScore BestWord
        {
            get
            {
                UnitScore unit = null;
                if (this.Words != null)
                {
                    unit = this.Words.OrderByDescending(x => x.Score).First();
                }

                return unit;
            }
        }
    }

    public class UnitScore
    {
        public int StartX { get; set; }
        public int EndX { get; set; }
        public int StartY { get; set; }
        public int EndY { get; set; }

        public string Word { get; set; }
        public int Score { get; set; }
    }
}
