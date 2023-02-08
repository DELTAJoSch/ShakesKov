using Microsoft.Data.Sqlite;
using ShakesKovDbCreator.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakesKovDbCreator.Database.Implementation
{
    internal class SqliteDriver : IDbDriver, IDisposable
    {
        private SqliteConnection connection;

        /// <summary>
        /// Creates the database and connects to it
        /// </summary>
        /// <param name="dbFile">The database</param>
        public SqliteDriver(FileInfo dbFile) 
        {
            this.connection = new SqliteConnection($"Data Source={dbFile.FullName};Mode=ReadWriteCreate");
            try
            {
                this.connection.Open();
                this.CreateTables();
            }
            catch (SqliteException)
            {
                Log.Error("An error occured whilst creating the database!");
                throw;
            }
        }

        /// <summary>
        /// Creates the tables in the database
        /// </summary>
        private void CreateTables()
        {
            var enableSynchronousStatement = "PRAGMA synchronous = OFF;";
            var enableSynchronous = new SqliteCommand(enableSynchronousStatement, this.connection);
            enableSynchronous.ExecuteNonQuery();

            var transaction = this.connection.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var enableForeignKeysStatement = "PRAGMA foreign_keys = ON;";

                var wordTableCreateStatement = @"CREATE TABLE IF NOT EXISTS words (
                                                    word TEXT PRIMARY KEY,
                                                    isLineEnd INTEGER NOT NULL DEFAULT 0,
                                                    isQuoteStart INTEGER NOT NULL DEFAULT 0,
                                                    isQuoteEnd INTEGER NOT NULL DEFAULT 0,
                                                    occurrences INTEGER NOT NULL DEFAULT 1
                                                 );";

                var pairTableCreateStatement = @"CREATE TABLE IF NOT EXISTS pairs (
                                                    previous TEXT REFERENCES words(word),
                                                    following TEXT REFERENCES words(word),
                                                    occurrences INTEGER NOT NULL DEFAULT 1,
                                                    PRIMARY KEY (previous, following)
                                                 );";

                var enableForeignKeys = new SqliteCommand(enableForeignKeysStatement, this.connection, transaction);
                var wordTableCreate = new SqliteCommand(wordTableCreateStatement, this.connection, transaction);
                var pairTableCreate = new SqliteCommand(pairTableCreateStatement, this.connection, transaction);


                enableForeignKeys.ExecuteNonQuery();
                wordTableCreate.ExecuteNonQuery();
                pairTableCreate.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (SqliteException)
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Disposes of the database connection
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.connection.Close();
                this.connection.Dispose();
            }
            catch (SqliteException)
            {
                Log.Error("An error occured whilst disposing of the database connection!");
                throw;
            }
        }

        /// <summary>
        /// Inserts or updates the count of a pair depending on the pair given
        /// </summary>
        /// <param name="prev">the previous word</param>
        /// <param name="following">the following word</param>
        public void InsertOrUdatePair(string prev, string following)
        {
            var transaction = this.connection.BeginTransaction();

            try
            {
                var getPairIfExistsStatement = @"SELECT * FROM pairs WHERE previous = $prevword AND following = $followingword;";
                var getPairCommand = new SqliteCommand(getPairIfExistsStatement, this.connection, transaction);
                getPairCommand.Parameters.AddWithValue("$prevword", prev);
                getPairCommand.Parameters.AddWithValue("$followingword", following);

                var createPairStatement = @"INSERT INTO pairs VALUES ($prev, $following, 1);";

                var updatePairStatement = @"UPDATE pairs SET occurrences = $count WHERE previous = $prevword AND following = $followingword";

                using (var reader = getPairCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var updateWordCommand = new SqliteCommand(updatePairStatement, this.connection, transaction);
                        updateWordCommand.Parameters.AddWithValue("$prevword", prev);
                        updateWordCommand.Parameters.AddWithValue("$followingword", following);
                        updateWordCommand.Parameters.AddWithValue("$count", reader.GetInt32(2) + 1);
                        updateWordCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        var createWordCommand = new SqliteCommand(createPairStatement, this.connection, transaction);
                        createWordCommand.Parameters.AddWithValue("$prev", prev);
                        createWordCommand.Parameters.AddWithValue("$following", following);
                        createWordCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (SqliteException)
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Inserts or updates the count of the word
        /// </summary>
        /// <param name="word">the word</param>
        /// <param name="isLineEnd">if true, signals that this word is a valid line ending</param>
        /// <param name="isQuoteStart">if true, signals that this is a valid quote start</param>
        /// <param name="isQuoteEnd">if true, signals that this is a valid quote end</param>
        public void InsertOrUpdateWord(string word, bool isLineEnd = false, bool isQuoteStart = false, bool isQuoteEnd = false)
        {
            var transaction = this.connection.BeginTransaction();

            try
            {
                var getWordIfExistsStatement = @"SELECT * FROM words WHERE word = $wordtext;";
                var getWordCommand = new SqliteCommand(getWordIfExistsStatement, this.connection, transaction);
                getWordCommand.Parameters.AddWithValue("$wordtext", word);

                var createWordStatement = @"INSERT INTO words VALUES ($wordparam, $isLineEnd, $isQuoteStart, $isQuoteEnd, 1);";

                var updateWordStatement = @"UPDATE words SET occurrences = $count WHERE word = $wordparam;";

                using(var reader = getWordCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var updateWordCommand = new SqliteCommand(updateWordStatement, this.connection, transaction);
                        updateWordCommand.Parameters.AddWithValue("$wordparam", word);
                        updateWordCommand.Parameters.AddWithValue("$count", reader.GetInt32(4) + 1);
                        updateWordCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        var createWordCommand = new SqliteCommand(createWordStatement, this.connection, transaction);
                        createWordCommand.Parameters.AddWithValue("$wordparam", word);
                        createWordCommand.Parameters.AddWithValue("$isLineEnd", isLineEnd ? 1 : 0);
                        createWordCommand.Parameters.AddWithValue("$isQuoteEnd", isLineEnd ? 1 : 0);
                        createWordCommand.Parameters.AddWithValue("$isQuoteStart", isLineEnd ? 1 : 0);
                        createWordCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
