# RSCG_ExportDiagram
export diagram for external relations for a csproj   

## Install

Add to the csproj
    
```xml

<ItemGroup>
<PackageReference Include="RSCG_ExportDiagram" Version="2024.806.2104" OutputItemType="Analyzer" ReferenceOutputAssembly="false"   />
</ItemGroup>
<ItemGroup>
	<CompilerVisibleProperty Include="RSCG_ExportDiagram_OutputFolder" />
	<CompilerVisibleProperty Include="RSCG_ExportDiagram_Exclude" />
</ItemGroup>	
<PropertyGroup>
<RSCG_ExportDiagram_OutputFolder>..</RSCG_ExportDiagram_OutputFolder>
<RSCG_ExportDiagram_Exclude></RSCG_ExportDiagram_Exclude>
</PropertyGroup>

```


    And the diagram will be generated in the folder parent fo the .csproj file

    

