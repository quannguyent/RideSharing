dotnet ef dbcontext scaffold  "data source=LAPTOP-DK17N0RS\MSSQLSERVER2019;initial catalog=RideSharing;persist security info=True;Trusted_Connection=True;multipleactiveresultsets=True;" Microsoft.EntityFrameworkCore.SqlServer -c DataContext  -o Models -f --no-build --use-database-names --json
$content = Get-Content -Path 'Models\DataContext.cs' -Encoding UTF8
$content = $content -replace "using System;", "using System;using Thinktecture;"
$content = $content -replace "modelBuilder.Entity<ActionDAO>", "modelBuilder.ConfigureTempTable<long>();modelBuilder.ConfigureTempTable<Guid>();modelBuilder.Entity<ActionDAO>"
$content | Set-Content -Path "Models\DataContext.cs"  -Encoding UTF8