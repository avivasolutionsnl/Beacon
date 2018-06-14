properties { 
	$BaseDirectory = Resolve-Path .. 
    
    $ProjectName = "Beacon"
    
    $AssemblyVersion = "1.2.3.4"
	$InformationalVersion = "1.2.3-unstable.34+34.Branch.develop.Sha.19b2cd7f494c092f87a522944f3ad52310de79e0"
	$NuGetVersion = "1.2.3-unstable4"
	
	$PackageDirectory = "$BaseDirectory\Package"
	
	$SrcDir = "$BaseDirectory\Sources"
    $ReportsDir = "$BaseDirectory\TestResults"
	$SolutionFilePath = "$SrcDir\$ProjectName.sln"

	$7zip = "$BaseDirectory\Tools\7z.exe"
    $NugetExe = "$BaseDirectory\Tools\nuget.exe"
    $GitVersionExe = "$BaseDirectory\Tools\GitVersion.exe"
}

TaskSetup {
    TeamCity-ReportBuildProgress "Starting task $($psake.context.Peek().currentTaskName)"
}

TaskTearDown {
    TeamCity-ReportBuildProgress "Finished task $($psake.context.Peek().currentTaskName)"
}

task default -depends Clean, CreateChocoPackages

task DetermineMsBuildPath -depends RestoreNugetPackages {
	Write-Host "Adding msbuild to the environment path"

	$installationPath = & "$BaseDirectory\Sources\Packages\vswhere.2.4.1\tools\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath

	if ($installationPath) {
		$msbuildPath = join-path $installationPath 'MSBuild\15.0\Bin'

		if (test-path $msbuildPath) {
                        Write-Host "msbuild directory set to $msbuildPath"
			$env:path = "$msbuildPath;$env:path"
		}
	}
}

task Clean -depends DetermineMsBuildPath -Description "Cleaning solution." {
	Get-ChildItem $PackageDirectory *.nupkg | ForEach-Object { Remove-Item $_.FullName }
	Get-ChildItem $PackageDirectory *.zip | ForEach-Object { Remove-Item $_.FullName }
	
	exec { msbuild /nologo /verbosity:minimal $SolutionFilePath /t:Clean /p:VSToolsPath="$SrcDir\Packages\MSBuild.Microsoft.VisualStudio.Web.targets.11.0.2.1\tools\VSToolsPath" }    
}

task ExtractVersionsFromGit {
        $json = . "$GitVersionExe" 
        
        if ($LASTEXITCODE -eq 0) {
            $version = (ConvertFrom-Json ($json -join "`n"));
          
            TeamCity-SetBuildNumber $version.FullSemVer;
            
            $script:AssemblyVersion = $version.ClassicVersion;
            $script:InformationalVersion = $version.InformationalVersion;
            $script:NuGetVersion = $version.NuGetVersionV2;
        }
        else {
            Write-Output $json -join "`n";
        }
}

task ApplyAssemblyVersioning -depends ExtractVersionsFromGit {
	Get-ChildItem -Path $SrcDir -Filter "?*AssemblyInfo.cs" -Recurse -Force |
	foreach-object {  

		Set-ItemProperty -Path $_.FullName -Name IsReadOnly -Value $false

        $content = Get-Content $_.FullName
        
        if ($script:AssemblyVersion) {
    		Write-Host "Updating " $_.FullName "with version" $script:AssemblyVersion
    	    $content = $content -replace 'AssemblyVersion\("(.+)"\)', ('AssemblyVersion("' + $script:AssemblyVersion + '")')
            $content = $content -replace 'AssemblyFileVersion\("(.+)"\)', ('AssemblyFileVersion("' + $script:AssemblyVersion + '")')
        }
		
        if ($script:InformationalVersion) {
    		Write-Host "Updating " $_.FullName "with information version" $script:InformationalVersion
            $content = $content -replace 'AssemblyInformationalVersion\("(.+)"\)', ('AssemblyInformationalVersion("' + $script:InformationalVersion + '")')
        }
        
	    Set-Content -Path $_.FullName $content
	}    
}

task ApplyPackageVersioning -depends ExtractVersionsFromGit {
    TeamCity-Block "Updating package version with build number $BuildNumber" {   
	
		$fullName = "$PackageDirectory\Beacon.nuspec"

	    Set-ItemProperty -Path $fullName -Name IsReadOnly -Value $false
		
	    $content = Get-Content $fullName
	    $content = $content -replace '<version>.+</version>', ('<version>' + "$script:NuGetVersion" + '</version>')
	    Set-Content -Path $fullName $content
	}
}

task RestoreNugetPackages {
    $packageConfigs = Get-ChildItem $BaseDirectory -Recurse | Where-Object { $_.Name -eq "packages.config" }

    foreach($packageConfig in $packageConfigs) {
    	Write-Host "Restoring" $packageConfig.FullName 
    	exec { 
            . "$NugetExe" install $packageConfig.FullName -OutputDirectory "$SrcDir\Packages" -ConfigFile "$SrcDir\.nuget\NuGet.Config"
        }
    }
}

task Compile -depends DetermineMsBuildPath, ApplyAssemblyVersioning, ApplyPackageVersioning, RestoreNugetPackages -Description "Compiling solution" { 
	exec { msbuild /nologo /verbosity:minimal $SolutionFilePath /p:Configuration=Release /p:VSToolsPath="$SrcDir\Packages\MSBuild.Microsoft.VisualStudio.Web.targets.11.0.2.1\tools\VSToolsPath" }
}

task RunTests -depends Compile -Description "Running unit tests" {
	$xunitRunner = "$SrcDir\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"
	Get-ChildItem $SrcDir -Recurse -Include *Specs.csproj | ForEach-Object {
		$project = $_.BaseName
		if(!(Test-Path $ReportsDir\xUnit\$project)) {
			New-Item $ReportsDir\xUnit\$project -Type Directory
		}
        
		exec { . $xunitRunner "$SrcDir\$project\bin\Release\$project.dll" /html "$ReportsDir\xUnit\$project\index.html" }
	}
}

task CopyFiles -depends Compile {
	@('LICENSE', 'README.md', 'VERIFICATION.txt') | Foreach-Object {
        Copy-Item -Path "$BaseDirectory\$_" -Destination $BaseDirectory\Package\Output
	}
}

task BuildZip -depends CopyFiles {
	TeamCity-Block "Zipping up the binaries" {
		$assembly = Get-ChildItem -Path $BaseDirectory\Package\Output -Filter Beacon.exe -Recurse | Select-Object -first 1
				
		$versionNumber = $assembly.VersionInfo.FileVersion

		& $7zip a -r "$BaseDirectory\Package\Beacon.$versionNumber.zip" "$BaseDirectory\Package\Output\*" -y
	}
}

task CreateChocoPackages -depends BuildZip -Description "Creating Chocolatey package" {
	if (!$env:ChocolateyInstall) {
		Write-Host "Installing Chocolatey"
		Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1')) 
  	}

	exec { 
        $lastcd = $PWD;
		Set-Location $PackageDirectory
		
		choco pack Beacon.nuspec
		
		Set-Location $lastcd;
    }
}