function Clean-Item {
	Param(
	[parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
	[string] $path
	)
	Process 
	{
		if(($path -ne $null) -and (test-path $path))
		{
			write-verbose ("Removing {0}" -f $path)
			remove-item -force -recurse $path | Out-Null
		}	
	}
}