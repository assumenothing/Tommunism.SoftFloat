using System;

namespace Tommunism.SoftFloat;

internal struct UInt64Extra : IEquatable<UInt64Extra>
{
    public ulong Extra;
    public ulong V;

    public UInt64Extra(ulong extra, ulong v)
    {
        Extra = extra;
        V = v;
    }

    public void Deconstruct(out ulong extra, out ulong v)
    {
        extra = Extra;
        v = V;
    }

    public override bool Equals(object? obj) => obj is UInt64Extra extra && Equals(extra);

    public bool Equals(UInt64Extra other) => Extra == other.Extra && V == other.V;

    public override int GetHashCode() => HashCode.Combine(Extra, V);

    public override string ToString() => $"0x{V:x16}:{Extra:x16}";

    public static bool operator ==(UInt64Extra left, UInt64Extra right) => left.Equals(right);

    public static bool operator !=(UInt64Extra left, UInt64Extra right) => !(left == right);
}
