using Core.AbstractClasses;
using Core.Interfaces;
using Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TestGenerator.Core;

namespace Core.Generators;
public class NewXUnitTestGenerator : Singleton<XUnitTestGenerator>, ITestGenerator
{
    //main class method
    public List<string> GenerateTests(string code)
    {
        var tests = new List<string>();
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        var usingsListStr = root.Usings.Select(x => x.Name.ToString()).ToList();
        
        var classVisitor = new ClassVisitor();
        classVisitor.Visit(root);

        var classRewriter = new NewClassRewriter();
        foreach (var classNode in classVisitor.classes)
        {            
            var compUnit = CompilationUnit().WithUsings(root.Usings);
            
            var classNamespace = GetNamespaceFrom(classNode);
            
            if (classNamespace != null)
                // class Declared in namespace
            {
                var customUsing = UsingDirective(ParseName(classNamespace));
                compUnit = compUnit.AddUsings(customUsing);

                var customNamespace = FileScopedNamespaceDeclaration(ParseName(classNamespace + ".Tests"));
                compUnit = compUnit.AddMembers(customNamespace);
            }
            else
            {
                // class Declared without namespace
                var customNamespace = FileScopedNamespaceDeclaration(ParseName("Tests"));
                compUnit = compUnit.AddMembers(customNamespace);
            }

            compUnit = compUnit.AddMembers((MemberDeclarationSyntax)classRewriter.Visit(classNode));
            tests.Add(compUnit.NormalizeWhitespace().ToFullString());
        }

        return tests;
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
