namespace RSCG_ExportDiagram;

public class ExportClass
{
    public string ClassName { get; set; } = "";
    public MethodswithexternalreferenceExport[] MethodsWithExternalReferences { get; set; } = [];
    public void EliminateDuplicates()
    {
        //eliminate duplicates in method names, even with different parameters
       var eq = new Eq<MethodswithexternalreferenceExport>((x, y) => x.MethodName == y.MethodName);
        MethodsWithExternalReferences= MethodsWithExternalReferences
            .Distinct(eq)
            .ToArray();
        MethodsWithExternalReferences = MethodsWithExternalReferences
                .Where(x => x.References.Length > 0)
                .ToArray();

    }
    public string[] ExternalClasses()
    {
       return  this.MethodsWithExternalReferences
            .SelectMany(it => it.References)
            //.Distinct(new Eq<ExternalReferenceExport>(
            //    (x, y) => 
            //    x.AssemblyName+"."+x.TypeName == y.AssemblyName+"."+y.TypeName))
            .Select(it=>it.FullClassName())
            .Distinct()
            .ToArray()??[];
            ;
    }
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

    public string FullClassName()
    {
        return AssemblyName + "." + TypeName;
    }
}

