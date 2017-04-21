using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Beacon
{
    internal class Options
    {
        [Option("url", Required = true, HelpText = "The root URL of the team city server.")]
        public string Url { get; set; }

        [Option("username", Required = true, HelpText = "The username of the account that has read access to TeamCity")]
        public string Username { get; set; }

        [Option("password", Required = true, HelpText = "The password of the account that has read access to TeamCity")]
        public string Password { get; set; }

        [OptionArray("builds", Required = true, HelpText = "One more builds identified by their id (eg, bt64. bt12 or * for all)")]
        public string[] BuildTypeIds { get; set; }

        [Option("device", DefaultValue = "delcom", HelpText = "The device to use as the build light (e.g. console, delcom)")]
        public string Device { get; set; }

        [Option("interval", DefaultValue = "10", HelpText = "The interval in seconds at which to check the status")]
        public string IntervalInSeconds { get; set; }

        [Option("timespan", DefaultValue = "7", HelpText = "The timespan in days to include builds from.")]
        public string Timespan { get; set; }

        [Option('v', "verbose", HelpText = "Log verbose messages")]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Beacon: Team City Monitor", Version),
                Copyright = new CopyrightInfo("Dennis Doomen", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            AddErrors(help);

            help.AddOptions(this);

            return help;
        }

        private void AddErrors(HelpText help)
        {
            if (LastParserState.Errors.Any())
            {
                var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces

                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }
        }

        private static string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }
    }
}