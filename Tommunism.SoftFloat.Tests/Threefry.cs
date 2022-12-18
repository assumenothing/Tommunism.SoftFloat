using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tommunism.RandomNumbers;

using static Threefry;

internal static class Threefry
{
    internal const ulong SkeinKeyScheduleParity64 = 0x1BD11BDA_A9FC1A22;
    internal const uint SkeinKeyScheduleParity32 = 0x1BD11BDA;

#if false
    internal static async Task RunQuickBenchmark()
    {
        const ulong TestCount = 10_000_000;

        // Use the same random seed instances (also known as the key) for all 32-bit and 64-bit operations.
        var randomSeed32 = new uint[4];
        var randomSeed64 = new ulong[4];

        Random.Shared.NextBytes(MemoryMarshal.AsBytes(randomSeed32.AsSpan()));
        Random.Shared.NextBytes(MemoryMarshal.AsBytes(randomSeed64.AsSpan()));

        static void BenchmarkRunner(string name, Action<object> action, object state)
        {
            var sw = Stopwatch.StartNew();
            action(state);
            sw.Stop();
            Console.WriteLine($"Executed {name} with {TestCount:#,###} tests in {sw.Elapsed.TotalMilliseconds:f3} ms.");
        }

        static void TestThreefry2x32(object state)
        {
            Span<uint> input = stackalloc uint[2];
            ReadOnlySpan<uint> key = (uint[])state;
            Span<uint> result = stackalloc uint[2];

            input.Clear();

            for (ulong i = 0; i < TestCount; i++)
            {
                input[0] = (uint)i;
                input[1] = (uint)(i >> 32);

                Threefry2x32.Random(input, key, result);
            }
        }

        static void TestThreefry4x32(object state)
        {
            Span<uint> input = stackalloc uint[4];
            ReadOnlySpan<uint> key = (uint[])state;
            Span<uint> result = stackalloc uint[4];

            input.Clear();

            for (ulong i = 0; i < TestCount; i++)
            {
                input[0] = (uint)i;
                input[1] = (uint)(i >> 32);

                Threefry4x32.Random(input, key, result);
            }
        }

        static void TestThreefry2x64(object state)
        {
            Span<ulong> input = stackalloc ulong[2];
            ReadOnlySpan<ulong> key = (ulong[])state;
            Span<ulong> result = stackalloc ulong[2];

            input.Clear();

            for (ulong i = 0; i < TestCount; i++)
            {
                input[0] = i;

                Threefry2x64.Random(input, key, result);
            }
        }

        static void TestThreefry4x64(object state)
        {
            Span<ulong> input = stackalloc ulong[4];
            ReadOnlySpan<ulong> key = (ulong[])state;
            Span<ulong> result = stackalloc ulong[4];

            input.Clear();

            for (ulong i = 0; i < TestCount; i++)
            {
                input[0] = i;

                Threefry2x64.Random(input, key, result);
            }
        }

        // Run the test a few times to get a reasonable good distribution of timings.
        for (var i = 0; i < 10; i++)
        {
            if (i != 0) Console.WriteLine();
            Console.WriteLine($"Test #{i}");
            await Task.WhenAll(
                Task.Run(() => BenchmarkRunner("Threefry2x32", TestThreefry2x32, randomSeed32)),
                Task.Run(() => BenchmarkRunner("Threefry4x32", TestThreefry4x32, randomSeed32)),
                Task.Run(() => BenchmarkRunner("Threefry2x64", TestThreefry2x64, randomSeed64)),
                Task.Run(() => BenchmarkRunner("Threefry4x64", TestThreefry4x64, randomSeed64))
            );
        }
    }
#endif
}

// From my quick benchmarks, it looks like 4x32 and 4x64 are basically tied for first and second place, 2x32 is in third (barely), and 4x32
// is fourth (by a fairly large margin).

#if false

internal static class Threefry2x32
{
    public const int DefaultRounds = 20;

    private const int R_32x2_0_0 = 13;
    private const int R_32x2_1_0 = 15;
    private const int R_32x2_2_0 = 26;
    private const int R_32x2_3_0 = 06;
    private const int R_32x2_4_0 = 17;
    private const int R_32x2_5_0 = 29;
    private const int R_32x2_6_0 = 16;
    private const int R_32x2_7_0 = 24;

    public static void Random(ReadOnlySpan<uint> input, ReadOnlySpan<uint> key, Span<uint> result)
    {
        if (input.Length < 2)
            throw new ArgumentException("Input length requires at least 2 elements.", nameof(input));
        if (key.Length < 2)
            throw new ArgumentException("Key length requires at least 2 elements.", nameof(key));
        if (result.Length < 2)
            throw new ArgumentException("Result length requires at least 2 elements.", nameof(result));

        Rounds(input, key, result, DefaultRounds);
    }

    public static void Random(ReadOnlySpan<uint> input, ReadOnlySpan<uint> key, Span<uint> result, int rounds)
    {
        if (input.Length < 2)
            throw new ArgumentException("Input length requires at least 2 elements.", nameof(input));
        if (key.Length < 2)
            throw new ArgumentException("Key length requires at least 2 elements.", nameof(key));
        if (result.Length < 2)
            throw new ArgumentException("Result length requires at least 2 elements.", nameof(result));
        if (rounds is < 0 or > 32)
            throw new ArgumentOutOfRangeException(nameof(rounds));

        Rounds(input, key, result, (uint)rounds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rounds(ReadOnlySpan<uint> input, ReadOnlySpan<uint> key, Span<uint> x, uint rounds)
    {
        Debug.Assert(rounds <= 32);
        Debug.Assert(input.Length >= 2);
        Debug.Assert(key.Length >= 2);
        Debug.Assert(x.Length >= 2);

        uint x0 = input[0], x1 = input[1];
        uint ks0 = key[0], ks1 = key[1], ks2 = SkeinKeyScheduleParity32 ^ ks0 ^ ks1;

        // Insert initial key before round 0.
        x0 += ks0; x1 += ks1;

        if (rounds > 00) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_0_0) ^ x0; }
        if (rounds > 01) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_1_0) ^ x0; }
        if (rounds > 02) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_2_0) ^ x0; }
        if (rounds > 03) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_3_0) ^ x0; }
        if (rounds > 03)
        {
            // InjectKey(r=1)
            x0 += ks1; x1 += ks2 + 1;
        }

        if (rounds > 04) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_4_0) ^ x0; }
        if (rounds > 05) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_5_0) ^ x0; }
        if (rounds > 06) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_6_0) ^ x0; }
        if (rounds > 07) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_7_0) ^ x0; }
        if (rounds > 07)
        {
            // InjectKey(r=2)
            x0 += ks2; x1 += ks0 + 2;
        }

        if (rounds > 08) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_0_0) ^ x0; }
        if (rounds > 09) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_1_0) ^ x0; }
        if (rounds > 10) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_2_0) ^ x0; }
        if (rounds > 11) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_3_0) ^ x0; }
        if (rounds > 11)
        {
            // InjectKey(r=3)
            x0 += ks0; x1 += ks1 + 3;
        }

        if (rounds > 12) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_4_0) ^ x0; }
        if (rounds > 13) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_5_0) ^ x0; }
        if (rounds > 14) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_6_0) ^ x0; }
        if (rounds > 15) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_7_0) ^ x0; }
        if (rounds > 15)
        {
            // InjectKey(r=4)
            x0 += ks1; x1 += ks2 + 4;
        }

        if (rounds > 16) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_0_0) ^ x0; }
        if (rounds > 17) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_1_0) ^ x0; }
        if (rounds > 18) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_2_0) ^ x0; }
        if (rounds > 19) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_3_0) ^ x0; }
        if (rounds > 19)
        {
            // InjectKey(r=5)
            x0 += ks2; x1 += ks0 + 5;
        }

        if (rounds > 20) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_4_0) ^ x0; }
        if (rounds > 21) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_5_0) ^ x0; }
        if (rounds > 22) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_6_0) ^ x0; }
        if (rounds > 23) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_7_0) ^ x0; }
        if (rounds > 23)
        {
            // InjectKey(r=6)
            x0 += ks0; x1 += ks1 + 6;
        }

        if (rounds > 24) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_0_0) ^ x0; }
        if (rounds > 25) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_1_0) ^ x0; }
        if (rounds > 26) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_2_0) ^ x0; }
        if (rounds > 27) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_3_0) ^ x0; }
        if (rounds > 27)
        {
            // InjectKey(r=7)
            x0 += ks1; x1 += ks2 + 7;
        }

        if (rounds > 28) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_4_0) ^ x0; }
        if (rounds > 29) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_5_0) ^ x0; }
        if (rounds > 30) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_6_0) ^ x0; }
        if (rounds > 31) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x2_7_0) ^ x0; }
        if (rounds > 31)
        {
            // InjectKey(r=8)
            x0 += ks2; x1 += ks0 + 8;
        }

        x[0] = x0; x[1] = x1;
    }
}

internal static class Threefry2x64
{
    public const int DefaultRounds = 20;

    // Rotation constants
    private const int R_64x2_0_0 = 16;
    private const int R_64x2_1_0 = 42;
    private const int R_64x2_2_0 = 12;
    private const int R_64x2_3_0 = 31;
    private const int R_64x2_4_0 = 16;
    private const int R_64x2_5_0 = 32;
    private const int R_64x2_6_0 = 24;
    private const int R_64x2_7_0 = 21;

    public static void Random(ReadOnlySpan<ulong> input, ReadOnlySpan<ulong> key, Span<ulong> result)
    {
        if (input.Length < 2)
            throw new ArgumentException("Input length requires at least 2 elements.", nameof(input));
        if (key.Length < 2)
            throw new ArgumentException("Key length requires at least 2 elements.", nameof(key));
        if (result.Length < 2)
            throw new ArgumentException("Result length requires at least 2 elements.", nameof(result));

        Rounds(input, key, result, DefaultRounds);
    }

    public static void Random(ReadOnlySpan<ulong> input, ReadOnlySpan<ulong> key, Span<ulong> result, int rounds)
    {
        if (input.Length < 2)
            throw new ArgumentException("Input length requires at least 2 elements.", nameof(input));
        if (key.Length < 2)
            throw new ArgumentException("Key length requires at least 2 elements.", nameof(key));
        if (result.Length < 2)
            throw new ArgumentException("Result length requires at least 2 elements.", nameof(result));
        if (rounds is < 0 or > 32)
            throw new ArgumentOutOfRangeException(nameof(rounds));

        Rounds(input, key, result, (uint)rounds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rounds(ReadOnlySpan<ulong> input, ReadOnlySpan<ulong> key, Span<ulong> x, uint rounds)
    {
        Debug.Assert(rounds <= 32);
        Debug.Assert(input.Length >= 2);
        Debug.Assert(key.Length >= 2);
        Debug.Assert(x.Length >= 2);

        ulong x0 = input[0], x1 = input[1];
        ulong ks0 = key[0], ks1 = key[1], ks2 = SkeinKeyScheduleParity64 ^ ks0 ^ ks1;

        // Insert initial key before round 0.
        x0 += ks0; x1 += ks1;

        if (rounds > 00) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_0_0) ^ x0; }
        if (rounds > 01) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_1_0) ^ x0; }
        if (rounds > 02) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_2_0) ^ x0; }
        if (rounds > 03) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_3_0) ^ x0; }
        if (rounds > 03)
        {
            // InjectKey(r=1)
            x0 += ks1; x1 += ks2 + 1;
        }

        if (rounds > 04) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_4_0) ^ x0; }
        if (rounds > 05) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_5_0) ^ x0; }
        if (rounds > 06) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_6_0) ^ x0; }
        if (rounds > 07) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_7_0) ^ x0; }
        if (rounds > 07)
        {
            // InjectKey(r=2)
            x0 += ks2; x1 += ks0 + 2;
        }

        if (rounds > 08) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_0_0) ^ x0; }
        if (rounds > 09) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_1_0) ^ x0; }
        if (rounds > 10) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_2_0) ^ x0; }
        if (rounds > 11) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_3_0) ^ x0; }
        if (rounds > 11)
        {
            // InjectKey(r=3)
            x0 += ks0; x1 += ks1 + 3;
        }

        if (rounds > 12) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_4_0) ^ x0; }
        if (rounds > 13) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_5_0) ^ x0; }
        if (rounds > 14) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_6_0) ^ x0; }
        if (rounds > 15) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_7_0) ^ x0; }
        if (rounds > 15)
        {
            // InjectKey(r=4)
            x0 += ks1; x1 += ks2 + 4;
        }

        if (rounds > 16) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_0_0) ^ x0; }
        if (rounds > 17) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_1_0) ^ x0; }
        if (rounds > 18) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_2_0) ^ x0; }
        if (rounds > 19) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_3_0) ^ x0; }
        if (rounds > 19)
        {
            // InjectKey(r=5)
            x0 += ks2; x1 += ks0 + 5;
        }

        if (rounds > 20) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_4_0) ^ x0; }
        if (rounds > 21) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_5_0) ^ x0; }
        if (rounds > 22) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_6_0) ^ x0; }
        if (rounds > 23) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_7_0) ^ x0; }
        if (rounds > 23)
        {
            // InjectKey(r=6)
            x0 += ks0; x1 += ks1 + 6;
        }

        if (rounds > 24) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_0_0) ^ x0; }
        if (rounds > 25) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_1_0) ^ x0; }
        if (rounds > 26) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_2_0) ^ x0; }
        if (rounds > 27) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_3_0) ^ x0; }
        if (rounds > 27)
        {
            // InjectKey(r=7)
            x0 += ks1; x1 += ks2 + 7;
        }

        if (rounds > 28) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_4_0) ^ x0; }
        if (rounds > 29) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_5_0) ^ x0; }
        if (rounds > 30) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_6_0) ^ x0; }
        if (rounds > 31) { x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x2_7_0) ^ x0; }
        if (rounds > 31)
        {
            // InjectKey(r=8)
            x0 += ks2; x1 += ks0 + 8;
        }

        x[0] = x0; x[1] = x1;
    }
}

internal static class Threefry4x32
{
    public const int DefaultRounds = 20;

    private const int R_32x4_0_0 = 10, R_32x4_0_1 = 26;
    private const int R_32x4_1_0 = 11, R_32x4_1_1 = 21;
    private const int R_32x4_2_0 = 13, R_32x4_2_1 = 27;
    private const int R_32x4_3_0 = 23, R_32x4_3_1 = 05;
    private const int R_32x4_4_0 = 06, R_32x4_4_1 = 20;
    private const int R_32x4_5_0 = 17, R_32x4_5_1 = 11;
    private const int R_32x4_6_0 = 25, R_32x4_6_1 = 10;
    private const int R_32x4_7_0 = 18, R_32x4_7_1 = 20;

    public static void Random(ReadOnlySpan<uint> input, ReadOnlySpan<uint> key, Span<uint> result)
    {
        if (input.Length < 4)
            throw new ArgumentException("Input length requires at least 4 elements.", nameof(input));
        if (key.Length < 4)
            throw new ArgumentException("Key length requires at least 4 elements.", nameof(key));
        if (result.Length < 4)
            throw new ArgumentException("Result length requires at least 4 elements.", nameof(result));

        Rounds(input, key, result, DefaultRounds);
    }

    public static void Random(ReadOnlySpan<uint> input, ReadOnlySpan<uint> key, Span<uint> result, int rounds)
    {
        if (input.Length < 4)
            throw new ArgumentException("Input length requires at least 4 elements.", nameof(input));
        if (key.Length < 4)
            throw new ArgumentException("Key length requires at least 4 elements.", nameof(key));
        if (result.Length < 4)
            throw new ArgumentException("Result length requires at least 4 elements.", nameof(result));
        if (rounds is < 0 or > 72)
            throw new ArgumentOutOfRangeException(nameof(rounds));

        Rounds(input, key, result, (uint)rounds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Rounds(ReadOnlySpan<uint> input, ReadOnlySpan<uint> key, Span<uint> x, uint rounds)
    {
        Debug.Assert(input.Length >= 4);
        Debug.Assert(key.Length >= 4);
        Debug.Assert(x.Length >= 4);
        Debug.Assert(rounds <= 72);

        uint x0 = input[0], x1 = input[1], x2 = input[2], x3 = input[3];
        uint ks0 = key[0], ks1 = key[1], ks2 = key[2], ks3 = key[3], ks4 = SkeinKeyScheduleParity32 ^ ks0 ^ ks1 ^ ks2 ^ ks3;

        // Insert initial key before round 0.
        x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3;

        if (rounds > 00)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 01)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 02)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 03)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=1)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 1;
        }

        if (rounds > 04)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 05)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 06)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 07)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=2)
            x0 += ks2; x1 += ks3; x2 += ks4; x3 += ks0 + 2;
        }

        if (rounds > 08)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 09)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 10)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 11)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=3)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 3;
        }

        if (rounds > 12)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 13)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 14)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 15)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=4)
            x0 += ks4; x1 += ks0; x2 += ks1; x3 += ks2 + 4;
        }

        if (rounds > 16)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 17)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 18)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 19)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=5)
            x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3 + 5;
        }

        if (rounds > 20)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 21)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 22)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 23)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=6)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 6;
        }

        if (rounds > 24)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 25)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 26)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 27)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=7)
            x0 += ks3; x1 += ks4; x2 += ks4; x3 += ks0 + 7;
        }

        if (rounds > 28)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 29)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 30)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 31)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=8)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 8;
        }

        if (rounds > 32)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 33)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 34)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 35)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=9)
            x0 += ks4; x1 += ks0; x2 += ks1; x3 += ks2 + 9;
        }

        if (rounds > 36)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 37)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 38)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 39)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=10)
            x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3 + 10;
        }

        if (rounds > 40)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 41)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 42)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 43)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=11)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 11;
        }

        if (rounds > 44)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 45)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 46)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 47)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=12)
            x0 += ks2; x1 += ks3; x2 += ks4; x3 += ks0 + 12;
        }

        if (rounds > 48)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 49)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 50)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 51)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=13)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 13;
        }

        if (rounds > 52)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 53)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 54)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 55)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=14)
            x0 += ks4; x1 += ks0; x2 += ks1; x3 += ks2 + 14;
        }

        if (rounds > 56)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 57)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 58)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 59)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=15)
            x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3 + 15;
        }

        if (rounds > 60)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 61)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 62)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 63)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=16)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 16;
        }

        if (rounds > 64)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_0_1) ^ x2;
        }
        if (rounds > 65)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_1_1) ^ x2;
        }
        if (rounds > 66)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_2_1) ^ x2;
        }
        if (rounds > 67)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_3_1) ^ x2;

            // InjectKey(r=17)
            x0 += ks2; x1 += ks3; x2 += ks4; x3 += ks0 + 17;
        }

        if (rounds > 68)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_4_1) ^ x2;
        }
        if (rounds > 69)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_5_1) ^ x2;
        }
        if (rounds > 70)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_6_1) ^ x2;
        }
        if (rounds > 71)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_32x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_32x4_7_1) ^ x2;

            // InjectKey(r=18)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 18;
        }

        x[0] = x0; x[1] = x1; x[2] = x2; x[3] = x3;
    }
}

#endif

internal static class Threefry4x64
{
    public const int DefaultRounds = 20;

    private const int R_64x4_0_0 = 14, R_64x4_0_1 = 16;
    private const int R_64x4_1_0 = 52, R_64x4_1_1 = 57;
    private const int R_64x4_2_0 = 23, R_64x4_2_1 = 40;
    private const int R_64x4_3_0 = 05, R_64x4_3_1 = 37;
    private const int R_64x4_4_0 = 25, R_64x4_4_1 = 33;
    private const int R_64x4_5_0 = 46, R_64x4_5_1 = 12;
    private const int R_64x4_6_0 = 58, R_64x4_6_1 = 22;
    private const int R_64x4_7_0 = 32, R_64x4_7_1 = 32;

    public static void Random(ReadOnlySpan<ulong> input, ReadOnlySpan<ulong> key, Span<ulong> result)
    {
        if (input.Length < 4)
            throw new ArgumentException("Input length requires at least 4 elements.", nameof(input));
        if (key.Length < 4)
            throw new ArgumentException("Key length requires at least 4 elements.", nameof(key));
        if (result.Length < 4)
            throw new ArgumentException("Result length requires at least 4 elements.", nameof(result));

        Rounds(input, key, result, DefaultRounds);
    }

    public static void Random(ReadOnlySpan<ulong> input, ReadOnlySpan<ulong> key, Span<ulong> result, int rounds)
    {
        if (input.Length < 4)
            throw new ArgumentException("Input length requires at least 4 elements.", nameof(input));
        if (key.Length < 4)
            throw new ArgumentException("Key length requires at least 4 elements.", nameof(key));
        if (result.Length < 4)
            throw new ArgumentException("Result length requires at least 4 elements.", nameof(result));
        if (rounds is < 0 or > 72)
            throw new ArgumentOutOfRangeException(nameof(rounds));

        Rounds(input, key, result, (uint)rounds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Rounds(ReadOnlySpan<ulong> input, ReadOnlySpan<ulong> key, Span<ulong> x, uint rounds)
    {
        Debug.Assert(input.Length >= 4);
        Debug.Assert(key.Length >= 4);
        Debug.Assert(x.Length >= 4);
        Debug.Assert(rounds <= 72);

        ulong x0 = input[0], x1 = input[1], x2 = input[2], x3 = input[3];
        ulong ks0 = key[0], ks1 = key[1], ks2 = key[2], ks3 = key[3], ks4 = SkeinKeyScheduleParity64 ^ ks0 ^ ks1 ^ ks2 ^ ks3;

        // Insert initial key before round 0.
        x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3;

        if (rounds > 00)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 01)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 02)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 03)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=1)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 1;
        }

        if (rounds > 04)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 05)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 06)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 07)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=2)
            x0 += ks2; x1 += ks3; x2 += ks4; x3 += ks0 + 2;
        }

        if (rounds > 08)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 09)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 10)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 11)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=3)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 3;
        }

        if (rounds > 12)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 13)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 14)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 15)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=4)
            x0 += ks4; x1 += ks0; x2 += ks1; x3 += ks2 + 4;
        }

        if (rounds > 16)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 17)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 18)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 19)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=5)
            x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3 + 5;
        }

        if (rounds > 20)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 21)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 22)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 23)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=6)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 6;
        }

        if (rounds > 24)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 25)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 26)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 27)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=7)
            x0 += ks3; x1 += ks4; x2 += ks4; x3 += ks0 + 7;
        }

        if (rounds > 28)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 29)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 30)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 31)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=8)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 8;
        }

        if (rounds > 32)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 33)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 34)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 35)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=9)
            x0 += ks4; x1 += ks0; x2 += ks1; x3 += ks2 + 9;
        }

        if (rounds > 36)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 37)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 38)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 39)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=10)
            x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3 + 10;
        }

        if (rounds > 40)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 41)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 42)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 43)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=11)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 11;
        }

        if (rounds > 44)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 45)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 46)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 47)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=12)
            x0 += ks2; x1 += ks3; x2 += ks4; x3 += ks0 + 12;
        }

        if (rounds > 48)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 49)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 50)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 51)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=13)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 13;
        }

        if (rounds > 52)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 53)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 54)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 55)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=14)
            x0 += ks4; x1 += ks0; x2 += ks1; x3 += ks2 + 14;
        }

        if (rounds > 56)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 57)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 58)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 59)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=15)
            x0 += ks0; x1 += ks1; x2 += ks2; x3 += ks3 + 15;
        }

        if (rounds > 60)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 61)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 62)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 63)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=16)
            x0 += ks1; x1 += ks2; x2 += ks3; x3 += ks4 + 16;
        }

        if (rounds > 64)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_0_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_0_1) ^ x2;
        }
        if (rounds > 65)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_1_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_1_1) ^ x2;
        }
        if (rounds > 66)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_2_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_2_1) ^ x2;
        }
        if (rounds > 67)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_3_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_3_1) ^ x2;

            // InjectKey(r=17)
            x0 += ks2; x1 += ks3; x2 += ks4; x3 += ks0 + 17;
        }

        if (rounds > 68)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_4_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_4_1) ^ x2;
        }
        if (rounds > 69)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_5_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_5_1) ^ x2;
        }
        if (rounds > 70)
        {
            x0 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_6_0) ^ x0;
            x2 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_6_1) ^ x2;
        }
        if (rounds > 71)
        {
            x0 += x3; x3 = BitOperations.RotateLeft(x3, R_64x4_7_0) ^ x0;
            x2 += x1; x1 = BitOperations.RotateLeft(x1, R_64x4_7_1) ^ x2;

            // InjectKey(r=18)
            x0 += ks3; x1 += ks4; x2 += ks0; x3 += ks1 + 18;
        }

        x[0] = x0; x[1] = x1; x[2] = x2; x[3] = x3;
    }
}

/// <summary>
/// Provides a simple "stateful" wrapper around <see cref="Threefry4x64"/> along with various helpers (similar to <see cref="Random"/>).
/// </summary>
internal ref struct ThreefryRandom
{
    #region Fields

    public const int ResultsSize = 4;

    private readonly ReadOnlySpan<ulong> _seed;
    private readonly Span<ulong> _results;
    private readonly ulong _counterBase; // generally the index for parallel tasks
    private ulong _counter;

    #endregion

    #region Constructors

    public ThreefryRandom(long index, ReadOnlySpan<ulong> seed, Span<ulong> resultsBuffer) : this((ulong)index, seed, resultsBuffer) { }

    // NOTE: The index is an arbitrary value that can be used for repeatable random values (this is generally an enumeration/task index).
    // NOTE: The resultsBuffer is generally passed into the constructor via stackalloc memory (avoids using weird struct hacks or unsafe code).
    public ThreefryRandom(ulong index, ReadOnlySpan<ulong> seed, Span<ulong> resultsBuffer)
    {
        Debug.Assert(seed.Length == 4, "Seed length is out of range.");
        Debug.Assert(resultsBuffer.Length == ResultsSize, "Results buffer length is out of range.");

        _seed = seed;
        _results = resultsBuffer;
        _counterBase = index;
        _counter = 0;
    }

    #endregion

    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextUInt64Core()
    {
        Debug.Assert(_results.Length == ResultsSize, "Results buffer is invalid. Did you accidentally create a default instance of this type or use the parameterless constructor?");

        // Consume all results first before moving on to the next random results.
        int resultIndex = (int)((uint)_counter & 3);
        if (resultIndex == 0)
        {
            // Fill the random results up with more random data.
            Span<ulong> inputSpan = stackalloc ulong[4];
            inputSpan[0] = _counterBase;
            inputSpan[1] = _counter; // lower 2 bits are for the result index and should always be zero here
            inputSpan[2] = 0;
            inputSpan[3] = 0;

            Threefry4x64.Random(inputSpan, _seed, _results);
        }

        //ulong result = _results[resultIndex]; // possibly some slowdown due to bounds checking
        ulong result = Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_results), (nuint)resultIndex << 3);
        _counter++;
        return result;
    }

    /// <summary>
    /// Gets the next unsigned 32-bit integer from the random pool.
    /// </summary>
    /// <returns>An unsigned 32-bit integer.</returns>
    public uint NextUInt32() => (uint)(NextUInt64Core() >> 32);

    /// <summary>
    /// Gets the next unsigned 64-bit integer from the random pool.
    /// </summary>
    /// <returns>An unsigned 64-bit integer.</returns>
    public ulong NextUInt64() => NextUInt64Core();

#if NET7_0_OR_GREATER

    /// <summary>
    /// Gets the next unsigned 128-bit integer from the random pool.
    /// </summary>
    /// <returns>An unsigned 128-bit integer.</returns>
    public UInt128 NextUInt128()
    {
        ulong valueLo = NextUInt64Core();
        ulong valueHi = NextUInt64Core();
        return new(valueHi, valueLo);
    }

#endif

    /// <summary>
    /// Gets the next non-negative signed 32-bit integer from the random pool.
    /// </summary>
    /// <returns>A signed non-negative 32-bit integer</returns>
    /// <remarks>
    /// Note that the returned value may be equal to <see cref="int.MaxValue"/> (this differs from <see cref="Random.Next()"/>). If you want that behavior, then call <c>NextInt32(int.MaxValue)</c>.
    /// </remarks>
    public int NextInt32() => (int)(NextUInt64Core() >> 33);

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to 0.</param>
    public int NextInt32(int maxValue)
    {
        if (maxValue > 1)
        {
            // Narrow down to the smallest range [0, 2^bits] that contains maxValue.
            // Then repeatedly generate a value in that outer range until we get one within the inner range.
            int bits = Log2Ceiling((uint)maxValue);
            while (true)
            {
                ulong result = NextUInt64Core() >> (sizeof(ulong) * 8 - bits);
                if (result < (uint)maxValue)
                    return (int)result;
            }
        }

        Debug.Assert(maxValue is >= 0 and <= 1);
        return 0;
    }

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    public int NextInt32(int minValue, int maxValue) => NextInt32(maxValue - minValue) + minValue;

    /// <summary>
    /// Gets the next non-negative signed 32-bit integer from the random pool.
    /// </summary>
    /// <returns>A signed non-negative 32-bit integer</returns>
    /// <remarks>
    /// Note that the returned value may be equal to <see cref="long.MaxValue"/> (this differs from <see cref="Random.NextInt64()"/>). If you want that behavior, then call <c>NextInt64(long.MaxValue)</c>.
    /// </remarks>
    public long NextInt64() => (int)(NextUInt64Core() >> 1);

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to 0.</param>
    public long NextInt64(long maxValue)
    {
        if (maxValue > 1)
        {
            // Narrow down to the smallest range [0, 2^bits] that contains maxValue.
            // Then repeatedly generate a value in that outer range until we get one within the inner range.
            int bits = Log2Ceiling((uint)maxValue);
            while (true)
            {
                ulong result = NextUInt64Core() >> (sizeof(ulong) * 8 - bits);
                if (result < (uint)maxValue)
                    return (int)result;
            }
        }

        Debug.Assert(maxValue is >= 0 and <= 1);
        return 0;
    }

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    public long NextInt64(long minValue, long maxValue) => NextInt64(maxValue - minValue) + minValue;

#if NET7_0_OR_GREATER

    /// <summary>
    /// Gets the next non-negative signed 128-bit integer from the random pool.
    /// </summary>
    /// <returns>A signed non-negative 128-bit integer</returns>
    /// <remarks>
    /// Note that the returned value may be equal to <see cref="Int128.MaxValue"/>.
    /// </remarks>
    public Int128 NextInt128()
    {
        ulong valueLo = NextUInt64Core();
        ulong valueHi = NextUInt64Core();
        return new(valueHi >> 1, valueLo);
    }

#endif

    /// <summary>
    /// Fills the elements of a specified span of bytes with random numbers.
    /// </summary>
    /// <param name="buffer">The span to be filled with random numbers.</param>
    public void NextBytes(Span<byte> buffer)
    {
        while (buffer.Length >= sizeof(ulong))
        {
            // Keep the random values consistent between CPU architectures by only using little endian writes.
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, NextUInt64Core());
            buffer = buffer[sizeof(ulong)..];
        }

        if (!buffer.IsEmpty)
        {
            Debug.Assert(buffer.Length < sizeof(ulong));
            var next = NextUInt64Core();
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(next >> (i * 8));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Log2Ceiling(uint value)
    {
        int result = BitOperations.Log2(value);
        if (BitOperations.PopCount(value) != 1)
            result++;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Log2Ceiling(ulong value)
    {
        int result = BitOperations.Log2(value);
        if (BitOperations.PopCount(value) != 1)
            result++;

        return result;
    }

    #endregion
}
