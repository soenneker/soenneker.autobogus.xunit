using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.AutoBogus.Config.Abstract;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Soenneker.AutoBogus.XUnit;

/// <summary>
/// Provides auto-generated data for test methods using AutoBogus.
/// This attribute can be used with xUnit v3 and supports automatic parameter injection.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class AutoBogusAttribute : DataAttribute
{
    /// <summary>
    /// Number of test cases to generate.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Optional seed for deterministic test runs.
    /// </summary>
    public int? Seed { get; }

    /// <summary>
    /// Optional configuration action to customize AutoBogus behavior.
    /// </summary>
    public Action<IAutoGenerateConfigBuilder>? Configure { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoBogusAttribute"/> class.
    /// </summary>
    /// <param name="count">Number of test cases to generate.</param>
    /// <param name="seed">Optional seed for deterministic test runs. Use -1 for random.</param>
    public AutoBogusAttribute(int count = 1, int seed = -1)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        Count = count;
        Seed = seed == -1 ? null : seed;
    }

    /// <summary>
    /// Returns the data to be used to test the theory.
    /// </summary>
    /// <param name="testMethod">The method that is being tested</param>
    /// <param name="disposalTracker">Tracker for disposing generated objects</param>
    /// <returns>Collection of theory data rows</returns>
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        if (testMethod == null)
            throw new ArgumentNullException(nameof(testMethod));

        ParameterInfo[] parameters = testMethod.GetParameters();
        var rows = new List<ITheoryDataRow>(Count);

        for (int i = 0; i < Count; i++)
        {
            var values = new object?[parameters.Length];

            for (int p = 0; p < parameters.Length; p++)
            {
                Type paramType = parameters[p].ParameterType;
                object? value = GenerateValue(paramType);

                // If you happen to generate disposables, hand them to xUnit:
                if (value is IDisposable d) disposalTracker.Add(d);
                if (value is IAsyncDisposable ad) disposalTracker.Add(ad);

                values[p] = value;
            }

            rows.Add(new TheoryDataRow(values!));
        }

        return new ValueTask<IReadOnlyCollection<ITheoryDataRow>>(rows);
    }

    /// <summary>
    /// Indicates whether this data source supports discovery enumeration.
    /// </summary>
    /// <returns>True if deterministic (has seed), false otherwise.</returns>
    public override bool SupportsDiscoveryEnumeration()
    {
        // Return false because data depends on runtime randomness (non-deterministic).
        // If you pass a Seed and only use it, you *could* return true.
        return Seed.HasValue;
    }

    /// <summary>
    /// Generates a value for the specified type using AutoBogus.
    /// </summary>
    /// <param name="type">The type to generate.</param>
    /// <returns>The generated value.</returns>
    private object? GenerateValue(Type type)
    {
        try
        {
            // Create AutoFaker with configuration if provided
            // Not fun creating a new one every time... but we'll come back to this
            var autoFaker = new AutoFaker(Configure);

            // Generate the data
            return autoFaker.Generate(type);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate data for parameter of type '{type.FullName}'. " +
                $"This might be due to the type not being supported by AutoBogus or having complex constructor requirements. " +
                $"Original error: {ex.Message}", ex);
        }
    }
}