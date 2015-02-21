$helpersPath = (Split-Path -parent $MyInvocation.MyCommand.Definition);

$functionPaths = Resolve-Path $helpersPath\IO\*.ps1

$functionPaths |
    Where-Object { -not ($_.ProviderPath.Contains(".Tests.")) } |
    ForEach-Object { . $_.ProviderPath }
	
$functions = (Get-ChildItem $functionPaths).BaseName

Export-ModuleMember -Function $functions