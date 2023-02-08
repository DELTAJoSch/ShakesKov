using ShakesKovDbCreator.Parser;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakesKovDbCreator
{
    internal static class EntryPoints
    {
        /// <summary>
        /// Entry point for create database
        /// </summary>
        /// <param name="inFile">The input file</param>
        /// <param name="outFile">The output file</param>
        internal static void CreateDb(FileInfo? inFile, FileInfo? outFile)
        {
            inFile = inFile ?? new FileInfo("input.txt");
            outFile = outFile ?? new FileInfo("out.db");

            Log.Info($"In-File: {inFile.Name}");
            Log.Info($"Out-File: {outFile.Name}");

            if(!inFile.Exists)
            {
                Log.Error("In-File does not exist! Did you mistype the name or path?");
                return;
            }

            var parser = new TxtFileParser(inFile, outFile);

            AnsiConsole.Progress().Start(
                ctx =>
                {
                    var task = ctx.AddTask("[springgreen2_1]Parsing[/]");
                    task.StartTask();
                    parser.Parse(task);
                });
        }
    }
}
