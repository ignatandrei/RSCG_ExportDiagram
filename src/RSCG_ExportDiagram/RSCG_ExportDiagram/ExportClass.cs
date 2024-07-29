namespace RSCG_ExportDiagram;
public class ExportAssembly
{
    public string AssemblyName { get; set; } = "";
    public ExportClass[] ClassesWithExternalReferences { get; set; } = [];
}

public class ExportClass
{
    public string ClassName { get; set; } = "";
    public MethodswithexternalreferenceExport[] MethodsWithExternalReferences { get; set; } = [];
}

public class MethodswithexternalreferenceExport
{
    public string MethodName { get; set; } = "";
    public ExternalReferenceExport[] References { get; set; } = [];
}

public class ExternalReferenceExport
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string AssemblyName { get; set; } = "";
}

