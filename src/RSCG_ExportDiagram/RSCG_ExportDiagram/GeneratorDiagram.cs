﻿using Microsoft.CodeAnalysis;
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
        projectName
    }
    [Generator]
    public class GeneratorDiagram : IIncrementalGenerator
    {
        private bool IsFromAnotherAssembly(ITypeSymbol typeSymbol, IAssemblySymbol containingAssembly)
        {
            return !SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingAssembly, containingAssembly);
        }
        private bool ShouldNotConsider(ISymbol typeSymbol)
        {
            if(typeSymbol is IMethodSymbol methodSymbol)
            {
                if(methodSymbol.MethodKind == MethodKind.BuiltinOperator)
                    return true;
                
            }
            var displa = typeSymbol.Name;
            if (displa.Contains("op_"))
            {
                return false;
            }
            var baseTypes = new[]
            {
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

            return baseTypes.Contains(typeSymbol.ToDisplayString());
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
            var methodSyntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
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
                    return map.ToArray();
                    
                }
                ).Collect(); 
            ;
            var assemblyNameProv = cnt.CompilationProvider
            .Select((compilation, _) => compilation.AssemblyName)
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
                            VerifyProperty(currentAssembly, propertySymbol);
                        }
                        else if (member is IMethodSymbol methodSymbol)
                        {
                            if(methodSymbol.IsImplicitlyDeclared)
                                continue;
                            var props = VerifyMethod(sm, currentAssembly, methodSymbol);
                            if (props?.Length > 0)
                            {
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


                if(externalReferencesTypes.Count > 0)
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
                            path = Path.Combine(csprojDecl.First(it => it.Key == CsprojDecl.projectDir).Value, path);
                        }
                        var nameProject = csprojDecl.First(it => it.Key == CsprojDecl.projectName).Value;
                        var fileNameJSON = Path.Combine(path, nameProject + ".json");
                        List<ExportClass> exportClasses = new ();
                        nr=0;
                        foreach (var item in externalReferencesTypes)
                        {
                            nr++;
                            GenerateText generateText = new(item, nr, additionalExport);
                            var Json = generateText.GenerateObjectsToExport();
                            exportClasses.Add(Json);
                        }
                        if (exportClasses.Count > 0)
                        {
                            JsonSerializerOptions options = new()
                            {
                                WriteIndented = true
                            };
                            ExportAssembly exAss =new();
                            exAss.AssemblyName = assemblyName??"";
                            exAss.ClassesWithExternalReferences = exportClasses.ToArray();
                            var data = JsonSerializer.Serialize(exAss, options);
                            File.WriteAllText(fileNameJSON, data);
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
                ret.AddRange(otherass);
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
