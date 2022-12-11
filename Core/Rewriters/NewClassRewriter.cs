using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestGenerator.Core;

public class NewClassRewriter : CSharpSyntaxRewriter
{
    private ConcurrentDictionary<string, bool>? _methodNamesDic;
    private ConcurrentBag<string> _methodNames = new();
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Modifiers.Where(modifier => modifier.Kind() == SyntaxKind.PublicKeyword).Any())
        {
            var identifier = node.Identifier.Text;

            var newIdentifierStr = "";
            if (_methodNamesDic[identifier])
            {
                int index = 1;
                while (_methodNames.Contains(identifier + index))
                {
                    index++;
                }
                _methodNames.Add(identifier + index);
                newIdentifierStr = identifier + index + "Test";
            }
            else
            {
                newIdentifierStr = identifier + "Test";
            }

            var newIdentifier = SyntaxFactory.Identifier(newIdentifierStr);

            var newMethod = MethodDeclaration(
                                PredefinedType(
                                    Token(SyntaxKind.VoidKeyword)),
                                newIdentifier)
                            .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SingletonSeparatedList<AttributeSyntax>(
                                            Attribute(
                                                IdentifierName("Fact"))))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithBody(
                                Block(
                                    SingletonList<StatementSyntax>(
                                        ExpressionStatement(
                                            InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("Assert"),
                                                    IdentifierName("True")))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SingletonSeparatedList<ArgumentSyntax>(
                                                        Argument(
                                                            LiteralExpression(
                                                                SyntaxKind.FalseLiteralExpression)))))))));

            return base.VisitMethodDeclaration(newMethod);
        }
        else
        {
            return null;
        }

    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var identifierText = node.Identifier.Text;

        var newNameClass = node.WithIdentifier(Identifier(identifierText + "Tests"));

        var methodNames = node.ChildNodes().
                Where(x => x.GetType() == typeof(MethodDeclarationSyntax) && ((MethodDeclarationSyntax)x).Modifiers.Where(modifier =>
                    modifier.Kind() == SyntaxKind.PublicKeyword)
                .Any()).
                Select(x => ((MethodDeclarationSyntax)x).Identifier.ToString()).ToList();

        var methodNamesDic = methodNames
       .GroupBy(p => p)
       .ToDictionary(p => p.Key, q => q.Count() > 1);
        _methodNamesDic = new ConcurrentDictionary<string, bool>(methodNamesDic);
        return base.VisitClassDeclaration(newNameClass);
    }
}