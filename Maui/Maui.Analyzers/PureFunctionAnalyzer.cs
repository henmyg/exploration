using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Maui.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PureFunctionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MAUI001";
        private const string Category = "Design";

        private static readonly LocalizableString Title = "Pure function should be extracted to static operations class";
        private static readonly LocalizableString MessageFormat = "Method '{0}' appears to be a pure function and should be extracted to a static operations class. Instance members can be passed as parameters.";
        private static readonly LocalizableString Description = "Pure functions (methods without side effects) should be extracted into companion static operations classes using extension method syntax. Methods that only read from instance fields/properties can be extracted by passing them as parameters.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            // Skip if method is static (already extracted)
            if (methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return;

            // Skip if method is async (likely has side effects)
            if (methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
                return;

            // Skip if method is public/protected (likely interface implementation or API)
            var hasPublicModifier = methodDeclaration.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.PublicKeyword) ||
                m.IsKind(SyntaxKind.ProtectedKeyword));

            if (hasPublicModifier)
                return;

            // Only check methods in classes that end with ViewModel or Service
            var classDeclaration = methodDeclaration.Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (classDeclaration == null)
                return;

            var className = classDeclaration.Identifier.Text;
            if (!className.EndsWith("ViewModel") && !className.EndsWith("Service"))
                return;

            // Check if method has side effects or dependencies
            var methodBody = methodDeclaration.Body;
            if (methodBody == null)
                return;

            // Check if method writes to any instance members (has side effects)
            var writesToInstanceMembers = HasInstanceMemberWrites(methodBody, context.SemanticModel);
            if (writesToInstanceMembers)
                return;

            // Check if method calls other instance methods (potential side effects or dependencies)
            var callsInstanceMethods = CallsInstanceMethods(methodBody, context.SemanticModel);
            if (callsInstanceMethods)
                return;

            // Method is extractable - it has no side effects and only uses:
            // - Parameters
            // - Local variables
            // - Static methods
            // - Instance fields/properties (which can be passed as parameters)
            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Checks if the method body writes to any instance members (fields or properties).
        /// Methods that write to instance state have side effects and cannot be extracted as pure functions.
        /// </summary>
        private static bool HasInstanceMemberWrites(BlockSyntax methodBody, SemanticModel semanticModel)
        {
            // Check for assignments to instance members
            var assignments = methodBody.DescendantNodes().OfType<AssignmentExpressionSyntax>();

            foreach (var assignment in assignments)
            {
                var leftSide = assignment.Left;

                // Check if assignment is to an instance member
                if (IsInstanceMemberReference(leftSide, semanticModel))
                    return true;
            }

            // Check for increment/decrement operations on instance members (++, --)
            var unaryExpressions = methodBody.DescendantNodes()
                .OfType<PrefixUnaryExpressionSyntax>()
                .Concat<ExpressionSyntax>(methodBody.DescendantNodes().OfType<PostfixUnaryExpressionSyntax>());

            foreach (var unary in unaryExpressions)
            {
                SyntaxNode operand;
                if (unary is PrefixUnaryExpressionSyntax prefix)
                {
                    if (prefix.IsKind(SyntaxKind.PreIncrementExpression) ||
                        prefix.IsKind(SyntaxKind.PreDecrementExpression))
                    {
                        operand = prefix.Operand;
                        if (IsInstanceMemberReference(operand, semanticModel))
                            return true;
                    }
                }
                else if (unary is PostfixUnaryExpressionSyntax postfix)
                {
                    if (postfix.IsKind(SyntaxKind.PostIncrementExpression) ||
                        postfix.IsKind(SyntaxKind.PostDecrementExpression))
                    {
                        operand = postfix.Operand;
                        if (IsInstanceMemberReference(operand, semanticModel))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the method calls any instance methods (non-static methods on the same class).
        /// Instance method calls could have side effects or introduce dependencies.
        /// </summary>
        private static bool CallsInstanceMethods(BlockSyntax methodBody, SemanticModel semanticModel)
        {
            // Find all invocation expressions (method calls)
            var invocations = methodBody.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                var method = symbolInfo.Symbol as IMethodSymbol;

                if (method == null)
                    continue;

                // Skip static methods - they could be pure functions
                if (method.IsStatic)
                    continue;

                // Skip extension methods - they're static under the hood
                if (method.IsExtensionMethod)
                    continue;

                // Check if it's an instance method on the containing type or its base types
                var currentType = semanticModel.GetDeclaredSymbol(
                    invocation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault()) as INamedTypeSymbol;

                if (currentType != null && IsMethodOnTypeOrBaseType(method, currentType))
                {
                    // This is a call to an instance method on the same class or its base classes
                    return true;
                }

                // Check if it's calling an instance method on an instance field/property
                // (e.g., _repository.GetData() where _repository is an instance field)
                var expression = invocation.Expression;
                if (expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (IsInstanceMemberReference(memberAccess.Expression, semanticModel))
                    {
                        // Calling a method on an instance field/property
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a method belongs to the given type or any of its base types.
        /// </summary>
        private static bool IsMethodOnTypeOrBaseType(IMethodSymbol method, INamedTypeSymbol currentType)
        {
            var type = currentType;
            while (type != null)
            {
                if (SymbolEqualityComparer.Default.Equals(method.ContainingType, type))
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Checks if a syntax node references an instance member (field or property).
        /// </summary>
        private static bool IsInstanceMemberReference(SyntaxNode node, SemanticModel semanticModel)
        {
            ISymbol symbol = null;

            // Handle member access (e.g., this.field or field)
            if (node is MemberAccessExpressionSyntax memberAccess)
            {
                symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
            }
            // Handle direct identifier (e.g., field without 'this.')
            else if (node is IdentifierNameSyntax identifier)
            {
                symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
            }

            if (symbol == null)
                return false;

            // Check if it's an instance field or property belonging to the containing type
            if ((symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property) && !symbol.IsStatic)
            {
                var containingType = symbol.ContainingType;
                var currentType = semanticModel.GetDeclaredSymbol(node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault());

                return SymbolEqualityComparer.Default.Equals(containingType, currentType);
            }

            return false;
        }
    }
}
