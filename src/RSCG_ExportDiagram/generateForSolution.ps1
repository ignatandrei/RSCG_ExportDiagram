function ProcessCsproj {
  param (
      [string]$project,
      [string]$folderOutput
  )

$version = "2024.904.427"
#$folderOutput= ".."
$newNode = [xml]@"
<MainData>
<ItemGroup>
  <CompilerVisibleProperty Include="RSCG_ExportDiagram_OutputFolder" />
  <CompilerVisibleProperty Include="RSCG_ExportDiagram_Exclude" />
</ItemGroup>

<PropertyGroup>
  <RSCG_ExportDiagram_OutputFolder>$folderOutput</RSCG_ExportDiagram_OutputFolder>
  <RSCG_ExportDiagram_Exclude></RSCG_ExportDiagram_Exclude>
</PropertyGroup>
</MainData>
"@




# Write-Host $newNode.MainData.InnerXml
$backFile =$project + ".bak"
Copy-Item $project $backFile 
dotnet add $project package RSCG_ExportDiagram -v $version 

$proj = [xml](Get-Content $project)

$foundNode = $proj.Project
#Write-Host $proj.Project.InnerXml

$ItemGroup = $proj.ImportNode($newNode.DocumentElement.ItemGroup,$true)
$proj.Project.PrependChild($ItemGroup) 
$proj.DocumentElement.AppendChild($ItemGroup )


$PropertyGroup = $proj.ImportNode($newNode.DocumentElement.PropertyGroup,$true)
$proj.Project.PrependChild($PropertyGroup) 
$proj.DocumentElement.AppendChild($PropertyGroup)
$proj.Save($project) 
dotnet build
# pause
Copy-Item $backFile  $project -Force
Remove-Item $backFile  -Force

}

$solution  = gci *.sln | %{ $_.FullName}
$folderSolution = Split-Path $solution
# Write-Host $folderSolution
Get-Content $solution |
  Select-String 'Project\(' |
    ForEach-Object {
      $projectParts = $_ -Split '[,=]' | ForEach-Object { $_.Trim('[ "{}]') };
      # New-Object PSObject -Property @{
      #   Name = $projectParts[1];
      #   File = $projectParts[2];
      #   Guid = $projectParts[3]
      # }
      if ($projectParts[2] -match '.csproj$'){	
        $fileProject =Join-Path  $folderSolution $projectParts[2]
        Write-Host $fileProject
        ProcessCsproj -project $fileProject -folderOutput $folderSolution
      }
    }

