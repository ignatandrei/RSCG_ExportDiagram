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
            .ToDictionary(it => it.Key, it => it.ToArray());


        var sb = new StringBuilder();
        sb.AppendLine($"# {AssemblyName}");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("flowchart TD");//TB?
        sb.AppendLine($"subgraph {AssemblyName}");
        sb.AppendLine($"style {AssemblyName} fill:#f9f,stroke:#333,stroke-width:4px");

        foreach (var expClass in this.ClassesWithExternalReferences)
        {
            //main assembly
            sb.AppendLine($"subgraph {expClass.ClassName}");
            foreach (var met in expClass.MethodsWithExternalReferences)
            {
                //TODO: add full method name
                sb.AppendLine($"{expClass.ClassName}.{met.MethodName}[{met.MethodName}]");
            }
            sb.AppendLine($"%% end of subgraph {expClass.ClassName}");
            sb.AppendLine("end");
        }
        sb.AppendLine($"%% end of subgraph {AssemblyName}");
        sb.AppendLine($"end");

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
                lastDot = met.LastIndexOf('.', lastDot - 1);
                var nameMethod = met.Substring(lastDot + 1);
                sb.AppendLine($"{met}[{nameMethod}]");
            }

            sb.AppendLine($"%% end of subgraph {item}");
            sb.AppendLine($"end");

        }


        foreach (var item in this.ClassesWithExternalReferences)
        {
            foreach (var met in item.MethodsWithExternalReferences)
            {
                string nameMethod = met.MethodName; 
                foreach (var refItem in met.References)
                {
                    sb.AppendLine($"{item.ClassName}.{nameMethod} --> {refItem.FullName}");
                }
            }
        }
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

