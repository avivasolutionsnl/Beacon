using System;

namespace Beacon.Core
{
    public class AzureDevopsConfig : Config
    {
        public Uri Url { get; set; }
        
        public string ProjectName { get; set; }
        
        public int DefinitionId { get; set; }
        
        public string BranchName { get; set; }
        
        public string PersonalAccessToken { get; set; }
    }
}