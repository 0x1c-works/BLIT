[CmdletBinding()]
param (
  [Parameter()]
  [string]
  $Action
)

switch ($Action) {
  "start" { bundle exec jekyll serve }
  "install" { bundle install }
  Default {
    Write-Host "Specify an action:
    start     Run the jekyll site locally
    install   Install the jekyll dependencies
    "
  }
}