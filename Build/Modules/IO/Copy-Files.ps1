function Copy-Files {
	Param(
		[parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
		[string] $source,
		[string] $destination,
		[alias("exclude")]
		[string[]] $excludeFiles=@(),
		[string[]] $excludeDirectories=@()
	)

    New-Directory $destination
		
    #Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)} 
	
	
	$arguments = @($source, $destination, "*.*", "/e")
	
	if(($excludeFiles -ne $null) -and ($excludeFiles.Length -gt 0)) {
		$arguments += "/xf"
		$arguments += $excludeFiles
	}
	
	if(($excludeDirectories -ne $null) -and ($excludeFiles.Length -gt 0)) {
		$arguments += "/xd"
		$arguments += $excludeDirectories
	}
	
	robocopy.exe $arguments | out-null
	
	Expect-ExitCode -expectedExitCode 0,1 -formatMessage { param($taskName) "Copy was not successful" }
}