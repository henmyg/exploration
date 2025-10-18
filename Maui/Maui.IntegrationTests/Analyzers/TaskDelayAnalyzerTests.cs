using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Maui.Analyzers;
using Xunit;

namespace Maui.IntegrationTests.Analyzers;

/// <summary>
/// Integration tests for the TaskDelayAnalyzer.
/// These tests verify that the analyzer correctly prevents direct usage of Task.Delay.
/// </summary>
[Trait("Category", "Integration")]
public class TaskDelayAnalyzerTests
{
    /// <summary>
    /// Test that direct usage of Task.Delay triggers an error.
    /// </summary>
    [Fact]
    public async Task DirectUsageOfTaskDelay_ShouldTriggerError()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestService
    {
        public async Task DoWork()
        {
            await Task.Delay(1000);
        }
    }
}";

        var expected = new DiagnosticResult(TaskDelayAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(11, 19, 11, 35) // Task.Delay(1000) location
            .WithArguments("Task.Delay");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that Task.Delay with TimeSpan triggers an error.
    /// </summary>
    [Fact]
    public async Task TaskDelayWithTimeSpan_ShouldTriggerError()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestService
    {
        public async Task DoWork()
        {
            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }
}";

        var expected = new DiagnosticResult(TaskDelayAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(11, 19, 11, 54) // Task.Delay(TimeSpan.FromMinutes(5)) location
            .WithArguments("Task.Delay");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that Task.Delay with CancellationToken triggers an error.
    /// </summary>
    [Fact]
    public async Task TaskDelayWithCancellationToken_ShouldTriggerError()
    {
        var testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestService
    {
        public async Task DoWork(CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
        }
    }
}";

        var expected = new DiagnosticResult(TaskDelayAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(12, 19, 12, 54) // Task.Delay(1000, cancellationToken) location
            .WithArguments("Task.Delay");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that Task.Delay inside SystemTaskDelayer class is allowed.
    /// </summary>
    [Fact]
    public async Task TaskDelayInSystemTaskDelayer_ShouldNotTriggerError()
    {
        var testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class SystemTaskDelayer
    {
        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            return Task.Delay(delay, cancellationToken);
        }
    }
}";

        // No diagnostic expected - Task.Delay is allowed in SystemTaskDelayer
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that using ITaskDelayer does not trigger an error.
    /// </summary>
    [Fact]
    public async Task UsingITaskDelayer_ShouldNotTriggerError()
    {
        var testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public interface ITaskDelayer
    {
        Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
    }

    public class TestService
    {
        private readonly ITaskDelayer _taskDelayer;

        public TestService(ITaskDelayer taskDelayer)
        {
            _taskDelayer = taskDelayer;
        }

        public async Task DoWork()
        {
            await _taskDelayer.DelayAsync(TimeSpan.FromMinutes(5));
        }
    }
}";

        // No diagnostic expected - using ITaskDelayer interface
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that multiple Task.Delay violations are all detected.
    /// </summary>
    [Fact]
    public async Task MultipleTaskDelayViolations_ShouldTriggerMultipleErrors()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestService
    {
        public async Task Method1()
        {
            await Task.Delay(1000);
        }

        public async Task Method2()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task Method3()
        {
            await Task.Delay(500);
        }
    }
}";

        var expected1 = new DiagnosticResult(TaskDelayAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(11, 19, 11, 35)
            .WithArguments("Task.Delay");

        var expected2 = new DiagnosticResult(TaskDelayAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(16, 19, 16, 54)
            .WithArguments("Task.Delay");

        var expected3 = new DiagnosticResult(TaskDelayAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(21, 19, 21, 34)
            .WithArguments("Task.Delay");

        await VerifyAnalyzerAsync(testCode, expected1, expected2, expected3);
    }

    /// <summary>
    /// Test that Task.Delay in async lambda triggers an error.
    /// </summary>
    [Fact]
    public async Task TaskDelayInAsyncLambda_ShouldTriggerError()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestService
    {
        public void DoWork()
        {
            Func<Task> action = async () => await Task.Delay(1000);
        }
    }
}";

        var expected = new DiagnosticResult(TaskDelayAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithSpan(11, 51, 11, 67)
            .WithArguments("Task.Delay");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Test that other Task methods don't trigger the analyzer.
    /// </summary>
    [Fact]
    public async Task OtherTaskMethods_ShouldNotTriggerError()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestService
    {
        public async Task DoWork()
        {
            await Task.CompletedTask;
            await Task.Yield();
            await Task.Run(() => Console.WriteLine(""Hello""));
            await Task.WhenAll(Task.CompletedTask, Task.CompletedTask);
        }
    }
}";

        // No diagnostic expected - these are other Task methods, not Task.Delay
        await VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Test that custom Delay method on a different type doesn't trigger.
    /// </summary>
    [Fact]
    public async Task CustomDelayMethod_ShouldNotTriggerError()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class MyCustomClass
    {
        public static Task Delay(int milliseconds)
        {
            return Task.CompletedTask;
        }
    }

    public class TestService
    {
        public async Task DoWork()
        {
            await MyCustomClass.Delay(1000);
        }
    }
}";

        // No diagnostic expected - this is not System.Threading.Tasks.Task.Delay
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
    /// Custom test class for the TaskDelayAnalyzer.
    /// </summary>
    private class Test : CSharpAnalyzerTest<TaskDelayAnalyzer, DefaultVerifier>
    {
        public Test()
        {
            // Configure test to use C# 12 and .NET 9
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
        }
    }
}
