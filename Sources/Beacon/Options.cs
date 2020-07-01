using CommandLine;

namespace Beacon
{
    internal class Options
    {
        [Option("device", Default = "delcom", 
            HelpText = "The device to use as the build light (e.g. console, delcom).")]
        public string Device { get; set; }
        
        [Option('r', "runonce", Default = false, HelpText = "Check the build status only once.")]
        public bool RunOnce { get; set; }

        [Option("interval", Default = 10, 
            HelpText = "The interval in seconds at which to check the build status.")]
        public int IntervalInSeconds { get; set; }
        
        [Option('v', "verbose", HelpText = "Log verbose messages.")]
        public bool Verbose { get; set; }
    }
}