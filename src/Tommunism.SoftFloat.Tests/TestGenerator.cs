namespace Tommunism.SoftFloat.Tests;

/// <summary>
/// Invokes the external "testfloat_gen" program with the given options and enumerates the results.
/// </summary>
internal class TestGenerator
{
    #region Fields

    private TestRunnerOptions? _options;

    #endregion

    #region Properties

    public TestRunnerOptions Options
    {
        get => _options ??= new(); // lazy initialized
        set => _options = value;
    }

    public string TestTypeOrFunction { get; set; } = string.Empty;

    // NOTE: This should only be used if the test function is a "type" (to indicate the number of arguments).
    public int TypeOperandCount { get; set; } = 0;

    #endregion

    #region Methods

    public IEnumerable<TestRunnerArguments> GenerateTestData() => GenerateTestData(CancellationToken.None);

    // NOTE: FormatException may be thrown if output from the generator cannot be parsed.
    // NOTE: InvalidOperationException may be thrown if generator's exit code was not zero.
    public virtual IEnumerable<TestRunnerArguments> GenerateTestData(CancellationToken cancellationToken)
    {
        Process? generatorProcess = null;

        // Check if aborted early.
        if (cancellationToken.IsCancellationRequested)
            yield break;

        try
        {
            // Create the process and set up the arguments.
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
            Options.SetupGeneratorArguments(generatorProcess.StartInfo.ArgumentList);
            generatorProcess.StartInfo.ArgumentList.Add(TestTypeOrFunction);
            if (TypeOperandCount > 0)
                generatorProcess.StartInfo.ArgumentList.Add(TypeOperandCount.ToString());

            // Start the process.
            generatorProcess.Start();

            using var generatorReader = generatorProcess.StandardOutput;
            var fastLineReader = new FastLineReader(generatorReader);

            // NOTE: Not checking for process exit, because there may still be pending data in STDOUT.
            while (!cancellationToken.IsCancellationRequested)
            {
                var readLine = fastLineReader.ReadLine();
                if (!readLine.HasValue)
                    break;

                // Skip trim line and skip if it is empty (or whitespace).
                var line = readLine.Value.Span.Trim();
                if (line.IsEmpty)
                    continue;

                // Try to parse the generator line.
                if (!TestRunnerArguments.TryParse(line, out var inputArguments))
                    throw new FormatException($"Could not parse generator line: {readLine}");

                yield return inputArguments;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                generatorProcess.StandardOutput.Close();
                generatorProcess.WaitForExit();
                if (generatorProcess.ExitCode != 0)
                    throw new InvalidOperationException($"TestFloat generator process returned an exit code of {generatorProcess.ExitCode}");
            }
        }
        finally
        {
            if (generatorProcess != null)
            {
                generatorProcess.StandardOutput.Close();
                generatorProcess.Kill();
            }
        }
    }

    #endregion

    #region Nested Types

    // This returns mutable Memory<char> objects instead of the String that StreamReader returns.
    // Just don't rely on or modify the contents of the memory after calling ReadLine() again!
    internal sealed class FastLineReader
    {
        private readonly TextReader _reader;
        private char[] _buffer;
        private int _lineStartPosition;
        private int _lineEndPosition;
        private bool _endOfFile;

        public FastLineReader(TextReader reader, int bufferSize = 1024)
        {
            _reader = reader;
            _buffer = new char[bufferSize];
        }

        public Memory<char>? ReadLine()
        {
            Memory<char> line;
            int newLineIndex, newLineLength;

            // Check for a new line sequence in existing buffer contents (avoids an unnecessary read).
            newLineIndex = FindNewLineSequence(_endOfFile, out newLineLength);
            if (newLineIndex >= 0)
            {
                // Get line memory and update line position for next line.
                line = _buffer.AsMemory(_lineStartPosition, newLineIndex);
                _lineStartPosition += newLineIndex + newLineLength;
                if (_lineStartPosition == _lineEndPosition)
                    _lineStartPosition = _lineEndPosition = 0;
                return line;
            }

            if (_endOfFile && _lineStartPosition != _lineEndPosition)
            {
                // No more chars available, so return whatever is remaining in the buffer.
                line = _buffer.AsMemory(_lineStartPosition, _lineEndPosition - _lineStartPosition);
                _lineStartPosition = _lineEndPosition = 0;
                return line;
            }

            // Read memory and try to find a new line sequence.
            while (!_endOfFile)
            {
                if (_lineEndPosition >= _buffer.Length)
                {
                    // Grow the buffer or move its contents to the beginning of the buffer so more data can be read.
                    MoveBufferContentsToBeginningOrGrow();
                }

                var oldLineEndPosition = _lineEndPosition;
                ReadIntoBuffer();

                // Search for new line sequence in new buffer contents. Read one character before old end of buffer (if
                // possible) in case the previous contents ended with a '\r' character (so either '\r' or '\r\n' can be detected
                // successfully).
                var newContentsStart = Math.Max(_lineStartPosition, oldLineEndPosition - 1);
                var newContentsSpan = _buffer.AsSpan(newContentsStart.._lineEndPosition);
                newLineIndex = FindNewLineSequence(newContentsSpan, _endOfFile, out newLineLength);
                if (newLineIndex >= 0)
                {
                    // Since we started in the middle of the line buffer, we need to update the new line index to the actual
                    // index relative to the line start position.
                    newLineIndex += newContentsStart - _lineStartPosition;

                    // Get line memory and update line position for next line.
                    line = _buffer.AsMemory(_lineStartPosition, newLineIndex);
                    _lineStartPosition += newLineIndex + newLineLength;
                    if (_lineStartPosition == _lineEndPosition)
                        _lineStartPosition = _lineEndPosition = 0;
                    return line;
                }
            }

            if (_lineStartPosition != _lineEndPosition)
            {
                // No more chars available, so return whatever is remaining in the buffer.
                line = _buffer.AsMemory(_lineStartPosition, _lineEndPosition - _lineStartPosition);
                _lineStartPosition = _lineEndPosition = 0;
                return line;
            }

            // Return null indicating no more lines available.
            return null;
        }

        // Returns -1 if no new line sequence was found in the buffer region. Index is relative to the start of the line.
        // NOTE: This will return -1 if the last character in the buffer is '\r', unless lastBlock parameter is true. (This is
        // because there may be a '\r\n' sequence, but there is no way of knowing for sure unless another chunk of characters
        // is read.)
        private int FindNewLineSequence(bool lastBlock, out int newLineLength)
        {
            var span = _buffer.AsSpan(_lineStartPosition, _lineEndPosition - _lineStartPosition);
            return FindNewLineSequence(span, lastBlock, out newLineLength);
        }

        private static int FindNewLineSequence(ReadOnlySpan<char> span, bool lastBlock, out int newLineLength)
        {
            if (span.Length > 0)
            {
                // Use IndexOfAny as it is likely to be faster than a naive char-by-char loop.
                var index = span.IndexOfAny('\r', '\n');
                if (index >= 0)
                {
                    var c = span[index];
                    if (c == '\r')
                    {
                        // Need at least two characters to guarantee that it is just '\r' and not '\r\n'.
                        var twoCharsAvailable = span.Length - 2 >= index;
                        if (lastBlock || twoCharsAvailable)
                        {
                            // Check if it is '\r\n' or just '\r'.
                            newLineLength = (twoCharsAvailable && span[index + 1] == '\n') ? 2 : 1;
                            return index;
                        }
                    }
                    else if (c == '\n')
                    {
                        // We don't recognize '\n\r' sequences as I don't believe any modern operating systems use it.
                        newLineLength = 1;
                        return index;
                    }
                    else
                    {
                        Debug.Fail("Unimplemented/unsupported new line character.");
                    }
                }
            }

            // No new line sequence was detected.
            newLineLength = 0;
            return -1;
        }

        private void MoveBufferContentsToBeginningOrGrow()
        {
            Debug.Assert(_lineEndPosition >= _lineStartPosition, "Line positions are in an invalid state.");

            if (_lineStartPosition == 0)
            {
                // Buffer is already at position zero, so it must be too small--need to reallocate.
                var newBufferSize = checked(_buffer.Length * 2);
                //Trace.TraceInformation($"Line buffer reallocating from {_buffer.Length} to {newBufferSize} (current line contents = {_lineStartPosition}..{_lineEndPosition})");
                Array.Resize(ref _buffer, newBufferSize); // TODO: Use ArrayPool?
            }
            else
            {
                //Trace.TraceInformation($"Line buffer contents ({_lineStartPosition}..{_lineEndPosition}) moving to index zero.");
                var length = _lineEndPosition - _lineStartPosition;
                _buffer.AsSpan(_lineStartPosition, length).CopyTo(_buffer);

                _lineStartPosition = 0;
                _lineEndPosition = length;
            }
        }

        private void ReadIntoBuffer()
        {
            Debug.Assert(_lineEndPosition < _buffer.Length, "Character buffer is full.");
            Debug.Assert(_lineEndPosition >= _lineStartPosition, "Line positions are in an invalid state.");

            var charsRead = _reader.Read(_buffer, _lineEndPosition, _buffer.Length - _lineEndPosition);
            if (charsRead < 0)
                throw new IOException("Read() returned a negative number.");

            _lineEndPosition += charsRead;
            _endOfFile = charsRead == 0;
        }
    }

    #endregion
}
