
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
namespace RSCG_ExportDiagram;
internal class ExternalReference
{
    public readonly ISymbol symbol;

    public ExternalReference(ISymbol symbol)
    {
        if(symbol.ContainingType == null)
        {
            throw new ArgumentException("symbol.ContainingType is null");
        }
        this.symbol = symbol;

    }
    public string FullName()
    {
        //var isStatic = symbol.IsStatic;
        //if (isStatic)
        //{
        //    return symbol.ContainingAssembly.Name + "." +  symbol.Name;
        //}
       return symbol.ContainingAssembly.Name + "." + symbol.ContainingType!.Name + "." + symbol.Name;
    }
}
internal class ExternalReferencesForMethod
{
    public readonly IMethodSymbol methodType;
    public readonly ExternalReference[] externalReferences;

    public ExternalReferencesForMethod(IMethodSymbol methodType, ISymbol[] externalReferences)
    {
        this.methodType = methodType;
        //eliminating duplicates
        var ext= externalReferences.Where(it=>it.ContainingType !=null).ToArray();
        ext =ext.GroupBy(x=>x,SymbolEqualityComparer.Default).Select(x=>x.First()).ToArray();
        this.externalReferences = ext.Select(it=>new ExternalReference(it)).ToArray();
    }
    public bool ShouldEliminate()
    {
        return externalReferences.Length == 0;
    }
    
}

internal class ExternalReferencesType {
    public readonly INamedTypeSymbol classType;
    public ExternalReferencesForMethod[] externalReferencesMethod;

    public ExternalReferencesType(INamedTypeSymbol classType)
    {
        this.classType = classType;
        externalReferencesMethod = [];
        
    }
}
internal class GenerateText
{
    private readonly ExternalReferencesType externalReferencesType;
    private readonly int nr;
    private readonly KeyValuePair<CsprojDecl, string>[] csprojDecl;
    
    public GenerateText(ExternalReferencesType externalReferencesType, int nr, KeyValuePair<CsprojDecl, string>[] csprojDecl)
    {
        this.externalReferencesType = externalReferencesType;
        this.nr = nr;
        this.csprojDecl = csprojDecl;
    }

    

    public string GenerateClass()
    {
        var methods = "";
        var methodsToWrite=
            externalReferencesType.externalReferencesMethod
            .Where(x=>!x.ShouldEliminate())
            .ToArray();
        foreach (var externalReferencesMethod in methodsToWrite)
        {
            var method = $@"
// Method {externalReferencesMethod.methodType.Name} has following external references
// {string.Join("\r\n"+"//", externalReferencesMethod.externalReferences.Select(x => x.FullName()).ToArray())}
";
            methods+="\r\n"+ method;
        }

        var str = $@"
//{string.Join("\r\n//", csprojDecl.Select(it=>$"{it.Key}={it.Value}"))}
file class {externalReferencesType.classType.Name}_References_{nr}
{{
    public {externalReferencesType.classType.Name}_References_{nr}()
{{
     {methods}
}}
}}
";
        return str;
    }

    internal ExportClass GenerateObjectsToExport(string[] exclude)
    {
        exclude = exclude.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        ExportClass obj = new()
        {            
            ClassName = externalReferencesType.classType.Name,
            MethodsWithExternalReferences = externalReferencesType.externalReferencesMethod
            .Select(x => new MethodswithexternalreferenceExport()
            {
                MethodName = x.methodType.Name,
                References = x.externalReferences.Select(x => new ExternalReferenceExport()
                {
                    Name = x.symbol.Name,
                    FullName = x.FullName(),
                    TypeName = x.symbol.ContainingType.Name,
                    AssemblyName = x.symbol.ContainingAssembly.Name
                })
                .ToArray()
            })
                .ToArray()
        };
        obj.EliminateDuplicates();
        
        if (exclude.Length > 0)
        {
            foreach (var met in obj.MethodsWithExternalReferences)
            {
                met.References = met.References
                    .Where(extRef => !exclude.Any(it => extRef.FullName.Contains(it)))
                    .ToArray();
            }
            obj.EliminateDuplicates();
        }
        return obj;
    }
}
