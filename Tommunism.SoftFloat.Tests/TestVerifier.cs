namespace Tommunism.SoftFloat.Tests;

// NOTE: This is not thread safe!

/// <summary>
/// Invokes the external "testfloat_ver" program with the given options and allows results (with their associated arguments) to be added to
/// the verify queue.
/// </summary>
/// <remarks>
/// The only "good" way to identify failures is by reading the process's exit code (when Stop() is called). Parsing the output is feasible,
/// but requires a lot of detailed knowledge of how it generates error messages.
/// </remarks>
internal class TestVerifier
{
    #region Fields

    private TestRunnerOptions? _options;
    private Process? _process;
    private StreamWriter? _writer;
    private bool _writeDebug;

    #endregion

    #region Properties

    public TestRunnerOptions Options
    {
        get => _options ??= new(); // lazy initialized
        set => _options = value;
    }

    public string TestFunction { get; set; } = string.Empty;

    public TextWriter? ResultsWriter { get; set; }

    public TextWriter? DebugWriter { get; set; }

    public Process? Process => _process;

    #endregion

    #region Methods

    public void Start()
    {
        if (_process != null)
            throw new InvalidOperationException("Verifier process is already running.");

        Debug.Assert(Program.FunctionInfos.ContainsKey(TestFunction), "Test function not found in hard coded functions.");

        // Use an internal flag in case the DebugWriter property changes. This is used for automatic STDERR event handling.
        _writeDebug = DebugWriter != null;

        // Create the process and set up the arguments.
        _process = new Process
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
        Options.SetupVerifierArguments(_process.StartInfo.ArgumentList);
        _process.StartInfo.ArgumentList.Add(TestFunction);

        // Create handler for output data.
        _process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            ResultsWriter?.WriteLine(e.Data);
        });

        // Create handler for debug data (if needed).
        if (_writeDebug)
        {
            _process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                var builder = new ValueStringBuilder(stackalloc char[128]);

                // TODO: Does the program use backspace characters? That would add even more complexity to this.

                // TODO: '\r' by itself is probably already handled as a new line by the main handler.

                // NOTE: The error output from testfloat_ver may use '\r' characters to clear the console output line. I'm just
                // going to split those out into separate lines.
                var lineIndex = 0;
                var span = e.Data.AsSpan();
                do
                {
                    ReadOnlySpan<char> line;
                    var carriageReturnIndex = span.IndexOf('\r');
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
                    //builder.Length = 0;
                    //builder.Append("ERR");
                    //if (lineIndex != 0)
                    //{
                    //    builder.Append('[');
                    //    builder.Append(lineIndex.ToString()); // TODO: optimize this?
                    //    builder.Append(']');
                    //}
                    //builder.Append(": ");
                    //builder.Append(line);
                    //Debug.WriteLine(builder.AsSpan().ToString()); // Too bad this requires some kind of string allocation.

                    DebugWriter?.WriteLine(line);

                    lineIndex++;
                }
                while (!span.IsEmpty);

                // Release any allocated memory.
                builder.Dispose();
            });
        }

        // Start process and begin reading output and error data.
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        _writer = _process.StandardInput;
    }

    // Returns the exit code for the process (may return zero if no process was started).
    public int Stop()
    {
        // Do nothing if the process is not running.
        if (_process == null)
            return 0;

        try
        {
            Debug.Assert(_writer != null, "Process is not null, but writer is null.");

            // Tell the verifier process that we're done verifying data. This is usually done by writing the EOF character. (Unit
            // may allow signals to be send, but that may not work on Windows.) Unfortunately this character appears to be platform-
            // specific... Windows uses "Control-Z" (char 26), and Unix uses "Control-D" (char 4) for EOF.
            var eofChar = (char)(Environment.OSVersion.Platform == PlatformID.Win32NT ? 0x1A : 0x04);
            _writer.Write(eofChar);
            _writer.Flush();

            // Wait for verifier process to close normally.
            _process.WaitForExit();

            // Stop reading output (STDOUT and STDERR) data.
            _process.CancelOutputRead();
            _process.CancelErrorRead();

            return _process.ExitCode;
        }
        finally
        {
            _writer?.Close();
            _writer = null;

            _process.Dispose();
            _process = null;
        }
    }

    // TODO: Add Cancel() method to stop (almost) immediately?

    public void AddResult(TestRunnerArguments arguments, TestRunnerResult result, bool appendResultToArguments = false)
    {
        arguments.SetResult(result, appendResultToArguments);
        AddResult(arguments);
    }

    // NOTE: The last two arguments are always the resuling value and exception flags (see TestRunnerResult).
    public void AddResult(TestRunnerArguments result, bool flush = true)
    {
        if (_process == null)
            throw new InvalidOperationException("Verifier process is not running.");

        Debug.Assert(_writer != null, "Verifier process is running, but STDIN is null.");

        // Format result into buffer (with new line sequence).
        var builder = new ValueStringBuilder(stackalloc char[128]);
        result.WriteTo(ref builder);
        builder.Append(Environment.NewLine);

        // Write contents to verifier and flush (to make sure it gets handled by the process quickly as possible).
        _writer.Write(builder.AsSpan());
        if (flush)
            _writer.Flush();

        // Release any allocated memory.
        builder.Dispose();
    }

    public void FlushResults()
    {
        if (_process == null)
            throw new InvalidOperationException("Verifier process is not running.");

        Debug.Assert(_writer != null, "Verifier process is running, but STDIN is null.");

        _writer.Flush();
    }

    #endregion
}
