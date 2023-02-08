using System;
using System.CommandLine;

namespace ShakesKovDbCreator
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var fileOption = new Option<FileInfo?>(name: "--file", description: "Specify a File to parse");
            fileOption.AddAlias("-f");

            var outOption = new Option<FileInfo?>(name: "--out", description: "Specify the name of the file to output (database-file)");

            var rootCommand = new RootCommand("ShakesKovDbCreator - create ShakesKov-compatible databases from text files!");
            rootCommand.AddOption(fileOption);
            rootCommand.AddOption(outOption);

            rootCommand.SetHandler((inFile, outFile) =>
            {
                EntryPoints.CreateDb(inFile, outFile);
            },
            fileOption, outOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}