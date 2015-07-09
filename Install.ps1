$proj = (Get-Content .\GitReview\GitReview.csproj) -as [Xml]
$url = $proj.Project.ProjectExtensions.VisualStudio.FlavorProperties.WebProjectProperties.IISUrl.TrimEnd('/') + '/new'
"Url: $url"

git config --global alias.cr "push $url @:refs/heads/source @{u}:refs/heads/destination"
