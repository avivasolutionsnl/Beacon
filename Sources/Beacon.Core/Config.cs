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

        /// <summary>
        /// Check the build status only once.
        /// </summary>
        public bool RunOnce { get; set; }

        /// <summary>
        /// Check the build status using the TeamCity guest account.
        /// </summary>
        public bool GuestAccess { get; set; }

        public bool IncludeAllBranches { get; set; }

        public bool IncludeFailedToStart { get; set; }
        
        /// <summary>
        /// Only check the build status on the default branch.
        /// </summary>
        public bool OnlyDefaultBranch { get; set; }
    }
}