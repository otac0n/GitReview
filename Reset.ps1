$webConfig = (Get-Content .\GitReview\Web.config) -as [Xml]
$db = ($webConfig.configuration.connectionStrings.add | ?{ $_.name -eq 'GitReview.Database' }).connectionString
$git = ($webConfig.configuration.appSettings.add | ?{ $_.key -eq 'GitPath' }).value
$repo = ($webConfig.configuration.appSettings.add | ?{ $_.key -eq 'RepositoryPath' }).value -replace '~', 'GitReview'
"Repo: $repo"
"DB: $db"
"Git: $git"
""

& $git init --bare $repo
& $git --git-dir=$repo branch | ?{ !($_ -match '^\*') } | %{ & $git --git-dir=$repo branch -D $_.TrimStart() }
& $git --git-dir=$repo reflog expire --expire=now --all
& $git --git-dir=$repo gc --aggressive --prune=now
& $git --git-dir=$repo repack -a -d -l

Push-Location .
Import-Module SqlPS -DisableNameChecking
Pop-Location
$sb = New-Object System.Data.Common.DbConnectionStringBuilder
$sb.set_ConnectionString($db)
Invoke-Sqlcmd -InputFile 'ResetDb.sql' -Database $sb['initial catalog'] -ServerInstance $sb['data source']
