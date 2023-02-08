using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakesKovDbCreator
{
    internal static class Log
    {
        public static void Debug(string message)
        {
            AnsiConsole.MarkupLine($"[teal]Debug:[/] {message}");
        }

        public static void Info(string message)
        {
            AnsiConsole.MarkupLine($"[orange4_1]Info:[/] {message}");
        }

        public static void Error(string message)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {message}");
        }
    }
}
