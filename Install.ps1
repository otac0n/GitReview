$proj = (Get-Content .\GitReview\GitReview.csproj) -as [Xml]
$url = $proj.Project.ProjectExtensions.VisualStudio.FlavorProperties.WebProjectProperties.IISUrl.TrimEnd('/') + '/new'
"Url: $url"

$command = "git push $url " + '${1:-HEAD}:refs/heads/source ${2:-$1@{u\}}:refs/heads/destination'
$quoted = "'" + ($command -replace '\$', '\$') + "'"
git config --global alias.cr "`"!sh -c '$command' --`""
Write-Host -n "Git alias.cr: "
git config --get alias.cr
