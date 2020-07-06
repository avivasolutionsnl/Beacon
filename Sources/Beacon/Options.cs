using System.Collections.Generic;

using CommandLine;

namespace Beacon
{
    internal class Options
    {
        [Option("device", Default = "delcom", 
            HelpText = "The device to use as the build light (e.g. console, delcom, shelly).")]
        public string Device { get; set; }
        
        [Option("shellyurl", HelpText = "The base URL of the Shelly Bulb")]
        public string ShellyUrl { get; set; }
        
        [Option('r', "runonce", Default = false, HelpText = "Check the build status only once.")]
        public bool RunOnce { get; set; }

        [Option("interval", Default = 10, 
            HelpText = "The interval in seconds at which to check the build status.")]
        public int IntervalInSeconds { get; set; }
        
        [Option("builds", Required = true, 
            HelpText = "One or more builds identified by their TeamCity id (eg, bt64 bt12 or * for all), or Azure Devops definition id (eg 1, 2, * for all))")]
        public IEnumerable<string> BuildTypeIds { get; set; }
        
        [Option('v', "verbose", HelpText = "Log verbose messages.")]
        public bool Verbose { get; set; }
    }
}