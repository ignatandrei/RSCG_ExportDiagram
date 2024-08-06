using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace RSCG_ExportDiagram
{
    enum CsprojDecl
    {
        None,
        JSONFolder,
        projectDir,
        projectName,
        excludeData
    }
    [Generator]
    public class GeneratorDiagram : IIncrementalGenerator
    {
        private bool IsFromAnotherAssembly(ITypeSymbol typeSymbol, IAssemblySymbol containingAssembly)
        {
            var ret= !SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingAssembly, containingAssembly);
            var ass=typeSymbol.ContainingAssembly;
            return ret;
        }
        private string[] NameReferencesProject(string csprojPath)
        {
            List<string> ret = new();
            XDocument csproj = XDocument.Load(csprojPath);

            var projectReferences = csproj.Descendants("ProjectReference")
                                          .Select(pr => pr.Attribute("Include")?.Value)
                                          .Where(include => !string.IsNullOrEmpty(include))  
                                          .Select(it=>it!)
                                          .ToArray()??[];

            projectReferences = projectReferences
                .Select(it => it.Split('\\', '/'))
                .Select(it => it.Last())
                .ToArray();

            
            return projectReferences ;
        }
        

    
    private bool ShouldNotConsider(ISymbol typeSymbol)
        {
            if(typeSymbol is IFieldSymbol field)
            {
                typeSymbol = field.ContainingType;
            }
            if(typeSymbol is IMethodSymbol methodSymbol)
            {
                if(methodSymbol.MethodKind == MethodKind.BuiltinOperator)
                    return true;
                typeSymbol = methodSymbol.ContainingType;
            }
            var displa = typeSymbol.Name;
            //if (displa.Contains("op_"))
            //{
            //    return false;
            //}
            var baseTypes = new[]
            {
                "System.DateTimeOffset",
                "System.DateTime",
                "System.Int32",
                "System.String",
                "System.Double",
                "System.Boolean",
                "System.Char",
                "System.Byte",
                "System.SByte",
                "System.Int16",
                "System.UInt16",
                "System.UInt32",
                "System.Int64",
                "System.UInt64",
                "System.Single",
                "System.Decimal",
                "System.Object",

                "DateTimeOffset",
                "DateTime",
                "Int32",
                "String",
                "Double",
                "Boolean",
                "Char",
                "Byte",
                "SByte",
                "Int16",
                "UInt16",
                "UInt32",
                "Int64",
                "UInt64",
                "Single",
                "Decimal",
                "Object",
                "string",
            };
            var res=
                baseTypes.Contains(typeSymbol.ToDisplayString())
                ||
                baseTypes.Contains(typeSymbol.Name)

                ;
            return res;
        }
        private T IsImplementationOfInterfaceMethod<T>(T methodSymbol)
            where T : ISymbol
        {
            
            var containingType = methodSymbol.ContainingType;
            foreach (var interfaceType in containingType.AllInterfaces)
            {
                foreach (var interfaceMember in interfaceType.GetMembers().OfType<T>())
                {
                    var implementation = containingType.FindImplementationForInterfaceMember(interfaceMember);
                    if (SymbolEqualityComparer.Default.Equals(implementation, methodSymbol))
                    {
                        return interfaceMember;
                    }
                }
            }
            return default(T);
        }
        private ISymbol[] MethodBodyReferencesOtherAssembly(IMethodSymbol methodSymbol, SemanticModel semanticModel, IAssemblySymbol currentAssembly)
        {
            var methodSyntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as BaseMethodDeclarationSyntax;
            if (methodSyntax == null)
                return [];

            var methodBody = methodSyntax.Body;
            if (methodBody == null)
                return [];
            List<ISymbol> references = new();

            foreach (var descendantNode in methodBody.DescendantNodes())
            {
                SymbolInfo symbolInfo;
                try
                {
                    symbolInfo = semanticModel.GetSymbolInfo(descendantNode);
                }
                catch (Exception)
                {
                    continue;
                }
                var referencedSymbol = symbolInfo.Symbol;
                if (referencedSymbol == null)
                    continue;
                if(referencedSymbol is ITypeSymbol typeSymbol)
                {
                    if (!ShouldNotConsider(typeSymbol))
                    {
                        if(IsFromAnotherAssembly(typeSymbol, currentAssembly))
                        {
                            references.Add(referencedSymbol);
                            continue;
                        }
                    }
                        
                }
                else if (!SymbolEqualityComparer.Default.Equals(referencedSymbol.ContainingAssembly, currentAssembly))
                {
                    references.Add(referencedSymbol);
                    continue;
                }
            }

            return references.ToArray();
        }
        public void Initialize(IncrementalGeneratorInitializationContext cnt)
        {

            var dataFromCsproj = cnt.AnalyzerConfigOptionsProvider.SelectMany(
                (it, _) =>
                {
                    Dictionary<CsprojDecl, string> map = new ();
                    
                    if(it.GlobalOptions.TryGetValue("build_property.RSCG_ExportDiagram_OutputFolder", out var value))
                    {
                        map.Add(CsprojDecl.JSONFolder, value);
                    }

                    if (it.GlobalOptions.TryGetValue("build_property.projectDir", out var value1))
                    {
                        map.Add(CsprojDecl.projectDir, value1);
                    }
                    if (it.GlobalOptions.TryGetValue("build_property.rootnamespace", out var value2))
                    {
                        map.Add(CsprojDecl.projectName, value2);
                    }
                    if (it.GlobalOptions.TryGetValue("build_property.RSCG_ExportDiagram_Exclude", out var value3))
                    {
                        map.Add(CsprojDecl.excludeData, value3);
                    }
                    return map.ToArray();
                    
                }
                ).Collect(); 
            ;
            var assemblyNameProv = cnt.CompilationProvider
            .Select((compilation, _) => {

                return compilation.AssemblyName;
                })
            ;
            
            var classToImplementProv = 
                cnt.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is BaseTypeDeclarationSyntax,
                transform: (context, _) => 
                    (context.SemanticModel ,context.SemanticModel.GetDeclaredSymbol(context.Node)))
            .Collect();

            var data =
                assemblyNameProv
                .Combine(classToImplementProv)
                .Combine(dataFromCsproj);
                
                ;
            cnt.RegisterSourceOutput(data, (context, AllData) =>
            {
                var (compound, csprojDecl) = AllData;
                var (assemblyName, classToImplement) = compound;
                var additionalExport = csprojDecl.ToArray().Distinct().ToArray();
                List<ExternalReferencesType> externalReferencesTypes = [];

                var projDir = csprojDecl.First(it => it.Key == CsprojDecl.projectDir).Value;
                var nameProject = csprojDecl.First(it => it.Key == CsprojDecl.projectName).Value;
                var fullNameProject = Path.Combine(projDir, nameProject + ".csproj");
                var refProjects = NameReferencesProject(fullNameProject);
                refProjects = refProjects.Select(it => it.Replace(".csproj","")).ToArray();

                foreach (var(sm, item) in classToImplement)
                {
                    
                    if (!(item is INamedTypeSymbol namedTypeSymbol))
                        continue;

                    var members = namedTypeSymbol.GetMembers();
                    var currentAssembly = namedTypeSymbol.ContainingAssembly;
                    List<ExternalReferencesForMethod> referencesMethod = [];
                    foreach (var member in members)
                    {
                        if (member is IPropertySymbol propertySymbol)
                        {
                            // for the moment, no property
                            continue;
                            //VerifyProperty(currentAssembly, propertySymbol);
                        }
                        else if (member is IMethodSymbol methodSymbol)
                        {
                            if(methodSymbol.IsImplicitlyDeclared)
                                continue;
                            var props = VerifyMethod(sm, currentAssembly, methodSymbol);
                            if (props?.Length > 0)
                            {
                                props = props
                                .Where(it=>it.ContainingAssembly != null)
                                .Where(it => refProjects.Contains(it.ContainingAssembly.Name))
                                .ToArray();
                                //foreach(var prop in props)
                                //{
                                //    var x= prop.ContainingAssembly.Name.ToString();
                                //    bool isProject = refProjects.Contains(x);
                                //    isProject=!isProject;
                                //}   
                                referencesMethod.Add(new ExternalReferencesForMethod(methodSymbol, props));
                            }
                        }

                    }
                    //create references
                    if (referencesMethod.Count > 0)
                    {
                        ExternalReferencesType externalReferencesType = new(namedTypeSymbol);
                        externalReferencesType.externalReferencesMethod= referencesMethod.ToArray();
                        externalReferencesTypes.Add(externalReferencesType);
                    }
                    

                }
                string[] excludeArr = [];
                var strExc = csprojDecl
                .Where(it => it.Key == CsprojDecl.excludeData)
                .ToArray();
                if (strExc.Length == 1)
                {
                    excludeArr = strExc[0].Value.Split(',');
                    excludeArr ??= [];
                }

                if (externalReferencesTypes.Count > 0)
                {
                    var nr=0;
                    foreach (var item in externalReferencesTypes)
                    {
                        nr++;
                        GenerateText generateText = new(item,nr, additionalExport);
                        var name = item.classType.ContainingAssembly.Name + "." + item.classType.Name;
                        name=name.Replace(".", "_")+"_"+nr;
                        var text = generateText.GenerateClass();
                        context.AddSource($"{name}_gen.cs", text);

                    }
                    var folder = csprojDecl
    .Where(it => it.Key == CsprojDecl.JSONFolder).ToArray();
                    if (folder.Length == 1)
                    {
                        var path = folder[0].Value;
                        if (!Path.IsPathRooted(path))
                        {
                            path = Path.Combine(projDir, path);
                        }
                        var fileNameJSON = Path.Combine(path, nameProject + ".json");
                        var fileNameMermaid = Path.Combine(path, nameProject + ".md");
                        var fileNameHTML = Path.Combine(path, nameProject + ".html");
                        List<ExportClass> exportClasses = new ();
                        nr=0;
                        foreach (var item in externalReferencesTypes)
                        {
                            nr++;
                            GenerateText generateText = new(item, nr, additionalExport);
                            var Json = generateText.GenerateObjectsToExport(excludeArr);
                            if(Json != null) exportClasses.Add(Json);
                        }
                        if (exportClasses.Count > 0)
                        {
                            JsonSerializerOptions options = new()
                            {
                                WriteIndented = true
                            };
                            ExportAssembly exAss =new();
                            exAss.AssemblyName = assemblyName??"";
                            exAss.ClassesWithExternalReferences = 
                                exportClasses
                                .Distinct(new Eq<ExportClass>((x, y) => x.ClassName == y.ClassName))
                                .OrderBy(it => it.ClassName)
                                .ToArray();
                            File.WriteAllText(fileNameJSON, exAss.ExportJSON());
                            File.WriteAllText(fileNameMermaid, exAss.ExportMermaid());
                            File.WriteAllText(fileNameHTML, exAss.ExportHTML());

                        }

                    }


                }
            });
            
        }

        private ISymbol[] VerifyMethod(SemanticModel sm, IAssemblySymbol currentAssembly, IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind == MethodKind.PropertyGet)
                return [];
            if (methodSymbol.MethodKind == MethodKind.PropertySet)
                return [];
            List<ISymbol> ret = new();
            var interfaceMethod = IsImplementationOfInterfaceMethod(methodSymbol);
            if (interfaceMethod != null)
            {
                if (!SymbolEqualityComparer.Default.Equals(interfaceMethod.ContainingAssembly, currentAssembly))
                {
                    ret.Add(interfaceMethod);
                }
            }
            else
            {

                if (methodSymbol.ReturnType.Name != "Void")
                {
                    if (!ShouldNotConsider(methodSymbol.ReturnType))
                    {
                        if (IsFromAnotherAssembly(methodSymbol.ReturnType, currentAssembly))
                        {
                            ret.Add(methodSymbol.ReturnType);
                        }
                    }

                }
            }
            var paramsAnother = methodSymbol.Parameters
                .Where(it => !ShouldNotConsider(it.Type))
                .Where(p => IsFromAnotherAssembly(p.Type, currentAssembly))
                .ToArray();

            if (paramsAnother.Length > 0)
            {
                ret.AddRange(paramsAnother);
                //var x = "another assemblu";
            }
            var otherass = (MethodBodyReferencesOtherAssembly(methodSymbol, sm, currentAssembly));
            if (otherass.Length > 0)
            {
                otherass= otherass.Where(it =>!ShouldNotConsider(it)).ToArray();
                if(otherass.Length>0)ret.AddRange(otherass);
            }
            return ret.ToArray();
            
            
        }

        private void VerifyProperty(IAssemblySymbol currentAssembly, IPropertySymbol propertySymbol)
        {
            var interfaceMethod = IsImplementationOfInterfaceMethod(propertySymbol);
            if (interfaceMethod != null)
            {
                if (!SymbolEqualityComparer.Default.Equals(interfaceMethod.ContainingAssembly, currentAssembly))
                {
                    var x = "another assemblu";
                }
            }

            if (!ShouldNotConsider(propertySymbol.Type))
            {


                if (IsFromAnotherAssembly(propertySymbol.Type, currentAssembly))
                {
                    // Handle property with reference to another assembly
                    var x = "another assemblu";

                    // Add your logic here
                }
            }
        }
    }
}
