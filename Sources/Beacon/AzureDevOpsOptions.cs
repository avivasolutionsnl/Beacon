using CommandLine;

namespace Beacon
{
    [Verb("azuredevops", HelpText = "Use Azure Devops as build server")]
    internal class AzureDevOpsOptions : Options
    {
        [Option("url", Required = true, HelpText = "Azure Devops URL")]
        public string Url { get; set; }

        [Option("project", Required = true, HelpText = "Azure Devops project name")]
        public string ProjectName { get; set; }

        [Option("token", HelpText = "Azure Devops personal access token, required for private projects")]
        public string PersonalAccessToken { get; set; }
    }
}