using System.Collections;

namespace Tommunism.SoftFloat.Tests;

internal abstract class GenerateCasesBase : IEnumerable<TestRunnerArguments>
{
    #region Fields

    private int _level;
    private long? _totalCountCached;

    #endregion

    #region Constructors

    protected GenerateCasesBase(int level = 1)
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
    /// Changing this value should affect the <see cref="TotalCount"/>. Never change this while generating test cases.
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
    /// Populates the test case arguments for the given test case index.
    /// </summary>
    /// <param name="index">The test case index. Must be greater than or equal to zero and less than <see cref="TotalCount"/>.</param>
    /// <param name="arguments">The test argument(s) to generate.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than <see cref="TotalCount"/>.</exception>
    public abstract TestRunnerArguments GenerateTestCase(long index);

    public IEnumerable<TestRunnerArguments> GenerateTestCases(long startIndex = 0)
    {
        long totalCount = TotalCount;

        if (startIndex < 0 || startIndex >= totalCount)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        for (long i = startIndex; i < totalCount; i++)
            yield return GenerateTestCase(i);
    }

    public IEnumerable<TestRunnerArguments> GenerateTestCases(long startIndex, long count)
    {
        long totalCount = TotalCount;

        if (startIndex < 0 || startIndex >= totalCount)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count < 0 || totalCount - startIndex < count)
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
    protected virtual void OnLevelChanged() { }

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
