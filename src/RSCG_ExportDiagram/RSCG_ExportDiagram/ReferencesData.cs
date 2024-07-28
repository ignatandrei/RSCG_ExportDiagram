
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
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
        var isStatic = symbol.IsStatic;
        if (isStatic)
        {
            return symbol.ContainingAssembly.Name + "." +  symbol.Name;
        }
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

    public GenerateText(ExternalReferencesType externalReferencesType)
    {
        this.externalReferencesType = externalReferencesType;
    }

    public string GenerateClass()
    {
        var methods = "";
        foreach (var externalReferencesMethod in externalReferencesType.externalReferencesMethod)
        {
            var method = $@"
// Method {externalReferencesMethod.methodType.Name} has following external references
// {string.Join("\r\n"+"//", externalReferencesMethod.externalReferences.Select(x => x.FullName()).ToArray())}
";
            methods+="\r\n"+ method;
        }

        var str = $@"
public class {externalReferencesType.classType.Name}_References
{{
    public {externalReferencesType.classType.Name}_References()
{{
     {methods}
}}
}}
";
        return str;
    }
}
