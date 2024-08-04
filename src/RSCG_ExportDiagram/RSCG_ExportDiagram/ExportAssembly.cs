namespace RSCG_ExportDiagram;

public class ExportAssembly
{
    public string AssemblyName { get; set; } = "";
    public ExportClass[] ClassesWithExternalReferences { get; set; } = [];

    public string ExportJSON()
    {
        //return "asda";
        try
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
        catch(Exception ex)
        {
            return ex.Message;
        }
    }
    public string ExportMermaid()
    {
        Templates.DisplayMarkdownWithMermaid template = new(this);
        return template.Render();

        //var assemblyReferences = this.ClassesWithExternalReferences
        //    .SelectMany(it => it.MethodsWithExternalReferences)
        //    .SelectMany(it => it.References)
        //    .GroupBy(it => it.AssemblyName)
        //    .ToDictionary(it => it.Key, it => it.ToArray());


        //var sb = new StringBuilder();
        //sb.AppendLine($"# {AssemblyName}");
        //sb.AppendLine();
        //sb.AppendLine("```mermaid");
        //sb.AppendLine("flowchart TD");//TB?
        //sb.AppendLine($"%% start main assembly {AssemblyName}");
        //sb.AppendLine($"subgraph {AssemblyName}");
        //sb.AppendLine($"style {AssemblyName} fill:#f9f,stroke:#333,stroke-width:4px");

        //foreach (var expClass in this.ClassesWithExternalReferences)
        //{
        //    sb.AppendLine($"%% start class {expClass.ClassName}");
        //    sb.AppendLine($"subgraph {expClass.ClassName}");
        //    foreach (var met in expClass.MethodsWithExternalReferences)
        //    {
        //        var methodName = met.MethodName;
        //        var lastDot = methodName.LastIndexOf('.');
        //        if(lastDot > 0)
        //            methodName = methodName.Substring(lastDot + 1);
        //        sb.AppendLine($"{expClass.ClassName}.{met.MethodName}[{methodName}]");
        //    }
        //    sb.AppendLine($"%% end of subgraph {expClass.ClassName}");
        //    sb.AppendLine("end");
        //}
        //sb.AppendLine($"%% end of subgraph {AssemblyName}");
        //sb.AppendLine($"end");

        //foreach (var item in assemblyReferences.Keys)
        //{
        //    sb.AppendLine($"%% start assembly {item}");
        //    sb.AppendLine($"subgraph {item}");
        //    var eq = new Eq<ExternalReferenceExport>((x, y) => x.FullName == y.FullName);
        //    var typeNames = assemblyReferences[item]
        //        //.Distinct(eq)
        //        .Select(it => it.TypeName)
        //        .Distinct()
        //        .ToArray();
        //    foreach (var typeName in typeNames)
        //    {
        //        sb.AppendLine($"%% start type {typeName}");
        //        sb.AppendLine($"subgraph {typeName}");

        //        var methods = assemblyReferences[item]
        //            .Where(it=>it.TypeName == typeName)
        //            .ToArray();
        //        methods = methods
        //            .Distinct(eq)
        //            .ToArray();
        //        foreach (var met in methods)
        //        {
        //            var nameMethod = met.FullName;
        //            sb.AppendLine($"{nameMethod}[{met.Name}]");
        //        }


        //        sb.AppendLine($"%% end of subgraph {typeName}");
        //        sb.AppendLine($"end");

        //    }

            
        //    sb.AppendLine($"%% end of subgraph {item}");
        //    sb.AppendLine($"end");

        //}


        //foreach (var item in this.ClassesWithExternalReferences)
        //{
        //    foreach (var met in item.MethodsWithExternalReferences)
        //    {
        //        string nameMethod = met.MethodName; 
        //        foreach (var refItem in met.References)
        //        {
        //            sb.AppendLine($"{item.ClassName}.{nameMethod} --> {refItem.FullName}");
        //        }
        //    }
        //}
        //sb.AppendLine("```");

        //return sb.ToString();
    }

    internal string ExportHTML()
    {
        Templates.DisplayVisJS template = new(this);
        return template.Render();

    }
}

