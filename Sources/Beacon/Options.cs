using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Beacon
{
    internal class Options
    {
        [Option("url", Required = true, HelpText = "The root URL of the TeamCity server.")]
        public string Url { get; set; }

        [Option("username", HelpText = "The username of the account that has read access to TeamCity. Username and guestaccess are mutually exclusive.", 
            MutuallyExclusiveSet = "auth")]
        public string Username { get; set; }

        [Option("password", HelpText = "The password of the account that has read access to TeamCity.")]
        public string Password { get; set; }

        [OptionArray("builds", Required = true, HelpText = "One or more builds identified by their TeamCity id (eg, bt64. bt12 or * for all).")]
        public string[] BuildTypeIds { get; set; }

        [Option("device", DefaultValue = "delcom", HelpText = "The device to use as the build light (e.g. console, delcom).")]
        public string Device { get; set; }

        [Option("interval", DefaultValue = "10", HelpText = "The interval in seconds at which to check the build status.")]
        public string IntervalInSeconds { get; set; }

        [Option("timespan", DefaultValue = "7", HelpText = "The timespan in days to include builds from.")]
        public string Timespan { get; set; }

        [Option('g', "guestaccess", HelpText = "Check the build status using the TeamCity guest account. Username and guestaccess are mutually exclusive.", 
            MutuallyExclusiveSet = "auth")]
        public bool GuestAccess { get; set; }

        [Option('r', "runonce", HelpText = "Check the build status only once.")]
        public bool RunOnce { get; set; }

        [Option('v', "verbose", HelpText = "Log verbose messages.")]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Beacon: TeamCity Monitor", Version),
                Copyright = new CopyrightInfo("Dennis Doomen", 2015, 2016, 2017, 2018),
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

        private static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}