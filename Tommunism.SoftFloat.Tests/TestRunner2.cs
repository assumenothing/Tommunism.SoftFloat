﻿using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat.Tests;

// TODO: Use a single thread only when MaxVerifierProcesses is set to 1. This should help when debugging as there aren't multiple tasks
// getting in the way.

// TODO: Share the semaphore between multiple run operations? This could allow for greater concurrency when running multiple
// configurations. But it would be nice if there was a method for waiting until the previous generator finishes starting run/verify tasks
// before starting the next batch of test runs.

// TODO: Use System.Collections.Concurrent.Partitioner for scheduling the work? This might result in better throughput, possibly at the end
// when some of the longer running tasks are remaining (e.g., modulus takes more variable amounts of time than the others).

internal class TestRunner2
{
    #region Fields

    private TestRunnerOptions? _options;

    // Recycle results arrays so they can be reused for multiple verifier tasks (should reduce GC overhead for the large object heap).
    private readonly ConcurrentBag<TestRunnerArguments[]> _testArgsPool = new();

    #endregion

    #region Properties

    // If not set, then then Environment.ProcessorCount is used. Technically this could probably be one less (because the generator thread
    // may take a long time too), but this assumes that performing the test and verifying will take longer than the generator. The
    // generator will probably also wait for very large numbers of tests to complete before generating more tests (sort of).
    public int MaxVerifierProcesses { get; set; } = 0;

    // Used to split generated tests between multiple execution/verifier threads.
    public int MaxTestsPerProcess { get; set; } = 1_000_000; // increasing this adds significant memory costs, but should reduce the number of tasks/processes which have to be spawned

    public TestRunnerOptions Options
    {
        get => _options ??= new(); // lazy initialized
        set => _options = value;
    }

    // NOTE: If this is different than the TestFunction, then results will be appended (as it is assumed that this property is a type code instead).
    public string? GeneratorTypeOrFunction { get; set; } = null;

    public int GeneratorTypeOperandCount { get; set; } = 0;

    public string TestFunction { get; set; } = string.Empty;

    public Func<TestRunnerState, TestRunnerArguments, TestRunnerResult>? TestHandler { get; set; }

    // Allow setter so it can be reset back to zero between test runs (if that's what the caller wants to do).
    public long TotalTestCount { get; set; } = 0;

    #endregion

    #region Methods

    public Task<bool> RunAsync() => RunAsync(CancellationToken.None);

    // TODO: Add support for cancellation? May not be complete...
    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        var options = Options;
        var testFunction = TestFunction;
        var generatorTypeOrFunction = GeneratorTypeOrFunction;
        var generatorTypeOperandCount = generatorTypeOrFunction != null ? GeneratorTypeOperandCount : 0;
        var testHandler = TestHandler;

        if (testHandler == null)
            throw new InvalidOperationException("Test handler is not defined.");

        // Calculate the maximum number of verifier processes that can run at any given time for this test runner instance.
        var verifierProcessCount = MaxVerifierProcesses;
        if (verifierProcessCount <= 0)
            verifierProcessCount = Environment.ProcessorCount;
        //Trace.TraceInformation($"VerifierProcessCount = {verifierProcessCount}");

        var maxTestsPerProcess = MaxTestsPerProcess;

        // Only a certain number of verifier tasks can run at any given time, so use a semaphore to limit them.
        using var semaphore = new SemaphoreSlim(verifierProcessCount, verifierProcessCount);

        // Create the generator instance.
        var generator = new TestGenerator()
        {
            Options = options,
            TestTypeOrFunction = generatorTypeOrFunction ?? testFunction,
            TypeOperandCount = generatorTypeOperandCount
        };

        // We only need to append the results to the arguments if the generator function is a type code (which can be detected if it doesn't match the test function).
        var state = new TestRunnerState(this, options, testFunction, testHandler,
            AppendResultsToArguments: !string.Equals(testFunction, generator.TestTypeOrFunction, StringComparison.Ordinal));

        // Keep track of the number of test blocks that failed.
        // Also keep track of the exceptions that were thrown.
        var testProcessFailures = 0;
        var testProcessExceptions = new ConcurrentBag<Exception>();

        // Start generating and running tests.
        int testCount = 0;
        TestRunnerArguments[]? testArgsQueue = null;

        void RunTestsImpl(object? taskState)
        {
            Debug.Assert(taskState != null && taskState is ReadOnlyMemory<TestRunnerArguments>);
            Debug.Assert(semaphore != null);
            Debug.Assert(state is not null);

            var testArguments = (ReadOnlyMemory<TestRunnerArguments>)taskState;
            try
            {
                Trace.TraceInformation($"[{Task.CurrentId}] Started new test run/verify task.");
                var stopwatch = Stopwatch.StartNew();

                var result = RunTestsCore(state, testArguments, cancellationToken);
                if (result != 0)
                    Interlocked.Increment(ref testProcessFailures);

                stopwatch.Stop();
                Trace.TraceInformation($"[{Task.CurrentId}] Task finished in {stopwatch.Elapsed.TotalSeconds:f3} seconds with result {result} ({(result == 0 ? "PASS" : "FAIL")}).");
            }
            catch (Exception ex)
            {
                testProcessExceptions.Add(ex);

#if DEBUG
                // Make it a little bit easier to debug when something goes wrong?
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
            }
            finally
            {
                // Release the tests array back into the pool (if it was actually an array).
                if (MemoryMarshal.TryGetArray(testArguments, out var testArgumentsArray))
                {
                    Debug.Assert(testArgumentsArray.Array != null);
                    _testArgsPool.Add(testArgumentsArray.Array);
                }

                // Release the semaphore and allow more tasks to run in its place.
                semaphore.Release();
            }
        }

        foreach (var testArgs in generator.GenerateTestData(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (testArgsQueue == null)
            {
                // Get the next available test args data from the pool or create a new array.
                if (!_testArgsPool.TryTake(out testArgsQueue))
                {
                    testArgsQueue = new TestRunnerArguments[maxTestsPerProcess];
                }
#if DEBUG
                else
                {
                    Array.Clear(testArgsQueue);
                }
#endif
            }

            // Add the current test arguments to the queue.
            testArgsQueue[testCount++] = testArgs;
            TotalTestCount++;

            // If the queue is full, then try to run the tests.
            if (testCount >= maxTestsPerProcess)
            {
                Debug.Assert(testCount == maxTestsPerProcess);

                // Wait on the semaphore to be able to run the next task.
                await semaphore.WaitAsync(cancellationToken);

                // Get the subset of tests to run and clear the current args state.
                ReadOnlyMemory<TestRunnerArguments> tests = testArgsQueue.AsMemory(0, testCount);
                testCount = 0;
                testArgsQueue = null;

                // Run the tests asynchronously. We don't need to track it, because the semaphore handles it (mostly).
                _ = Task.Factory.StartNew(RunTestsImpl, tests, CancellationToken.None);
            }
        }

        if (testCount > 0 && testArgsQueue != null)
        {
            // Wait on the semaphore to be able to run the next task.
            await semaphore.WaitAsync(cancellationToken);

            // Get the subset of tests to run.
            ReadOnlyMemory<TestRunnerArguments> tests = testArgsQueue.AsMemory(0, testCount);

            // Run the tests asynchronously. We don't need to track it, because the semaphore handles it (mostly).
            _ = Task.Factory.StartNew(RunTestsImpl, tests, CancellationToken.None);
        }

        // Use the semaphore to wait for the max number of tasks. If they can all be consumed, then all tasks are complete.
        Trace.TraceInformation($"Generator finished generating tests. Waiting for tasks to finish ({verifierProcessCount - semaphore.CurrentCount} / {verifierProcessCount} threads currently running).");
        for (var i = 0; i < verifierProcessCount; i++)
            await semaphore.WaitAsync(cancellationToken);

        // Results are in an unknown state if cancelled. Just throw if that happens.
        cancellationToken.ThrowIfCancellationRequested();

        // Throw aggregate exception if any were thrown in the individual run tasks.
        if (!testProcessExceptions.IsEmpty)
            throw new AggregateException(testProcessExceptions);

        // Return false if the number of failed test run blocks is not zero.
        return testProcessFailures == 0;
    }

    private int RunTestsCore(TestRunnerState state, ReadOnlyMemory<TestRunnerArguments> tests, CancellationToken cancellationToken)
    {
        if (tests.Length == 0)
            return 0;

        var testsSpan = tests.Span;
        var testHandler = TestHandler;
        Debug.Assert(testHandler != null);

        // Make a copy of the state. This is because thread-specific contexts are required.
        state = new TestRunnerState(this, state.Options, state.TestFunction, state.TestFunctionHandler, state.AppendResultsToArguments);

        // Create the verifier instance.
        var verifier = new TestVerifier()
        {
            Options = state.Options,
            TestFunction = state.TestFunction,
            ResultsWriter = Console.Out // TODO: add property for choosing/generating the output streams
        };

        // Start the verifier process.
        verifier.Start();
        try
        {
            // Run tests against verifier.
            for (var i = 0; i < testsSpan.Length; i++)
            {
                // TODO: Only check for this every few tests to reduce overhead?
                cancellationToken.ThrowIfCancellationRequested();

                // Run test and get result.
                var testData = testsSpan[i];
                var result = testHandler(state, testData);
                testData.SetResult(result, state.AppendResultsToArguments);

                // Add test results to verifier queue.
                verifier.AddResult(testData);
            }

            // Stop the verifier process and return the result.
            var verifierResult = verifier.Stop();
            return verifierResult;
        }
        finally
        {
            var verifierProcess = verifier.Process;
            if (verifierProcess != null)
            {
                if (!verifierProcess.HasExited)
                    verifierProcess.Kill();
                verifierProcess.Dispose();
            }
        }
    }

    #endregion
}
