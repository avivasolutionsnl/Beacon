using System;

namespace Beacon.Core
{
    public class Logger
    {
        public static void WriteErrorLine(string message, params object[] parameters)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(message, parameters);
            Console.ForegroundColor = foregroundColor;
        }

        public static void WriteWarningLine(string message, params object[] parameters)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine(message, parameters);
            Console.ForegroundColor = foregroundColor;
        }

        public static void WriteLine(string message, params object[] parameters)
        {
            var messageToLog = string.Format(message, parameters);
            Console.WriteLine("{0} {1}", DateTime.Now.ToShortTimeString(), messageToLog);
        }

        public static void Verbose(string message, params object[] parameters)
        {
            if (VerboseEnabled)
            {
                var messageToLog = string.Format(message, parameters);
                Console.WriteLine("VERBOSE: {0} {1}", DateTime.Now.ToShortTimeString(), messageToLog);
            }
        }

        public static void Error(Exception exception)
        {
            WriteLine(exception.ToString());
        }

        public static bool VerboseEnabled { get; set; }
    }
}