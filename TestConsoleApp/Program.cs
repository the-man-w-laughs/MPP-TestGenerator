using Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace tcm
{
    class ClassVisitor : CSharpSyntaxWalker
    {
        public List<SyntaxNode> classes = new List<SyntaxNode>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            base.VisitClassDeclaration(node);

            classes.Add(node); // save your visited classes
        }
    }
    static class TestConsoleMain
    {
        // method with pattern matching
        public static string? GetNamespaceFrom(SyntaxNode s)
        {
            var result = "";
            while (s.Parent.GetType() == typeof(NamespaceDeclarationSyntax))
            {
                result = ((NamespaceDeclarationSyntax)s.Parent).Name.ToString() + '.' + result;
                s = s.Parent;
            }
            if (result != "")
                return result.Remove(result.Length - 1, 1);
            else
                return null;            
        }

        static void Main()
        {
            const string programText =
        @"using System;
using System.Collections;
using System.Linq;
using System.Text;

//namespace HelloWorld
{
    //namespace HelloWorld2{
    class Program1
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}

    class Program2
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            Console.WriteLine($"The tree is a {root.Kind()} node.");
            Console.WriteLine($"The tree has {root.Usings.Count} using statements. They are:");
            foreach (UsingDirectiveSyntax element in root.Usings)
            {
                Console.WriteLine($"\t{element.Name}");
                //element.
            }

            Console.WriteLine($"The tree has {root.Members.Count} members statements. They are:");
            foreach (var element in root.Members)
            {
                Console.WriteLine($"\t{element.Kind()}");
                //element.
            }

            var classVisitor = new ClassVisitor();
            classVisitor.Visit(root);

            var classes = classVisitor.classes; // list of classes in your solution

            Console.WriteLine("Classes:");
            foreach (var classNode in classes){
                // somewhere call it passing the class declaration syntax:
                string ns = GetNamespaceFrom(classNode);
                Console.WriteLine($"{ns}");
            }

            var tests = XUnitTestGenerator.GenerateTests(programText);
            Console.WriteLine(tests[0]);
        }


    }
}