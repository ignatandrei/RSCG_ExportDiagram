using System.Linq;
using System.Text;
using System.Text.Json;

namespace RSCG_ExportDiagram;
public class ExportAssembly
{
    public string AssemblyName { get; set; } = "";
    public ExportClass[] ClassesWithExternalReferences { get; set; } = [];

    public string ExportJSON()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
    public string ExportMermaid()
    {
        var assemblyReferences = this.ClassesWithExternalReferences
            .SelectMany(it => it.MethodsWithExternalReferences)
            .SelectMany(it => it.References)
            .GroupBy(it => it.AssemblyName)
            .ToDictionary(it=>it.Key,it=>it.ToArray());
        
        
       var sb = new StringBuilder();
        sb.AppendLine($"# {AssemblyName}");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("flowchart TD");//TB?
        sb.AppendLine($"subgraph {AssemblyName}");


        foreach (var expClass in this.ClassesWithExternalReferences)
        {
            //main assembly
            sb.AppendLine($"subgraph {expClass.ClassName}");
            foreach(var met in expClass.MethodsWithExternalReferences)
            {
                sb.AppendLine($"{met.MethodName}");
            }
            sb.AppendLine($"%% end of subgraph {expClass.ClassName}");
            sb.AppendLine("end");

            foreach (var item in assemblyReferences.Keys)
            {
                sb.AppendLine($"subgraph {item}");

                var methods = assemblyReferences[item]
                    .Select(it => it.FullName)                    
                    .Distinct()
                    .ToArray();
                foreach (var met in methods)
                {
                    var lastDot = met.LastIndexOf('.');
                    var nameMethod = met.Substring(lastDot+ 1);  
                    sb.AppendLine($"{met}[{nameMethod}]");
                }

                sb.AppendLine($"%% end of subgraph {expClass.ClassName}");
                sb.AppendLine("end");

            }


        }
        sb.AppendLine($"%% end of subgraph {AssemblyName}");
        sb.AppendLine($"end");
        sb.AppendLine("```");

        return sb.ToString();
    }
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

