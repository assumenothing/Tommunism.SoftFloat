using System.Collections;

namespace Tommunism.SoftFloat.Tests;

/// <summary>
/// Generates test cases to test against a given floating-point implementation.
/// </summary>
internal abstract class TestCaseGenerator : IEnumerable<TestRunnerArguments>
{
    #region Fields

    private int _level;
    private long? _totalCountCached;

    #endregion

    #region Constructors

    /// <summary>
    /// Instantiates a <see cref="TestCaseGenerator"/> using the given test level.
    /// </summary>
    /// <param name="level">The level of testing to for generating test cases. Currently only level 1 and 2 are supported. The higher the number, the more test cases that are generated.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is not 1 or 2.</exception>
    protected TestCaseGenerator(int level = 1)
    {
        if (level is < 1 or > 2)
            throw new ArgumentOutOfRangeException(nameof(level));

        _level = level;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of arguments generated in each test case.
    /// </summary>
    public abstract int ArgumentCount { get; }

    /// <summary>
    /// Gets or sets the testing level to use for the generated test cases. This value must be either 1 or 2.
    /// </summary>
    /// <remarks>
    /// Changing this value should affect the <see cref="TotalCount"/>. Do not change this while generating test cases.
    /// </remarks>
    public int Level
    {
        get => _level;

        set
        {
            if (value is < 1 or > 2)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (_level != value)
            {
                _level = value;
                OnLevelChanged();
            }
        }
    }

    /// <summary>
    /// Gets the total number of tests to generate based on the configuration options.
    /// </summary>
    /// <remarks>
    /// This property should never return a value less than zero.
    /// </remarks>
    public virtual long TotalCount => _totalCountCached ??= CalculateTotalCases();

    #endregion

    #region Methods

    /// <summary>
    /// Creates a new test case generator for a given type name and argument count.
    /// </summary>
    /// <param name="typeName">The type name to use when generating test cases. Supported values are: <c>ui32</c>, <c>ui64</c>, <c>i32</c>, <c>i64</c>, <c>f16</c>, <c>f32</c>, <c>f64</c>, <c>extF80</c>, and <c>f128</c>.</param>
    /// <param name="argumentCount">The number of arguments to generate for each test case. Integers types must be either 0 or 1 and floating-point types must be either 1, 2, or 3.</param>
    /// <param name="level">The level of testing to for generating test cases. Currently only level 1 and 2 are supported. The higher the number, the more test cases that are generated.</param>
    /// <returns>A test case generator supporting the supplied type name and argument count.</returns>
    /// <exception cref="NotImplementedException">Either <paramref name="typeName"/> or <paramref name="argumentCount"/> is not supported/implemented.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is not 1 or 2.</exception>
    /// <remarks>
    /// Just like TestFloat, no results are calculated when using these type names as there is no way of knowing what function these are being used with.
    /// </remarks>
    public static TestCaseGenerator FromTypeName(string typeName, int argumentCount, int level = 1) => typeName switch
    {
        "ui32" => TestCaseGeneratorUInt32.Create(argumentCount, level),
        "ui64" => TestCaseGeneratorUInt64.Create(argumentCount, level),
        "i32" => TestCaseGeneratorInt32.Create(argumentCount, level),
        "i64" => TestCaseGeneratorInt64.Create(argumentCount, level),
        "f16" => TestCaseGeneratorFloat16.Create(argumentCount, level),
        "f32" => TestCaseGeneratorFloat32.Create(argumentCount, level),
        "f64" => TestCaseGeneratorFloat64.Create(argumentCount, level),
        "extF80" => TestCaseGeneratorExtFloat80.Create(argumentCount, level),
        "f128" => TestCaseGeneratorFloat128.Create(argumentCount, level),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Creates a new test case generator for a given function name.
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="level"></param>
    /// <returns>A test case generator supported the supplied function name.</returns>
    /// <exception cref="NotImplementedException"><paramref name="functionName"/> is not supported/implemented.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is not 1 or 2.</exception>
    /// <remarks>
    /// Unlike TestFloat, this does not calculate the results of the function.
    /// </remarks>
    public static TestCaseGenerator FromFunctionName(string functionName, int level = 1) =>
        FunctionInfo.GeneratorTypes.TryGetValue(functionName, out var generatorType)
            ? FromTypeName(generatorType.TypeName, generatorType.ArgCount, level)
            : throw new NotImplementedException();

    /// <summary>
    /// Generates the test case arguments for the given test case index.
    /// </summary>
    /// <param name="index">The test case index. Must be greater than or equal to zero.</param>
    /// <returns>The generated test case arguments.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
    /// <remarks>
    /// Any implementation of <see cref="TestCaseGenerator"/> should be able to handle indexes greater than <see cref="TotalCount"/>,
    /// because the user may choose to run even more tests than the default.
    /// </remarks>
    public abstract TestRunnerArguments GenerateTestCase(long index);

    /// <summary>
    /// Generates test cases starting at the given index.
    /// </summary>
    /// <param name="startIndex">The index to start generating cases.</param>
    /// <returns>An enumerable containing all tests starting at the given index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> is less than zero.</exception>
    /// <remarks>
    /// If the <paramref name="startIndex"/> is greater than <see cref="TotalCount"/>, then no test cases will be generated.
    /// </remarks>
    public IEnumerable<TestRunnerArguments> GenerateTestCases(long startIndex = 0)
    {
        long totalCount = TotalCount;

        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        for (long i = startIndex; i < totalCount; i++)
            yield return GenerateTestCase(i);
    }

    /// <summary>
    /// Generates test cases in the given index range.
    /// </summary>
    /// <param name="startIndex">The index to start generating cases.</param>
    /// <param name="count">The total number of test cases to generate.</param>
    /// <returns>An enumerable containing all tests in the given range.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> or <paramref name="count"/> is less than zero.</exception>
    /// <remarks>
    /// Both <paramref name="startIndex"/> and <paramref name="count"/> can exceed <see cref="TotalCount"/>.
    /// </remarks>
    public IEnumerable<TestRunnerArguments> GenerateTestCases(long startIndex, long count)
    {
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var endIndex = startIndex + count;
        for (long i = startIndex; i < endIndex; i++)
            yield return GenerateTestCase(i);
    }

    /// <summary>
    /// Called when the <see cref="Level"/> property value changes.
    /// </summary>
    /// <remarks>
    /// Useful for updating/invalidating a cached value to use for <see cref="TotalCount"/>.
    /// </remarks>
    protected virtual void OnLevelChanged() => _totalCountCached = null;

    /// <summary>
    /// Calculates the total number of test cases for the current state.
    /// </summary>
    /// <returns>The total number of test cases that should be generated.</returns>
    /// <remarks>
    /// The default implementation returns zero. Either <see cref="CalculateTotalCases"/> should be overridden to cache values or
    /// <see cref="TotalCount"/> should be overridden without caching or to use a custom cache.
    /// </remarks>
    protected virtual long CalculateTotalCases() => 0;

    public IEnumerator<TestRunnerArguments> GetEnumerator()
    {
        long totalCount = TotalCount;
        for (long i = 0; i < totalCount; i++)
            yield return GenerateTestCase(i);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
