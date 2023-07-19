param (
	[Parameter(Mandatory=$true, Position=0)]
	[string]$PublishDir
)

$ParentDir = $PublishDir | Split-Path -Parent
$ArchivePath = "$ParentDir\BLIT.dist.zip"

Write-Host "Archiving publish directory to $ArchivePath..."

Compress-Archive -Path $PublishDir -DestinationPath $ArchivePath -Force -CompressionLevel Optimal

