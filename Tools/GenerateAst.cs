using System.Text;
using Tools.Extensions;

namespace Tools;

public class GenerateAst
{
    public static void Init(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        var outputDir = args[0];
        DefineAst(outputDir, "Expr", [
            "Binary: Expr left, Token @operator, Expr right",
            "Grouping: Expr expression",
            "Literal: object value",
            "Unary: Token @operator, Expr right"
        ]);
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        var path = $"{outputDir}\\{baseName}.cs";
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        
        writer.WriteLine("namespace DotLox;");
        writer.WriteLine("");
        writer.WriteLine($"public abstract class {baseName}");
        writer.WriteLine("{");

        DefineVisitor(writer, baseName, types);

        foreach (var type in types)
        {
            var typeValues = type.Split(":");
            var className = typeValues[0].Trim();
            var fields = typeValues[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
        
        writer.WriteLine("\tpublic abstract T Accept<T>(Visitor<T> visitor);");
        
        writer.WriteLine("}");
    }

    private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("\tpublic interface Visitor<T>");
        writer.WriteLine("\t{");

        foreach (var type in types)
        {
            var typeName = type.Split(":")[0].Trim();
            writer.WriteLine($"\t\tpublic T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
        }
        
        writer.WriteLine("\t}");
        writer.WriteLine("");
    }

    private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
    {
        writer.WriteLine($"\tpublic class {className} : {baseName}");
        writer.WriteLine("\t{");
        
        var fields = fieldList.Split(", ");
        foreach (var field in fields)
        {
            var fieldVals = field.Split(" ");
            writer.WriteLine($"\t\tpublic {fieldVals[0]} {fieldVals[1].RemoveAtSymbol().Capitalize()} {{ get; }}");
        }

        writer.WriteLine("");
        
        writer.WriteLine($"\t\tpublic {className}({fieldList})");
        writer.WriteLine("\t\t{");
        
        foreach (var field in fields)
        {
            var name = field.Split(" ")[1];
            writer.WriteLine($"\t\t\t{name.RemoveAtSymbol().Capitalize()} = {name};");
        }
        
        writer.WriteLine("\t\t}");
        
        writer.WriteLine("");
        writer.WriteLine("\t\tpublic override T Accept<T>(Visitor<T> visitor)");
        writer.WriteLine("\t\t{");
        writer.WriteLine($"\t\t\treturn visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("\t\t}");
        
        writer.WriteLine("\t}");
        
        writer.WriteLine("");
    }
}