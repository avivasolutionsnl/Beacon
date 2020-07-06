using System;

namespace Beacon.Core
{
    public class Config
    {
        /// <summary>
        /// Check the build status only once.
        /// </summary>
        public bool RunOnce { get; set; }
        
        /// <summary>
        /// The interval at which to check the status of all builds.
        /// </summary>
        public TimeSpan Interval { get; set; }
        
        public string BuildTypeIds { get; set; }
    }
}