using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Core.Generators;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        const string programText =
        @"
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace NS1
{
    namespace NS2
    {
        class Class1
        {
            private void FirstMethod()
            {                
            }
            public void FirstMethod(int a)
            {                
            }            
            public void FirstMethod(int a, int b)
            {
            }
            
        }
    }

    class Class2
    {
        public void FirstMethod()
        {                
        }
        public void SecondMethod(int a)
        {                
        }            
        public void ThirdMethod(int a, int b)
        {
        }
    }
}";
        var tests = NewXUnitTestGenerator.GetInstance().GenerateTests(programText);

        //verify files count
        Assert.Equal(2, tests.Count);

        //
        //verify first class 
        //
        var class1Tests = tests[0];
        var tree = CSharpSyntaxTree.ParseText(class1Tests);
        var root = tree.GetCompilationUnitRoot();

        //verify usings generation
        var usings = root.Usings.Select(x => x.Name.ToString()).ToList();
        Assert.Equal(5, usings.Count);
        Assert.Contains("System", usings);
        Assert.Contains("System.Collections", usings);
        Assert.Contains("System.Linq", usings);
        Assert.Contains("System.Text", usings);
        Assert.Contains("NS1.NS2", usings);

        //verify namespace declaration generation
        var namespaseDeclarations = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Select(x => x.Name.ToString()).ToList();
        Assert.Single(namespaseDeclarations);
        Assert.Contains("NS1.NS2.Tests", namespaseDeclarations);

        // virify class declaration generation
        var classDeclarationsNodes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        Assert.Single(classDeclarationsNodes);
        Assert.Equal("Class1Tests", ((ClassDeclarationSyntax)(classDeclarationsNodes.First())).Identifier.Text.ToString());

        // verify test methods generation
        var methodNames = classDeclarationsNodes.First().ChildNodes().
                Where(x => x.GetType() == typeof(MethodDeclarationSyntax)).
                Select(x => ((MethodDeclarationSyntax)x).Identifier.ToString()).ToList();
        Assert.Equal(2, methodNames.Count);
        Assert.Contains("FirstMethod1Test", methodNames);
        Assert.Contains("FirstMethod2Test", methodNames);

        //
        //verify second class 
        //

        var class2Tests = tests[1];
        tree = CSharpSyntaxTree.ParseText(class2Tests);
        root = tree.GetCompilationUnitRoot();

        //verify usings generation
        usings = root.Usings.Select(x => x.Name.ToString()).ToList();
        Assert.Equal(5, usings.Count);
        Assert.Contains("System", usings);
        Assert.Contains("System.Collections", usings);
        Assert.Contains("System.Linq", usings);
        Assert.Contains("System.Text", usings);
        Assert.Contains("NS1", usings);

        //verify namespace declaration generation
        namespaseDeclarations = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Select(x => x.Name.ToString()).ToList();
        Assert.Single(namespaseDeclarations);
        Assert.Contains("NS1.Tests", namespaseDeclarations);

        // virify class declaration generation
        classDeclarationsNodes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        Assert.Single(classDeclarationsNodes);
        Assert.Equal("Class2Tests", classDeclarationsNodes.First().Identifier.Text);

        // verify test methods generation
        methodNames = classDeclarationsNodes.First().ChildNodes().
                Where(x => x.GetType() == typeof(MethodDeclarationSyntax)).
                Select(x => ((MethodDeclarationSyntax)x).Identifier.ToString()).ToList();
        Assert.Equal(3, methodNames.Count);
        Assert.Contains("FirstMethodTest", methodNames);
        Assert.Contains("SecondMethodTest", methodNames);
        Assert.Contains("ThirdMethodTest", methodNames);



        var comp = SyntaxFactory.CompilationUnit()
        .AddMembers(
            SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("ACO"))
                    .AddMembers(
                    SyntaxFactory.ClassDeclaration("MainForm")
                        .AddMembers(
                            SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("System.Windows.Forms.Timer"), "Ticker")
                                    .AddAccessorListAccessors(
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))),
                            SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Main")
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                    .WithBody(SyntaxFactory.Block())
                            )
                    )
            );

        var console = SyntaxFactory.IdentifierName("Console");
        var writeline = SyntaxFactory.IdentifierName("WriteLine");
        var memberaccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, console, writeline);

        var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("A")));
        var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

        var writeLineCall =
            SyntaxFactory.ExpressionStatement(
            SyntaxFactory.InvocationExpression(memberaccess,
            SyntaxFactory.ArgumentList(argumentList)));

        var text = writeLineCall.ToFullString();

    }
}