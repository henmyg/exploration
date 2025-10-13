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
    /// Test that a method only READING instance fields SHOULD trigger a diagnostic.
    /// These fields can be passed as parameters.
    /// </summary>
    [Fact]
    public async Task MethodReadingInstanceField_ShouldTriggerDiagnostic()
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

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(10, 25, 10, 37) // Method name location
            .WithArguments("CalculateTax");

        await VerifyAnalyzerAsync(testCode, expected);
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
    /// Test that a method only READING properties SHOULD trigger a diagnostic.
    /// These properties can be passed as parameters.
    /// </summary>
    [Fact]
    public async Task MethodReadingProperty_ShouldTriggerDiagnostic()
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

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(10, 25, 10, 37) // Method name location
            .WithArguments("CalculateTax");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that a method WRITING to instance fields does NOT trigger a diagnostic.
    /// Methods with side effects cannot be extracted as pure functions.
    /// </summary>
    [Fact]
    public async Task MethodWritingToInstanceField_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private int _counter = 0;

        private void IncrementCounter()
        {
            _counter++;
        }
    }
}";

        // No diagnostic expected - method has side effects
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a method WRITING to properties does NOT trigger a diagnostic.
    /// Methods with side effects cannot be extracted as pure functions.
    /// </summary>
    [Fact]
    public async Task MethodWritingToProperty_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private string Status { get; set; } = string.Empty;

        private void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }
    }
}";

        // No diagnostic expected - method has side effects
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a method reading multiple fields SHOULD trigger a diagnostic.
    /// </summary>
    [Fact]
    public async Task MethodReadingMultipleFields_ShouldTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class PricingService
    {
        private decimal _taxRate = 0.15m;
        private decimal _discountRate = 0.10m;

        private decimal CalculateFinalPrice(decimal basePrice)
        {
            var discounted = basePrice * (1 - _discountRate);
            return discounted * (1 + _taxRate);
        }
    }
}";

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(11, 25, 11, 44) // Method name location
            .WithArguments("CalculateFinalPrice");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that a method calling other instance methods does NOT trigger a diagnostic.
    /// Instance method calls could have side effects.
    /// Note: The called method (FormatCurrency) will trigger its own diagnostic since it's pure.
    /// </summary>
    [Fact]
    public async Task MethodCallingInstanceMethod_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private string FormatPrice(decimal price)
        {
            return FormatCurrency(price);
        }

        private string FormatCurrency(decimal amount)
        {
            return $""${amount:F2}"";
        }
    }
}";

        // FormatPrice should NOT trigger (calls instance method)
        // FormatCurrency WILL trigger (is pure)
        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(13, 24, 13, 38) // FormatCurrency method name location
            .WithArguments("FormatCurrency");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that a method calling base class instance methods does NOT trigger a diagnostic.
    /// Base class instance method calls could have side effects.
    /// </summary>
    [Fact]
    public async Task MethodCallingBaseClassMethod_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class BaseViewModel
    {
        protected void NotifyChanged(string propertyName)
        {
            // Simulate notification logic
        }
    }

    public class TestViewModel : BaseViewModel
    {
        private void UpdateData(string data)
        {
            NotifyChanged(""Data"");
        }
    }
}";

        // No diagnostic expected - calls base class instance method
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a method calling methods on instance fields does NOT trigger a diagnostic.
    /// These calls could have side effects or access external state.
    /// </summary>
    [Fact]
    public async Task MethodCallingMethodOnInstanceField_ShouldNotTriggerDiagnostic()
    {
        var testCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public interface IRepository
    {
        List<string> GetData();
    }

    public class DataService
    {
        private readonly IRepository _repository;

        public DataService(IRepository repository)
        {
            _repository = repository;
        }

        private List<string> FetchData()
        {
            return _repository.GetData();
        }
    }
}";

        // No diagnostic expected - calls method on instance field
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that a method calling static methods SHOULD trigger a diagnostic.
    /// Static methods can be called from extracted static operations.
    /// </summary>
    [Fact]
    public async Task MethodCallingStaticMethod_ShouldTriggerDiagnostic()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private string FormatPrice(decimal price)
        {
            return string.Format(""${0:F2}"", price);
        }
    }
}";

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(8, 24, 8, 35) // Method name location
            .WithArguments("FormatPrice");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that a method calling extension methods SHOULD trigger a diagnostic.
    /// Extension methods are static methods and don't have side effects.
    /// </summary>
    [Fact]
    public async Task MethodCallingExtensionMethod_ShouldTriggerDiagnostic()
    {
        var testCode = @"
using System;
using System.Linq;

namespace TestNamespace
{
    public class DataViewModel
    {
        private int CountItems(int[] items)
        {
            return items.Count();
        }
    }
}";

        var expected = new DiagnosticResult(PureFunctionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(9, 21, 9, 31) // Method name location
            .WithArguments("CountItems");

        await VerifyAnalyzerAsync(testCode, expected);
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
