using System;

using Beacon.Core;

namespace Beacon.Lights.Console
{
    internal class ConsoleBuildLight : IBuildLight
    {
        public void Success()
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Build is successful");
            System.Console.ResetColor();
        }

        public void Investigate()
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("Build is being investigated");
            System.Console.ResetColor();
        }

        public void Fail()
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Build has failed");
            System.Console.ResetColor();
        }

        public void Fixed()
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGreen;
            System.Console.WriteLine("Build is successful");
            System.Console.ResetColor();
        }

        public void NoStatus()
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine("Build status is unknown");
            System.Console.ResetColor();
        }
    }
}