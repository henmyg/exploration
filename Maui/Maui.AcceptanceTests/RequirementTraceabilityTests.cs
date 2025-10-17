using System.Reflection;
using System.Text.RegularExpressions;

namespace Maui.AcceptanceTests;

/// <summary>
/// Tests that verify all acceptance criteria referenced in documentation
/// actually exist as test methods in the codebase.
/// </summary>
public class RequirementTraceabilityTests
{
    private const string DocumentationPath = @"..\..\..\..\..\Documentation\docs\features";

    [Fact]
    public void AllReferencedAcceptanceCriteria_ExistAsTests()
    {
        // Arrange
        var referencedTests = GetReferencedTestsFromDocumentation();
        var existingTests = GetAllTestMethodNames();

        // Act
        var missingTests = referencedTests.Except(existingTests).ToList();

        // Assert
        Assert.Empty(missingTests);
    }

    [Fact]
    public void CurrentPrice_AllAcceptanceCriteria_AreReferenced()
    {
        // Arrange
        var currentPriceDocPath = Path.Combine(DocumentationPath, "current-price.md");
        var referencedTests = GetReferencedTestsFromFile(currentPriceDocPath);
        var currentPriceTests = GetTestMethodNamesFromNamespace("Maui.AcceptanceTests.CurrentPrice");

        // Act
        var unreferencedTests = currentPriceTests.Except(referencedTests).ToList();

        // Assert - All tests should be referenced in documentation
        Assert.Empty(unreferencedTests.Select(t => $"Test '{t}' exists but is not referenced in current-price.md"));
    }

    private static HashSet<string> GetReferencedTestsFromDocumentation()
    {
        var tests = new HashSet<string>();
        var mdFiles = Directory.GetFiles(DocumentationPath, "*.md", SearchOption.AllDirectories);

        foreach (var file in mdFiles)
        {
            var referencedTests = GetReferencedTestsFromFile(file);
            foreach (var test in referencedTests)
            {
                tests.Add(test);
            }
        }

        return tests;
    }

    private static HashSet<string> GetReferencedTestsFromFile(string filePath)
    {
        var tests = new HashSet<string>();

        if (!File.Exists(filePath))
            return tests;

        var content = File.ReadAllText(filePath);

        // Match pattern: **AC**: `TestMethodName`
        var matches = Regex.Matches(content, @"\*\*AC\*\*:\s*`([^`]+)`");

        foreach (Match match in matches)
        {
            tests.Add(match.Groups[1].Value);
        }

        return tests;
    }

    private static HashSet<string> GetAllTestMethodNames()
    {
        var tests = new HashSet<string>();
        var assembly = Assembly.GetExecutingAssembly();

        var testMethods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<FactAttribute>().Any() ||
                       m.GetCustomAttributes<TheoryAttribute>().Any());

        foreach (var method in testMethods)
        {
            tests.Add(method.Name);
        }

        return tests;
    }

    private static HashSet<string> GetTestMethodNamesFromNamespace(string namespaceName)
    {
        var tests = new HashSet<string>();
        var assembly = Assembly.GetExecutingAssembly();

        var testMethods = assembly.GetTypes()
            .Where(t => t.Namespace == namespaceName)
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<FactAttribute>().Any() ||
                       m.GetCustomAttributes<TheoryAttribute>().Any());

        foreach (var method in testMethods)
        {
            tests.Add(method.Name);
        }

        return tests;
    }
}
