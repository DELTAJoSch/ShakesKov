using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakesKovDbCreator.Database.Interfaces
{
    internal interface IDbDriver
    {
        /// <summary>
        /// Inserts or updates the count of a pair depending on the pair given
        /// </summary>
        /// <param name="prev">the previous word</param>
        /// <param name="following">the following word</param>
        public void InsertOrUdatePair(string prev, string following);

        /// <summary>
        /// Inserts or updates the count of the word
        /// </summary>
        /// <param name="word">the word</param>
        /// <param name="isLineEnd">if true, signals that this word is a valid line ending</param>
        /// <param name="isQuoteStart">if true, signals that this is a valid quote start</param>
        /// <param name="isQuoteEnd">if true, signals that this is a valid quote end</param>
        public void InsertOrUpdateWord(string word, bool isLineEnd = false, bool isQuoteStart = false, bool isQuoteEnd = false);
    }
}
