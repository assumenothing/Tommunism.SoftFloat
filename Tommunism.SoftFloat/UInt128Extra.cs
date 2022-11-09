using System;

namespace Tommunism.SoftFloat;

internal struct UInt128Extra : IEquatable<UInt128Extra>
{
    public ulong Extra;
    public UInt128 V;

    public UInt128Extra(ulong extra, UInt128 v)
    {
        Extra = extra;
        V = v;
    }

    public UInt128Extra(ulong extra, ulong v0, ulong v64)
    {
        Extra = extra;
        V = new UInt128(v0: v0, v64: v64);
    }

    public void Deconstruct(out ulong extra, out UInt128 v)
    {
        extra = Extra;
        v = V;
    }

    public void Deconstruct(out ulong extra, out ulong v64, out ulong v0)
    {
        extra = Extra;
        V.Deconstruct(v64: out v64, v0: out v0);
    }

    public override bool Equals(object? obj) => obj is UInt128Extra extra && Equals(extra);

    public bool Equals(UInt128Extra other) => Extra == other.Extra && V.Equals(other.V);

    public override int GetHashCode() => HashCode.Combine(Extra, V.V00, V.V64);

    public override string ToString() => $"0x{V.V64:x16}{V.V00:x16}:{Extra:x16}";

    public static bool operator ==(UInt128Extra left, UInt128Extra right) => left.Equals(right);

    public static bool operator !=(UInt128Extra left, UInt128Extra right) => !(left == right);
}
