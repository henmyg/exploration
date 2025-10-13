using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Maui.Analyzers;
using Xunit;

namespace Maui.IntegrationTests.Analyzers;

/// <summary>
/// Integration tests for the PureFunctionAnalyzer.
/// These tests verify that the analyzer correctly identifies pure functions that should be extracted.
/// </summary>
[Trait("Category", "Integration")]
public class PureFunctionAnalyzerTests
{
    /// <summary>
    /// Test that a simple pure function in a ViewModel is detected.
    /// </summary>
    [Fact]
    public async Task SimplePureFunctionInViewModel_ShouldTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private string FormatPrice(decimal price)
        {
            return $""${price:F2}"";
        }
    }
}";

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(8, 24, 8, 35) // Method name location
            .WithArguments("FormatPrice");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that a pure function with calculations is detected in a Service.
    /// </summary>
    [Fact]
    public async Task CalculationMethodInService_ShouldTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class PricingService
    {
        private decimal CalculateTax(decimal amount, decimal taxRate)
        {
            return amount * taxRate;
        }
    }
}";

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(8, 25, 8, 37) // Method name location
            .WithArguments("CalculateTax");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that a method accessing instance fields does NOT trigger a diagnostic.
    /// </summary>
    [Fact]
    public async Task MethodAccessingInstanceField_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private decimal _taxRate = 0.15m;

        private decimal CalculateTax(decimal amount)
        {
            return amount * _taxRate;
        }
    }
}";

        // No diagnostic expected
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a public method does NOT trigger a diagnostic.
    /// </summary>
    [Fact]
    public async Task PublicMethod_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        public string FormatPrice(decimal price)
        {
            return $""${price:F2}"";
        }
    }
}";

        // No diagnostic expected
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that an async method does NOT trigger a diagnostic.
    /// </summary>
    [Fact]
    public async Task AsyncMethod_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestService
    {
        private async Task<string> FormatPriceAsync(decimal price)
        {
            await Task.Delay(10);
            return $""${price:F2}"";
        }
    }
}";

        // No diagnostic expected
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a static method does NOT trigger a diagnostic.
    /// </summary>
    [Fact]
    public async Task StaticMethod_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private static string FormatPrice(decimal price)
        {
            return $""${price:F2}"";
        }
    }
}";

        // No diagnostic expected
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a method in a class not ending with ViewModel or Service is NOT analyzed.
    /// </summary>
    [Fact]
    public async Task MethodInRegularClass_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class PriceHelper
    {
        private string FormatPrice(decimal price)
        {
            return $""${price:F2}"";
        }
    }
}";

        // No diagnostic expected - class doesn't end with ViewModel or Service
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a pure function with complex logic is detected.
    /// </summary>
    [Fact]
    public async Task ComplexPureFunction_ShouldTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class DataViewModel
    {
        private int CalculateSum(int a, int b, int c)
        {
            int result = a + b;
            result += c;
            return result * 2;
        }
    }
}";

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(8, 21, 8, 33) // Method name location
            .WithArguments("CalculateSum");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that a method accessing properties does NOT trigger a diagnostic.
    /// </summary>
    [Fact]
    public async Task MethodAccessingProperty_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private decimal TaxRate { get; set; } = 0.15m;

        private decimal CalculateTax(decimal amount)
        {
            return amount * TaxRate;
        }
    }
}";

        // No diagnostic expected
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Helper method to verify analyzer diagnostics.
    /// </summary>
    private static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = source,
        };

        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync();
    }

    /// <summary>
    /// Custom test class for the PureFunctionAnalyzer.
    /// </summary>
    private class Test : CSharpAnalyzerTest<PureFunctionAnalyzer, DefaultVerifier>
    {
        public Test()
        {
            // Configure test to use C# 12 and .NET 9
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
        }
    }
}
