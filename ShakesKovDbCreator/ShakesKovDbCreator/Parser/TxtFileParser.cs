using ShakesKovDbCreator.Database.Implementation;
using ShakesKovDbCreator.Database.Interfaces;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakesKovDbCreator.Parser
{
    internal class TxtFileParser
    {
        private FileInfo inFile;
        private FileInfo outFile;
        private IDbDriver? dbDriver;

        private string? prev;

        /// <summary>
        /// Parses the text file by turning it into tokens and feeding the tokens into the database
        /// </summary>
        /// <param name="inFile">The text file to read</param>
        /// <param name="outFile">The database file</param>
        public TxtFileParser(FileInfo inFile, FileInfo outFile)
        {
            this.inFile = inFile;
            this.outFile = outFile;
        }

        /// <summary>
        /// Parses the text file token by token
        /// </summary>
        /// <param name="task">The task to add the percentages</param>
        public void Parse(ProgressTask task)
        {
            string[] lines = File.ReadAllLines(this.inFile.FullName);
            long max = lines.LongLength;
            long count = max / 1000;
            long pos = 0;

            using (var db = new SqliteDriver(this.outFile))
            {
                lines.AsEnumerable<string>().ToList().ForEach(line =>
                {
                    ParseLine(line, db);
                    pos++;
                    if((pos % count) == 0)
                        task.Increment(0.1);
                });
            }
        }

        /// <summary>
        /// Parses a line
        /// </summary>
        /// <param name="line">The line to parse</param>
        private void ParseLine(string line, IDbDriver db)
        {
            string[] tokens = line.Split(" ");
            foreach(string token in tokens)
            {
                var filteredToken = token.Trim();
                string[] lineBreaks = { "\t", "\r", "\n" };
                lineBreaks.AsEnumerable().ToList().ForEach(lb => { filteredToken = filteredToken.Replace(lb, ""); });

                string[] quoteMarks = { "<", ">", "'" };
                quoteMarks.AsEnumerable().ToList().ForEach(ap => { filteredToken = filteredToken.Replace(ap, "\""); });

                bool isQuoteEnd = false;
                if(filteredToken.Length > 1)
                {
                    isQuoteEnd = filteredToken.Substring(1).Contains("\"");
                }

                bool isQuoteStart = filteredToken.StartsWith("\"");

                string[] lineEndings = { ".", "!", "?" };
                bool isLineEnd = lineEndings.Any((x) => filteredToken.EndsWith(x));

                db.InsertOrUpdateWord(filteredToken, isLineEnd, isQuoteStart, isQuoteEnd);

                if(this.prev != null)
                {
                    db.InsertOrUdatePair(this.prev, filteredToken);
                }

                this.prev = filteredToken;
            }
        }
    }
}
