namespace Beacon.Core
{
    public enum BuildStatus
    {
        Unavailable,
        Passed,
        Investigating,
        
        /// <summary>
        /// The build failed during its last run, but was marked as fixed as part of an investigation.
        /// </summary>
        Fixed,
        Failed
    }
}