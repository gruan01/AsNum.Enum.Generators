using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AsNum.Enum.Generators;
/// <summary>
/// 
/// </summary>
[Generator(LanguageNames.CSharp)]
public class EnumGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 
    /// </summary>
    private static readonly Regex reg = new(@"\p{P}", RegexOptions.Compiled);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        var provider = context.CompilationProvider.Select((compilation, cts) =>
        {
            var ns = compilation.Assembly.Name;

            var enums = new Dictionary<string, string>();
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {

                var es = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<EnumDeclarationSyntax>();
                foreach (var e in es)
                {
                    var semanticModel = compilation.GetSemanticModel(e.SyntaxTree);
                    if (semanticModel.GetDeclaredSymbol(e) is not INamedTypeSymbol enumSymbol)
                        continue;

                    var symbol = semanticModel.GetDeclaredSymbol(e);
                    var symbolName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    //var symbolName = $"{symbol!.ContainingNamespace}.{symbol.Name}";

                    var memberAttribute = new Dictionary<string, string>();
                    var resources = new Dictionary<string, string>();

                    foreach (var member in enumSymbol.GetMembers())
                    {
                        if (member is not IFieldSymbol field || field.ConstantValue is null)
                            continue;

                        foreach (var attribute in member.GetAttributes())
                        {
                            if (attribute.AttributeClass is null || attribute.AttributeClass.Name != "DisplayAttribute")
                                continue;

                            foreach (var namedArgument in attribute.NamedArguments)
                            {
                                if (namedArgument.Key.Equals("Name", StringComparison.OrdinalIgnoreCase) &&
                                    namedArgument.Value.Value?.ToString() is { } dn)
                                {
                                    memberAttribute.Add(member.Name, dn);
                                    //break;
                                }

                                //var a = (INamedTypeSymbol)namedArgument.Value.Value;
                                //var b = a.OriginalDefinition;

                                if (namedArgument.Key.Equals("ResourceType", StringComparison.OrdinalIgnoreCase)
                                    && namedArgument.Value.Value is INamedTypeSymbol t
                                )
                                {
                                    resources.Add(member.Name, t.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                                }
                            }
                        }
                    }


                    var fileName = reg.Replace(symbolName, "_");

                    var sourceBuilder = new StringBuilder($@"using System;
namespace {ns}
{{

    /// <summary>
    /// 
    /// </summary>
    public static class {fileName}_EnumExtensions
    {{
        
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Resources.ResourceManager> resManagers = new();

        private static string Get(string key, Type resourceType, System.Globalization.CultureInfo culture = null)
        {{
            return resManagers.GetOrAdd(key, (k) => new System.Resources.ResourceManager(resourceType)).GetString(key, culture);
        }}
");

                    //ToStringFast
                    ToStringFast(sourceBuilder, symbolName, e);

                    //IsDefined enum
                    IsDefinedEnum(sourceBuilder, symbolName, e);

                    //IsDefined string
                    IsDefinedString(sourceBuilder, e, symbolName);

                    //ToDisplay string
                    ToDisplay(sourceBuilder, symbolName, e, memberAttribute, resources);

                    //GetValues
                    GetValuesFast(sourceBuilder, symbolName, e);

                    //GetNames
                    GetNamesFast(sourceBuilder, symbolName, e);

                    //GetLength
                    GetLengthFast(sourceBuilder, e);

                    sourceBuilder.Append(@"
    }
}
");
                    enums.Add($"{fileName}_EnumGenerator.g.cs", sourceBuilder.ToString());
                }
            }

            return enums;
        });

        context.RegisterSourceOutput(provider, (spc, datas) =>
        {
            foreach (var kv in datas)
            {
                spc.AddSource(kv.Key, kv.Value);
            }
        });
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBuilder"></param>
    /// <param name="symbolName"></param>
    /// <param name="e"></param>
    /// <param name="memberAttribute"></param>
    private static void ToDisplay(
            StringBuilder sourceBuilder,
            string symbolName,
            EnumDeclarationSyntax e,
            Dictionary<string, string> memberAttribute,
            Dictionary<string, string> resourceTypes
        )
    {
        sourceBuilder.Append($@"

        /// <summary>
        /// 
        /// </summary>
        public static string ToDisplayFast(this {symbolName} states, System.Globalization.CultureInfo culture = null)
        {{
            return states switch
            {{
");
        foreach (var member in e.Members)
        {
            var display = memberAttribute
                              .FirstOrDefault(r => r.Key.Equals(member.Identifier.ValueText, StringComparison.OrdinalIgnoreCase))
                              .Value ?? member.Identifier.ValueText;

            var res = resourceTypes
                              .FirstOrDefault(r => r.Key.Equals(member.Identifier.ValueText, StringComparison.OrdinalIgnoreCase))
                              .Value;

            if (res == null)
            {
                sourceBuilder.AppendLine($@"                {symbolName}.{member.Identifier.ValueText} => ""{display}"",");
            }
            else
            {
                sourceBuilder.AppendLine($@"                {symbolName}.{member.Identifier.ValueText} => Get(""{display}"", typeof({res}), culture) ?? ""{display}"",");
            }
        }

        sourceBuilder.Append(
            @"                _ => throw new ArgumentOutOfRangeException(nameof(states), states, null)
            };
        }");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBuilder"></param>
    /// <param name="e"></param>
    /// <param name="symbolName"></param>
    private static void IsDefinedString(StringBuilder sourceBuilder, EnumDeclarationSyntax e, string symbolName)
    {
        sourceBuilder.Append($@"

        /// <summary>
        /// 
        /// </summary>
        public static bool IsDefinedFast(string states)
        {{
            return states switch
            {{
");
        foreach (var member in e.Members.Select(x => x.Identifier.ValueText))
            sourceBuilder.AppendLine($@"                nameof({symbolName}.{member}) => true,");
        sourceBuilder.Append(
            @"                _ => false
            };
        }");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBuilder"></param>
    /// <param name="symbolName"></param>
    /// <param name="e"></param>
    private static void IsDefinedEnum(StringBuilder sourceBuilder, string symbolName, EnumDeclarationSyntax e)
    {
        sourceBuilder.Append($@"

        /// <summary>
        /// 
        /// </summary>
        public static bool IsDefinedFast({symbolName} states)
        {{
            return states switch
            {{
");
        foreach (var member in e.Members.Select(x => x.Identifier.ValueText))
            sourceBuilder.AppendLine($@"                {symbolName}.{member} => true,");
        sourceBuilder.Append(
            @"                _ => false
            };
        }");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBuilder"></param>
    /// <param name="symbolName"></param>
    /// <param name="e"></param>
    private static void ToStringFast(StringBuilder sourceBuilder, string symbolName, EnumDeclarationSyntax e)
    {
        sourceBuilder.Append($@"

        /// <summary>
        /// 
        /// </summary>
        public static string ToStringFast(this {symbolName} states)
        {{
            return states switch
            {{
");
        foreach (var member in e.Members.Select(x => x.Identifier.ValueText))
            sourceBuilder.AppendLine($@"                {symbolName}.{member} => nameof({symbolName}.{member}),");
        sourceBuilder.Append(
            @"                _ => throw new ArgumentOutOfRangeException(nameof(states), states, null)
            };
        }");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBuilder"></param>
    /// <param name="symbolName"></param>
    /// <param name="e"></param>
    private static void GetValuesFast(StringBuilder sourceBuilder, string symbolName, EnumDeclarationSyntax e)
    {
        sourceBuilder.Append($@"

        /// <summary>
        /// 
        /// </summary>
        public static {symbolName}[] GetValuesFast()
        {{
            return new[]
            {{
");
        foreach (var member in e.Members.Select(x => x.Identifier.ValueText))
            sourceBuilder.AppendLine($@"                {symbolName}.{member},");

        sourceBuilder.Append(@"            };
        }");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBuilder"></param>
    /// <param name="symbolName"></param>
    /// <param name="e"></param>
    private static void GetNamesFast(StringBuilder sourceBuilder, string symbolName, EnumDeclarationSyntax e)
    {
        sourceBuilder.Append($@"

        /// <summary>
        /// 
        /// </summary>
        public static string[] GetNamesFast()
        {{
            return new[]
            {{
");
        foreach (var member in e.Members.Select(x => x.Identifier.ValueText))
            sourceBuilder.AppendLine($@"                nameof({symbolName}.{member}),");

        sourceBuilder.Append(@"            };
        }");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBuilder"></param>
    /// <param name="symbolName"></param>
    /// <param name="e"></param>
    private static void GetLengthFast(StringBuilder sourceBuilder, EnumDeclarationSyntax e)
    {
        sourceBuilder.Append($@"

        /// <summary>
        /// 
        /// </summary>
        public static int GetLengthFast()
        {{
            return {e.Members.Count};
");

        sourceBuilder.Append(@"
        }");
    }
}
