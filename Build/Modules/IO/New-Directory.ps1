function New-Directory
{
	Param(
	[parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
	[string] $path
	)
	
	mkdir $path -ErrorAction SilentlyContinue | out-null
}