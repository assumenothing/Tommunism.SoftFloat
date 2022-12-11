namespace Tommunism.SoftFloat.Tests;

// NOTE: This does not use any multi-threading, it is up to the caller to encapsulate the Run() method if that is the desired operation.
internal class TestRunner
{
    #region Fields

    // TODO?

    #endregion

    #region Constructors

    public TestRunner()
    {
    }

    #endregion

    #region Properties

    // Prints each line read from the generator and written to the verifier as well as the results read from the verifier.
    // TODO: Add different levels of output (debug/progress/error lines are probably unnecessary unless using the highest level of detail).
    // 0 - doesn't display anything to debug or console output
    // 1 - only reports verifier STDOUT messages
    // 2 - reports generator STDOUT, executed test results, and verifier process exit code along with above (useful for debugging, but pretty verbose)
    // 3 - reports everything (extremely verbose)
    public int ConsoleDebug { get; set; } = 0;

    public bool SkipVerifierProcess { get; set; } = false;

    #endregion

    #region Methods

    public Task<int> RunAsync(string testFunction, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult> testHandler, TestRunnerOptions options) =>
        RunAsync(testFunction, testHandler, options, CancellationToken.None);

    public Task<int> RunAsync(string testFunction, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult> testHandler, TestRunnerOptions options, CancellationToken cancellationToken) =>
        Task.Factory.StartNew(() => Run(testFunction, testHandler, options, cancellationToken), TaskCreationOptions.LongRunning);

    public int Run(string testFunction, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult> testHandler, TestRunnerOptions options) =>
        Run(testFunction, testHandler, options, CancellationToken.None);

    // NOTE: This is a blocking operation! It is highly recommended to use some kind of thread pool or parallel execution. This is not thread-safe, do not call more than once at a time!
    public int Run(string testFunction, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult> testHandler, TestRunnerOptions options, CancellationToken cancellationToken)
    {
        Process? generatorProcess = null;
        Process? verifierProcess = null;

        var testContext = new TestRunnerState(this, options, testFunction, testHandler);

        // Buffer for lines to send to the verifier (or log file).
        var verifierLineBuffer = new ValueStringBuilder(stackalloc char[128]);

        int? verifierExitCode = null;

        try
        {
            // Try to abort as early as possible if cancellation is requested.
            if (!cancellationToken.IsCancellationRequested)
            {
                // Create and start generator process.
                generatorProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Program.GeneratorCommandPath,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                options.SetupGeneratorArguments(generatorProcess.StartInfo.ArgumentList);
                generatorProcess.StartInfo.ArgumentList.Add(testFunction);

                // Create and start verifier process.
                if (!SkipVerifierProcess)
                {
                    verifierProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Program.VerifierCommandPath,
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };
                    options.SetupVerifierArguments(verifierProcess.StartInfo.ArgumentList);
                    verifierProcess.StartInfo.ArgumentList.Add(testFunction);

                    // TODO: Theoretically this same trick could be used for the generator.
                    // TODO: Do the same with ErrorDataReceived? (Tougher to do, because it is used for indicating progress and other debugging info.)
                    verifierProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        // TODO: Write data to log file (or console).
                        if (ConsoleDebug >= 1)
                        {
                            var message = $"VER: {e.Data}";
                            Console.WriteLine(message);
                            Debug.WriteLine(message);
                        }
                    });

                    // Are we being extremely verbose?
                    if (ConsoleDebug >= 2)
                    {
                        verifierProcess.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            var builder = new ValueStringBuilder(stackalloc char[128]);

                            // NOTE: The error output from testfloat_ver uses a lot of '\r' characters to clear the console output. I'm just going to split those out into separate lines.
                            // TODO: Does the program use backspace characters? That would add even more complexity to this.
                            var lineIndex = 0;
                            var span = e.Data.AsSpan();
                            do
                            {
                                var carriageReturnIndex = span.IndexOf('\r');
                                ReadOnlySpan<char> line;
                                if (carriageReturnIndex >= 0)
                                {
                                    line = span[..carriageReturnIndex];
                                    span = span[(carriageReturnIndex + 1)..];
                                }
                                else
                                {
                                    line = span;
                                    span = ReadOnlySpan<char>.Empty;
                                }

                                // Use an indexer to make it obvious that there are line continuations (a.k.a. line resets/overwrites)
                                builder.Length = 0;
                                builder.Append("ERR");
                                if (lineIndex != 0)
                                {
                                    builder.Append('[');
                                    builder.Append(lineIndex.ToString()); // TODO: optimize this?
                                    builder.Append(']');
                                }
                                builder.Append(": ");
                                builder.Append(line);

                                Console.Error.WriteLine(builder.AsSpan());
                                Debug.WriteLine(builder.AsSpan().ToString()); // Too bad this requires some kind of string allocation.

                                lineIndex++;
                            }
                            while (!span.IsEmpty);
                        });
                    }
                }

                generatorProcess.Start();

                if (verifierProcess != null)
                {
                    verifierProcess.Start();
                    verifierProcess.BeginOutputReadLine();
                    verifierProcess.BeginErrorReadLine();
                }

                using var generatorReader = generatorProcess.StandardOutput;
                using var verifierWriter = verifierProcess?.StandardInput;

                // TODO: Buffer for reading lines?

                // TODO: If the verifier process quits before the generator, then there are probably too many errors.
                // (Figure out a good way to handle that, if necessary.)

                while (!(verifierProcess?.HasExited ?? false) &&
                    !cancellationToken.IsCancellationRequested)
                {
                    // TODO: Is there a more efficient way to do this without allocating a string every time?
                    var generatorLine = generatorReader.ReadLine();
                    if (generatorLine == null)
                        break;

                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(generatorLine))
                        continue;

                    if (ConsoleDebug >= 3)
                        Console.WriteLine($"GEN: {generatorLine}");

                    // Try to parse the generator line.
                    if (!TestRunnerArguments.TryParse(generatorLine, out var inputArguments))
                    {
                        Trace.TraceError($"Could not parse generator line: {generatorLine}");
                        break;
                    }

                    // Run the test with the given input arguments.
                    var result = testHandler(testContext, inputArguments);

                    // This is crude (as there are several ways to handle uncertainties in the IEEE spec, but this is good for basic debugging).
                    // Exception flags always appear to be correct, just the values might be specific to each implementation.
                    //Debug.Assert(result.Result == inputArguments[^2], "Resulting value does not match generator's expected value.");
                    //Debug.Assert(result.ExceptionFlags == inputArguments[^1].ToExceptionFlags(), "Resulting exception flags does not match generator's expected exception flags.");

                    // Update the input arguments so they can be used as verifier arguments.
                    inputArguments.SetResult(result);

                    // Compile the results into a line buffer.
                    verifierLineBuffer.Length = 0;
                    inputArguments.WriteTo(ref verifierLineBuffer);
                    {
                        // TODO: Write to log or STDOUT? (Before final new line sequence is appended.)
                        if (ConsoleDebug >= 3)
                            Console.WriteLine($"OUT: {verifierLineBuffer.AsSpan().ToString()}");
                    }
                    verifierLineBuffer.Append(Environment.NewLine);
                    var verifierLine = verifierLineBuffer.AsSpan();

                    if (verifierWriter != null)
                    {
                        // Write the line to the verifier's input and flush.
                        verifierWriter.Write(verifierLine);
                        verifierWriter.Flush();

                        // NOTE: Errors should automatically be handled by the OutputDataReceived event.
                    }
                }

                // Tell the verifier process that we're done verifying data. This is usually done by writing the EOF character.
                // (Unit may allow signals to be send, but that may not work on Windows.) Unfortunately this character appears to be platform specific...
                // Windows uses "Control-Z" (char 26), and Unix uses "Control-D" (char 4) for EOF.
                if (verifierWriter != null)
                {
                    var eofChar = (char)(Environment.OSVersion.Platform == PlatformID.Win32NT ? 0x1A : 0x04);
                    verifierWriter.Write(eofChar);
                    verifierWriter.Flush();

                    // Wait for verifier process to close normally.
                    verifierProcess?.WaitForExit();
                }

                verifierProcess?.CancelOutputRead();
                if (ConsoleDebug >= 2)
                    verifierProcess?.CancelErrorRead();
            }
        }
        finally
        {
            if (verifierProcess != null)
            {
                //verifierProcess.StandardInput.Close();
                verifierProcess.Kill();
                if (!cancellationToken.IsCancellationRequested)
                {
                    verifierProcess.WaitForExit();
                    verifierExitCode = verifierProcess.ExitCode;
                }
                verifierProcess = null;
            }

            if (generatorProcess != null)
            {
                generatorProcess.StandardOutput.Close();
                generatorProcess.Kill();
                if (!cancellationToken.IsCancellationRequested)
                    generatorProcess.WaitForExit();
                generatorProcess = null;
            }

            verifierLineBuffer.Dispose();
        }

        if (ConsoleDebug >= 1 && verifierExitCode.HasValue)
        {
            var message = $"Verifier process exit code = {verifierExitCode.Value}";
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }

        // Probably abnormal termination if the exit code is not set.
        return verifierExitCode ?? 1;
    }

    #endregion
}
