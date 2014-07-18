using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsLib
{
    public class WordsLookup
    {
        private string[] allWords;
        private Dictionary<string, bool> wordRootMap;

        public void Init()
        {
            DateTime start = DateTime.Now;
            this.allWords = File.ReadAllLines(Path.Combine(this.AssemblyDirectory, "dictionary/words.txt"));

            wordRootMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (string word in allWords)
            {
                this.AddWordSegmentRecursive(word, true);
            }

            System.Diagnostics.Debug.WriteLine("$$$$$$ Word lookup initialized in " + DateTime.Now.Subtract(start).TotalSeconds.ToString("0.000") + " seconds");
        }

        public bool IsWord(string word)
        {
            return this.wordRootMap.ContainsKey(word) && this.wordRootMap[word];
        }

        public bool IsWordOrSegment(string segment)
        {
            return this.wordRootMap.ContainsKey(segment);
        }

        /// <summary>
        /// Add the word to the lookup and every segement within the word to the lookup.
        /// </summary>
        /// <param name="segment">the word or segment to add and all of it's sub segments will also be added.</param>
        /// <param name="isWord">true if this is a word, false if only a segment.</param>
        private void AddWordSegmentRecursive(string segment, bool isWord)
        {
            if (this.wordRootMap.ContainsKey(segment))
            {
                if (isWord && !this.wordRootMap[segment])
                {
                    // was previously a segment, now tag this as a word.
                    this.wordRootMap[segment] = true;
                }

                // hault recursion, already added this segment and all it's sub segments.
                return;
            }
            else
            {
                // add word or segment
                this.wordRootMap[segment] = isWord;
            }

            // by taking the all letters except last letter as "left" and all letters except first as "right"
            // recursively this will pick up all segments.
            if (segment.Length > 2)
            {
                string left = segment.Substring(0, segment.Length - 1);
                string right = segment.Substring(1, segment.Length - 1);
                this.AddWordSegmentRecursive(left, false);
                this.AddWordSegmentRecursive(right, false);
            }
        }
        
        private string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
