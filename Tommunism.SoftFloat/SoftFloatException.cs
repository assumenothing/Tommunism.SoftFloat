using System;
using System.Runtime.Serialization;

namespace Tommunism.SoftFloat;

[Serializable]
public class SoftFloatException : Exception
{
    public SoftFloatException()
    {
    }

    public SoftFloatException(ExceptionFlags flags) : base(GetMessage(flags))
    {
        Flags = flags;
    }

    public SoftFloatException(string message) : base(message)
    {
    }

    public SoftFloatException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected SoftFloatException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Flags = (ExceptionFlags)(info.GetValue("SoftFloatFlags", typeof(uint)) ?? 0);
    }

    public ExceptionFlags Flags { get; }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue("SoftFloatFlags", (uint)Flags);
    }

    internal static string GetMessage(ExceptionFlags flags) => $"SoftFloat exception flags: {flags}";
}
