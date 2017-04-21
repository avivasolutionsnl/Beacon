using System;

namespace Beacon.Core
{
    public class Config
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BuildTypeIds { get; set; }

        /// <summary>
        /// The interval at which to check the status of all builds.
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// The period of time to look back for builds while evaluating the state.
        /// </summary>
        public TimeSpan TimeSpan { get; set; }
    }
}