function Remove-Directory {
	Param(
	[parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
	[string] $path
	)
  rd $path -recurse -force -ErrorAction SilentlyContinue | out-null
}