using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Maui.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TaskDelayAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MAUI003";
        private const string Category = "Usage";

        private static readonly LocalizableString Title = "Do not use Task.Delay directly";
        private static readonly LocalizableString MessageFormat = "Use injected ITaskDelayer instead of '{0}'. Inject ITaskDelayer in constructor for testability.";
        private static readonly LocalizableString Description = "Direct use of Task.Delay makes tests slow and flaky. Use ITaskDelayer abstraction to enable fast, deterministic tests.";

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
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Check if this is a member access expression (e.g., Task.Delay)
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
                return;

            // Check if the method name is "Delay"
            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "Delay")
                return;

            // Get the symbol information
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;

            // Check if it's Task.Delay from System.Threading.Tasks
            if (methodSymbol.ContainingType?.ToString() != "System.Threading.Tasks.Task")
                return;

            // Allow Task.Delay in SystemTaskDelayer implementation
            if (IsInSystemTaskDelayer(invocation))
                return;

            var diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                "Task.Delay");

            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Checks if the Task.Delay call is inside the SystemTaskDelayer class.
        /// This is the only acceptable location for direct Task.Delay usage.
        /// </summary>
        private static bool IsInSystemTaskDelayer(InvocationExpressionSyntax invocation)
        {
            var classDeclaration = invocation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
                return false;

            return classDeclaration.Identifier.Text == "SystemTaskDelayer";
        }
    }
}
