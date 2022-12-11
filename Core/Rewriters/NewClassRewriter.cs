using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestGenerator.Core;

public class NewClassRewriter : CSharpSyntaxRewriter
{
    private ConcurrentBag<string> _uniqueMethods = new();    
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Modifiers.First().IsKind(SyntaxKind.PublicKeyword))
        {
            var identifierText = node.Identifier.Text;

            int index = 0;

            while (_uniqueMethods.Contains(identifierText + (index == 0 ? "" : index)))
            {
                index++;
            }

            _uniqueMethods.Add(identifierText + (index == 0 ? "" : index));

            var newIdentifier = Identifier(identifierText + (index == 0 ? "" : index) + "Test");

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
        var newClassDeclaration = ClassDeclaration(identifierText + "Tests")
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)));
        return base.VisitClassDeclaration(newClassDeclaration);
    }
}