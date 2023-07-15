[CmdletBinding()]
param (
  [Parameter()]
  [string]
  $Action
)

Function install {
  bundle install
}
Function serve {
  bundle exec jekyll serve --livereload
}

switch ($Action) {
  "start" {
    install
    serve
  }
  "serve" { serve }
  "install" { install }
  Default {
    Write-Host "Specify an action:
    start     Run the jekyll site locally
    serve     Run the jekyll site locally without install first
    install   Install the jekyll dependencies
    "
  }
}