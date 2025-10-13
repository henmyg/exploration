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
        private static readonly LocalizableString MessageFormat = "Method '{0}' appears to be a pure function and should be extracted to a static operations class";
        private static readonly LocalizableString Description = "Pure functions (methods without dependencies or side effects) should be extracted into companion static operations classes using extension method syntax.";

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

            // Check if method accesses any instance members
            var methodBody = methodDeclaration.Body;
            if (methodBody == null)
                return;

            // Look for 'this.' references or direct field/property access
            var hasInstanceMemberAccess = methodBody.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Any(ma => ma.Expression is ThisExpressionSyntax || IsInstanceMemberAccess(ma, context.SemanticModel));

            // Look for direct identifiers that might be instance fields
            if (!hasInstanceMemberAccess)
            {
                hasInstanceMemberAccess = methodBody.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Any(id => IsInstanceField(id, context.SemanticModel));
            }

            // If method doesn't access instance members, it might be extractable
            if (!hasInstanceMemberAccess)
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsInstanceMemberAccess(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
                return false;

            // Check if it's an instance field or property
            return (symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property) && !symbol.IsStatic;
        }

        private static bool IsInstanceField(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
                return false;

            // Check if it's an instance field or property belonging to the containing type
            if ((symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property) && !symbol.IsStatic)
            {
                var containingType = symbol.ContainingType;
                var currentType = semanticModel.GetDeclaredSymbol(identifier.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault());

                return SymbolEqualityComparer.Default.Equals(containingType, currentType);
            }

            return false;
        }
    }
}
