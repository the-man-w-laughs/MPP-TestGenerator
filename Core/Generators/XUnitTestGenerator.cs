using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Core.Interfaces;
using Core.AbstractClasses;

namespace Core.Generators;
public class XUnitTestGenerator : Singleton<XUnitTestGenerator>, ITestGenerator
{
    //main class method
    public List<string> GenerateTests(string code)
    {
        var tests = new List<string>();
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
        var usingsList = root.Usings.Select(x => x.Name.ToString()).ToList();
        var usingsStr = getUsingsStr(usingsList);

        var classVisitor = new ClassVisitor();
        classVisitor.Visit(root);

        foreach (var classNode in classVisitor.classes)
        {
            var classTests = new StringBuilder();
            var methodNames = classNode.ChildNodes().
                Where(x => x.GetType() == typeof(MethodDeclarationSyntax) && ((MethodDeclarationSyntax)x).Modifiers.Where(modifier =>
                    modifier.Kind() == SyntaxKind.PublicKeyword)
                .Any()).
                Select(x => ((MethodDeclarationSyntax)x).Identifier.ToString()).ToList();

            var mathodNamesDic = methodNames
           .GroupBy(p => p)
           .ToDictionary(p => p.Key, q => q.Count());
            var classNamespace = GetNamespaceFrom(classNode);

            classTests.Append(usingsStr);
            if (classNamespace != null)
            {
                classTests.Append($"using {GetNamespaceFrom(classNode)};\n\n");
                classTests.Append($"namespace {classNamespace}.Tests;\n\n");
            }
            else
            {
                classTests.Append($"namespace Tests;\n\n");
            }


            classTests.Append($"public class {classNode.Identifier.ToString()}Tests\n");
            classTests.Append("{\n");
            foreach (var methodName in mathodNamesDic)
            {
                if (methodName.Value != 1)
                    for (int i = 0; i < methodName.Value; i++)
                    {
                        classTests.Append(getMethodTestStr($"{methodName.Key}{i + 1}Test"));
                    }
                else
                    classTests.Append(getMethodTestStr($"{methodName.Key}Test"));
            }
            classTests.Append("}\n");
            tests.Add(classTests.ToString());
        }

        return tests;
    }

    private string getMethodTestStr(string methodName)
    {
        var test = new StringBuilder();

        test.Append("\t[Fact]\n");
        test.Append($"\tpublic void {methodName}()\n");
        test.Append("\t{\n");
        test.Append("\t\tAssert.True(false);\n");
        test.Append("\t}\n\n");
        return test.ToString();
    }
    private string getUsingsStr(IEnumerable<string> usings)
    {
        var usingsStr = new StringBuilder();
        foreach (var us in usings)
        {
            usingsStr.Append($"using {us};\n");
        }
        usingsStr.Append("\n");
        return usingsStr.ToString();
    }

    private string? GetNamespaceFrom(SyntaxNode s)
    {
        var result = "";
        while (s.Parent.GetType() == typeof(NamespaceDeclarationSyntax) ||
            s.Parent.GetType() == typeof(FileScopedNamespaceDeclarationSyntax))
        {
            result = ((NamespaceDeclarationSyntax)s.Parent).Name.ToString() + '.' + result;
            s = s.Parent;
        }
        if (result != "")
            return result.Remove(result.Length - 1, 1);
        else
            return null;
    }
}
