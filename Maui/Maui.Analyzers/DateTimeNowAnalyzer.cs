using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Maui.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DateTimeNowAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MAUI002";
        private const string Category = "Usage";

        private static readonly LocalizableString Title = "Do not use DateTime.Now, DateTime.UtcNow, or DateTime.Today directly";
        private static readonly LocalizableString MessageFormat = "Use injected time provider instead of '{0}'. Inject Func<DateTime> getNow in constructor for testability.";
        private static readonly LocalizableString Description = "Direct use of DateTime.Now, DateTime.UtcNow, or DateTime.Today makes code untestable. Use dependency injection to provide the current time instead.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            // Check if this is DateTime.Now, DateTime.UtcNow, or DateTime.Today
            var memberName = memberAccess.Name.Identifier.Text;
            if (memberName != "Now" && memberName != "UtcNow" && memberName != "Today")
                return;

            // Get the symbol for the expression
            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Expression);
            var typeSymbol = symbolInfo.Symbol as INamedTypeSymbol;

            if (typeSymbol == null)
                return;

            // Check if it's System.DateTime
            if (typeSymbol.ToString() != "System.DateTime")
                return;

            // Allow DateTime.Now/UtcNow in specific acceptable patterns:
            // 1. Inside lambda expressions used as default parameter values in constructors
            //    Pattern: getNow ?? (() => DateTime.Now)
            // 2. Inside null-coalescing expressions for optional parameters
            //    Pattern: now ?? DateTime.UtcNow
            // 3. Inside DI registration lambdas
            //    Pattern: builder.Services.AddSingleton<Func<DateTime>>(sp => () => DateTime.UtcNow)
            if (IsAcceptableDefaultValuePattern(memberAccess) || IsInsideDIRegistration(memberAccess))
                return;

            var diagnostic = Diagnostic.Create(
                Rule,
                memberAccess.GetLocation(),
                $"DateTime.{memberName}");

            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Checks if DateTime.Now/UtcNow/Today is used in an acceptable pattern for default values.
        /// Acceptable patterns:
        /// 1. In constructors with null-coalescing: getNow ?? (() => DateTime.Now)
        /// 2. In any method with null-coalescing: now ?? DateTime.UtcNow
        /// 3. In any method with null-coalescing: today ?? DateTime.Today
        /// </summary>
        private static bool IsAcceptableDefaultValuePattern(MemberAccessExpressionSyntax memberAccess)
        {
            // Check if we're inside a constructor
            var constructor = memberAccess.Ancestors().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (constructor != null)
            {
                // In a constructor - check if there's a null-coalescing operator in the ancestry
                var hasCoalesceAncestor = memberAccess.Ancestors()
                    .OfType<BinaryExpressionSyntax>()
                    .Any(b => b.IsKind(SyntaxKind.CoalesceExpression));

                if (hasCoalesceAncestor)
                {
                    // Pattern: _field = parameter ?? (() => DateTime.Now)
                    return true;
                }
            }

            // Check if there's a null-coalescing expression in the immediate ancestry
            // Pattern: var x = now ?? DateTime.UtcNow
            var hasNearbyCoalesce = memberAccess.Ancestors()
                .Take(5) // Only check nearby ancestors
                .OfType<BinaryExpressionSyntax>()
                .Any(b => b.IsKind(SyntaxKind.CoalesceExpression));

            if (hasNearbyCoalesce)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if DateTime.Now/UtcNow/Today is used inside a DI registration lambda.
        /// Acceptable patterns:
        /// builder.Services.AddSingleton<Func<DateTime>>(sp => () => DateTime.UtcNow)
        /// builder.Services.AddTransient<ITimeProvider>(_ => new TimeProvider(() => DateTime.Now))
        /// </summary>
        private static bool IsInsideDIRegistration(MemberAccessExpressionSyntax memberAccess)
        {
            // Check if we're inside a lambda expression
            var lambdaExpression = memberAccess.Ancestors()
                .OfType<LambdaExpressionSyntax>()
                .FirstOrDefault();

            if (lambdaExpression == null)
                return false;

            // Check if this lambda is an argument to a method call
            var invocation = lambdaExpression.Ancestors()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (invocation == null)
                return false;

            // Check if the method is a DI registration method (AddSingleton, AddTransient, AddScoped)
            var memberAccessExpression = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpression != null)
            {
                var methodName = memberAccessExpression.Name.Identifier.Text;
                if (methodName == "AddSingleton" || methodName == "AddTransient" || methodName == "AddScoped")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
