using CommandLine;

namespace Beacon
{
    [Verb("azuredevops", HelpText = "Use Azure Devops as build server")]
    internal class AzureDevopsOptions : Options
    {
        [Option("url", Required = true, HelpText = "Azure Devops URL")]
        public string Url { get; set; }

        [Option("project", Required = true, HelpText = "Azure Devops project name")]
        public string ProjectName { get; set; }

        [Option("definitionid", Required = true, HelpText = "Azure Devops definition id")]
        public int DefinitionId { get; set; }

        [Option('b', "branch", Default="refs/heads/master", HelpText = "Branch name")]
        public string BranchName { get; set; }

        [Option("token", HelpText = "Azure Devops personal access token, required for private projects")]
        public string PersonalAccessToken { get; set; }
    }
}