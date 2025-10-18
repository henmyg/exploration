using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Maui.Analyzers;
using Xunit;

namespace Maui.IntegrationTests.Analyzers;

/// <summary>
/// Integration tests for the DateTimeNowAnalyzer.
/// These tests verify that the analyzer correctly prevents direct usage of DateTime.Now and DateTime.UtcNow.
/// </summary>
[Trait("Category", "Integration")]
public class DateTimeNowAnalyzerTests
{
    /// <summary>
    /// Test that direct usage of DateTime.Now triggers an error.
    /// </summary>
    [Fact]
    public async Task DirectUsageOfDateTimeNow_ShouldTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        public void DoWork()
        {
            var now = DateTime.Now;
        }
    }
}";

        var expected = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(10, 23, 10, 35) // DateTime.Now location
            .WithArguments("DateTime.Now");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that direct usage of DateTime.UtcNow triggers an error.
    /// </summary>
    [Fact]
    public async Task DirectUsageOfDateTimeUtcNow_ShouldTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        public void ProcessData()
        {
            var utcNow = DateTime.UtcNow;
        }
    }
}";

        var expected = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(10, 26, 10, 41) // DateTime.UtcNow location
            .WithArguments("DateTime.UtcNow");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that DateTime.Now in constructor with null-coalescing is allowed.
    /// Pattern: _getNow = getNow ?? (() => DateTime.Now)
    /// </summary>
    [Fact]
    public async Task DateTimeNowInConstructorWithNullCoalescing_ShouldNotTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private readonly Func<DateTime> _getNow;

        public TestViewModel(Func<DateTime>? getNow = null)
        {
            _getNow = getNow ?? (() => DateTime.Now);
        }
    }
}";

        // No diagnostic expected - this is an acceptable pattern
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that DateTime.UtcNow in method with null-coalescing is allowed.
    /// Pattern: now ?? DateTime.UtcNow
    /// </summary>
    [Fact]
    public async Task DateTimeUtcNowWithNullCoalescing_ShouldNotTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        public void ProcessData(DateTime? timestamp = null)
        {
            var effectiveTime = timestamp ?? DateTime.UtcNow;
        }
    }
}";

        // No diagnostic expected - this is an acceptable pattern
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that DateTime.Now used outside of null-coalescing in constructor triggers an error.
    /// </summary>
    [Fact]
    public async Task DateTimeNowInConstructorWithoutNullCoalescing_ShouldTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestViewModel
    {
        private readonly DateTime _createdAt;

        public TestViewModel()
        {
            _createdAt = DateTime.Now;
        }
    }
}";

        var expected = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(12, 26, 12, 38) // DateTime.Now location
            .WithArguments("DateTime.Now");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that DateTime.Now in regular method (not constructor) triggers an error.
    /// </summary>
    [Fact]
    public async Task DateTimeNowInRegularMethod_ShouldTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        private readonly Func<DateTime> _getNow;

        public TestService(Func<DateTime>? getNow = null)
        {
            _getNow = getNow ?? (() => DateTime.Now);
        }

        public void DoWork()
        {
            var now = DateTime.Now; // This should trigger error
        }
    }
}";

        var expected = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(17, 23, 17, 35) // DateTime.Now in DoWork method
            .WithArguments("DateTime.Now");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that multiple violations are all detected.
    /// </summary>
    [Fact]
    public async Task MultipleViolations_ShouldTriggerMultipleErrors()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        public void Method1()
        {
            var now1 = DateTime.Now;
        }

        public void Method2()
        {
            var now2 = DateTime.UtcNow;
        }

        public void Method3()
        {
            var now3 = DateTime.Now;
        }
    }
}";

        var expected1 = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(10, 24, 10, 36)
            .WithArguments("DateTime.Now");

        var expected2 = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(15, 24, 15, 39)
            .WithArguments("DateTime.UtcNow");

        var expected3 = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(20, 24, 20, 36)
            .WithArguments("DateTime.Now");

        await VerifyAnalyzerAsync(testCode, expected1, expected2, expected3);
    }

    /// <summary>
    /// Test that DateTime.Today triggers an error (for consistency with Now/UtcNow).
    /// </summary>
    [Fact]
    public async Task DateTimeToday_ShouldTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        public void DoWork()
        {
            var today = DateTime.Today;
        }
    }
}";

        var expected = new DiagnosticResult(DateTimeNowAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(10, 25, 10, 39) // DateTime.Today location
            .WithArguments("DateTime.Today");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that using a different type's Now property doesn't trigger (e.g., DateTimeOffset.Now).
    /// </summary>
    [Fact]
    public async Task DateTimeOffsetNow_ShouldNotTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        public void DoWork()
        {
            var now = DateTimeOffset.Now;
        }
    }
}";

        // No diagnostic expected - this analyzer only targets System.DateTime
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test the pattern with SimpleLambdaExpression (single parameter).
    /// </summary>
    [Fact]
    public async Task SimpleLambdaWithDateTimeNow_InConstructor_ShouldNotTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        private readonly Func<DateTime> _getTime;

        public TestService(Func<DateTime>? getTime = null)
        {
            _getTime = getTime ?? (() => DateTime.Now);
        }
    }
}";

        // No diagnostic expected - acceptable constructor pattern
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test nested null-coalescing expressions.
    /// </summary>
    [Fact]
    public async Task NestedNullCoalescing_ShouldNotTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        public void ProcessData(DateTime? timestamp1, DateTime? timestamp2)
        {
            var effectiveTime = timestamp1 ?? timestamp2 ?? DateTime.UtcNow;
        }
    }
}";

        // No diagnostic expected - null-coalescing pattern
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that DateTime.Today with null-coalescing is allowed.
    /// </summary>
    [Fact]
    public async Task DateTimeTodayWithNullCoalescing_ShouldNotTriggerError()
    {
        var testCode = @"
using System;

namespace TestNamespace
{
    public class TestService
    {
        public void ProcessData(DateTime? date = null)
        {
            var effectiveDate = date ?? DateTime.Today;
        }
    }
}";

        // No diagnostic expected - null-coalescing pattern
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
    /// Custom test class for the DateTimeNowAnalyzer.
    /// </summary>
    private class Test : CSharpAnalyzerTest<DateTimeNowAnalyzer, DefaultVerifier>
    {
        public Test()
        {
            // Configure test to use C# 12 and .NET 9
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
        }
    }
}
