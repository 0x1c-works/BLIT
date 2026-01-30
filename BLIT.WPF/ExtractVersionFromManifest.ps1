# PowerShell script to extract version from Package.appxmanifest and update Version.props
# This script is called during the build process to synchronize version numbers

param(
    [string]$ManifestPath = "",
    [string]$VersionPropsPath = ""
)

# If paths not provided, derive them from script location
if (-not $ManifestPath) {
    $ManifestPath = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent)) "BLIT.Package\Package.appxmanifest"
}

if (-not $VersionPropsPath) {
    $VersionPropsPath = Join-Path $PSScriptRoot "Properties\Version.props"
}

Write-Host "Extracting version from: $ManifestPath"
Write-Host "Updating version file: $VersionPropsPath"

# Read the appxmanifest file
if (-not (Test-Path $ManifestPath)) {
    Write-Error "Package.appxmanifest not found at: $ManifestPath"
    exit 1
}

try {
    [xml]$manifest = Get-Content $ManifestPath -Encoding UTF8
    
    # Extract version from Identity element
    # The namespace makes this tricky, so we use XPath with namespace
    $ns = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
    $ns.AddNamespace('default', 'http://schemas.microsoft.com/appx/manifest/foundation/windows10')
    
    $identityNode = $manifest.SelectSingleNode('//default:Identity', $ns)
    
    if (-not $identityNode) {
        Write-Error "Could not find Identity element in manifest"
        exit 1
    }
    
    $versionString = $identityNode.Version
    
    if (-not $versionString) {
        Write-Error "Version attribute not found in Identity element"
        exit 1
    }
    
    Write-Host "Found version in manifest: $versionString"
    
    # Parse version (format: x.y.z.w)
    $versionParts = $versionString -split '\.'
    if ($versionParts.Count -ne 4) {
        Write-Error "Invalid version format in manifest: $versionString (expected x.y.z.w)"
        exit 1
    }
    
    $majorVersion = $versionParts[0]
    $minorVersion = $versionParts[1]
    $patchVersion = $versionParts[2]
    $buildNumber = $versionParts[3]
    
    Write-Host "Parsed version - Major: $majorVersion, Minor: $minorVersion, Patch: $patchVersion, Build: $buildNumber"
    
    # Build the XML content with proper formatting
    $versionPropsContent = @"
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <!-- Version number components definition -->
    <MajorVersion>$majorVersion</MajorVersion>
    <MinorVersion>$minorVersion</MinorVersion>
    <PatchVersion>$patchVersion</PatchVersion>
    <BuildNumber>$buildNumber</BuildNumber>

    <!-- Combined version number (3 parts) -->
    <Version>`$(MajorVersion).`$(MinorVersion).`$(PatchVersion)</Version>

    <!-- Combined version number (4 parts) - for Assembly and UI display -->
    <AssemblyVersion>`$(MajorVersion).`$(MinorVersion).`$(PatchVersion).`$(BuildNumber)</AssemblyVersion>
    <FileVersion>`$(MajorVersion).`$(MinorVersion).`$(PatchVersion).`$(BuildNumber)</FileVersion>
    <InformationalVersion>`$(MajorVersion).`$(MinorVersion).`$(PatchVersion).`$(BuildNumber)</InformationalVersion>
  </PropertyGroup>
</Project>
"@
    
    # Write the updated Version.props with UTF8 No BOM encoding
    [System.IO.File]::WriteAllText($VersionPropsPath, $versionPropsContent, [System.Text.Encoding]::UTF8)
    
    Write-Host "Successfully updated $VersionPropsPath with version: $versionString"
    exit 0
}
catch {
    Write-Error "Error processing manifest: $_"
    exit 1
}
