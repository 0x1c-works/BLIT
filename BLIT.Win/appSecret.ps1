param (
  [Parameter(Mandatory=$true, HelpMessage="Whether is pre or post build")]
  [ValidateSet("pre", "post")]
  [String]
  $phase
)

$filename=".\Helpers\AppCenterHelper.cs";
$regex='(Secret = ").+(";)'

function doReplace($value) {
  $newContent = (Get-Content $filename) -replace $regex, "`$1$value`$2"
  Set-Content -Path $filename -Value $newContent
}

function preBuild {
  doReplace $env:BLIT_APP_CENTER_SECRET
}

function postBuild {
  doReplace "##TO BE REPLACED AT BUILD TIME##"
}

switch ($phase) {
  "pre" { preBuild }
  "post" { postBuild }
  default { throw "Invalid phase: must be 'pre' or 'post'" }
}
