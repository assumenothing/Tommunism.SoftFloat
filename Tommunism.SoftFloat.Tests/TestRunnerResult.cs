namespace Tommunism.SoftFloat.Tests;

internal record struct TestRunnerResult(TestRunnerArgument Value, ExceptionFlags ExceptionFlags)
{
    #region Additional Constructors

    public TestRunnerResult(TestRunnerArguments args) : this(args[^2], args[^1].ToExceptionFlags()) { }

    public TestRunnerResult(bool value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value ? 1U : 0, TestRunnerArgumentKind.Bits1), ExceptionFlags) { }

    public TestRunnerResult(byte value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value, TestRunnerArgumentKind.Bits8), ExceptionFlags) { }

    public TestRunnerResult(ushort value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value, TestRunnerArgumentKind.Bits16), ExceptionFlags) { }

    public TestRunnerResult(uint value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value, TestRunnerArgumentKind.Bits32), ExceptionFlags) { }

    public TestRunnerResult(ulong value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value, TestRunnerArgumentKind.Bits64), ExceptionFlags) { }

    public TestRunnerResult(sbyte value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument((byte)value, TestRunnerArgumentKind.Bits8), ExceptionFlags) { }

    public TestRunnerResult(short value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument((ushort)value, TestRunnerArgumentKind.Bits16), ExceptionFlags) { }

    public TestRunnerResult(int value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument((uint)value, TestRunnerArgumentKind.Bits32), ExceptionFlags) { }

    public TestRunnerResult(long value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument((ulong)value, TestRunnerArgumentKind.Bits64), ExceptionFlags) { }

    public TestRunnerResult(UInt128 value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value, TestRunnerArgumentKind.Bits128), ExceptionFlags) { }

    public TestRunnerResult(Int128 value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument((UInt128)value, TestRunnerArgumentKind.Bits128), ExceptionFlags) { }

    public TestRunnerResult(Float16 value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value.ToUInt16Bits(), TestRunnerArgumentKind.Bits16), ExceptionFlags) { }

    public TestRunnerResult(Float32 value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value.ToUInt32Bits(), TestRunnerArgumentKind.Bits32), ExceptionFlags) { }

    public TestRunnerResult(Float64 value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value.ToUInt64Bits(), TestRunnerArgumentKind.Bits64), ExceptionFlags) { }

    public TestRunnerResult(ExtFloat80 value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value.ToUInt128Bits(), TestRunnerArgumentKind.Bits80), ExceptionFlags) { }

    public TestRunnerResult(Float128 value, ExceptionFlags ExceptionFlags) : this(new TestRunnerArgument(value.ToUInt128Bits(), TestRunnerArgumentKind.Bits128), ExceptionFlags) { }

    #endregion
}
