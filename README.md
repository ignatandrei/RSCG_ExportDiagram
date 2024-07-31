# RSCG_ExportDiagram
export diagram for external relations for a csproj   

## Install

Add to the csproj
    
```xml

<ItemGroup>
<PackageReference Include="RSCG_ExportDiagram" Version="2024.730.717" />
</ItemGroup>
<PropertyGroup>
<RSCG_ExportDiagram_OutputFolder>.</RSCG_ExportDiagram_OutputFolder>
<RSCG_ExportDiagram_Exclude>System.Runtime.Uri,System.Runtime.IList,System.Runtime.Object,System.Runtime.Exception,System.Runtime.Func,System.Runtime.String,System.Runtime.IDictionary,System.Collections,System.Console,System.Linq</RSCG_ExportDiagram_Exclude>
</PropertyGroup>

```


    And the diagram will be generated in the folder parent fo the .csproj file

    

