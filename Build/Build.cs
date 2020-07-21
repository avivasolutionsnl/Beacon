using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Chocolatey API key")] readonly string ChocoApiKey;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    [PathExecutable]
    readonly Tool Choco;
    
    [PathExecutable(name: "7z")]
    readonly Tool Zip;
    
    AbsolutePath OutputDirectory => PackageDirectory / "Output";
    AbsolutePath PackageDirectory => RootDirectory / "Package";
    AbsolutePath SourceDirectory => RootDirectory / "Sources";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            NuGetRestore(s => s
                .SetTargetPath(SourceDirectory));
        });

    Target RunGitVersion =>
        _ => _
            .Executes(() =>
            {
                var updateAssemblyInfo = IsServerBuild; // Only update AssemblyInfo.cs files on server, to avoid local changes

                GitVersionTasks.GitVersion(c => c
                    .SetFramework("netcoreapp3.0")
                    .SetUpdateAssemblyInfo(updateAssemblyInfo)
                    .SetWorkingDirectory(RootDirectory)
                );
            });
    
    Target Compile => _ => _
        .DependsOn(Restore, RunGitVersion)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild));
        });
    
    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            // Pack the artifacts such that these can be published using Chocolatey            
            CopyFile(RootDirectory / "LICENSE", OutputDirectory / "LICENSE", FileExistsPolicy.Overwrite);
            CopyFile(RootDirectory / "README.md", OutputDirectory / "README.md", FileExistsPolicy.Overwrite);
            CopyFile(RootDirectory / "VERIFICATION.txt", OutputDirectory / "VERIFICATION.txt", FileExistsPolicy.Overwrite);
            CopyDirectoryRecursively(SourceDirectory / "Beacon" / "bin" / Configuration, OutputDirectory, DirectoryExistsPolicy.Merge);
            
            Choco($"pack {PackageDirectory / "Beacon.nuspec"} --version {GitVersion.NuGetVersion}", workingDirectory: PackageDirectory);
        });

    Target Publish =>
        _ => _
            .DependsOn(Pack)
            .Executes(() =>
            {
                var filename = $"Beacon.{GitVersion.NuGetVersion}.nupkg";
                Choco($"push {PackageDirectory / filename} --apiKey={ChocoApiKey}", workingDirectory: PackageDirectory);
            });
}
