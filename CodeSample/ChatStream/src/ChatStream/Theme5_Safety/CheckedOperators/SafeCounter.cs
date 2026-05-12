namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Checked User-Defined Operators  (C# 11)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Custom numeric types (counters, scores, capacities) need
//  arithmetic operators.  But without overflow checking,
//  adding 1 to MaxValue silently wraps to MinValue.  Before
//  C# 11, you couldn't define separate checked/unchecked
//  versions of your operators.
//
//  SOLUTION
//  --------
//  C# 11 allows `static checked operator+` alongside
//  `static operator+`.  The compiler dispatches to the
//  checked variant inside `checked { }` blocks, and to the
//  unchecked variant otherwise.
//
//  WHY IT MATTERS
//  ──────────────
//  Your custom types now participate in the checked/unchecked
//  system just like built-in integers.  Overflow is caught
//  exactly where it matters, not everywhere or nowhere.
//
//  TRY IT
//  ──────
//  1. Add checked operator* to MessageCounter.
//  2. Create a BoundedCapacity type with min/max constraints.
//  3. Test with `checked { }` vs `unchecked { }` blocks.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates checked user-defined operators for safe counting.
/// </summary>
public static class SafeCounterDemo
{
    /// <summary>
    /// A message counter that supports checked and unchecked arithmetic.
    /// The checked variant throws on overflow; the unchecked variant wraps.
    /// </summary>
    private readonly record struct MessageCounter(int Value) :
        IComparable<MessageCounter>
    {
        public static readonly MessageCounter Zero = new(0);
        public static readonly MessageCounter Max = new(int.MaxValue);

        // ── Unchecked operator+ (default) ────────────────────
        // Used in normal (unchecked) contexts. Wraps on overflow.
        public static MessageCounter operator +(MessageCounter a, MessageCounter b) =>
            new(unchecked(a.Value + b.Value));

        // ── Checked operator+ ────────────────────────────────
        // Used inside `checked { }` blocks. Throws OverflowException.
        public static MessageCounter operator checked +(MessageCounter a, MessageCounter b) =>
            new(checked(a.Value + b.Value));

        // ── Unchecked operator++ (increment) ─────────────────
        public static MessageCounter operator ++(MessageCounter c) =>
            new(unchecked(c.Value + 1));

        // ── Checked operator++ ───────────────────────────────
        public static MessageCounter operator checked ++(MessageCounter c) =>
            new(checked(c.Value + 1));

        // ── Unchecked explicit cast from int ─────────────────
        public static explicit operator MessageCounter(int value) =>
            new(value);

        // ── Checked explicit cast from int ───────────────────
        // Ensures the value is non-negative in checked contexts.
        public static explicit operator checked MessageCounter(int value) =>
            value >= 0
                ? new(value)
                : throw new OverflowException(
                    $"Message count cannot be negative: {value}");

        public int CompareTo(MessageCounter other) =>
            Value.CompareTo(other.Value);

        public override string ToString() => $"Count({Value:N0})";
    }

    /// <summary>
    /// A bounded capacity type that enforces min/max in checked contexts.
    /// </summary>
    private readonly record struct RoomCapacity(int Value)
    {
        public const int MinCapacity = 1;
        public const int MaxCapacity = 10_000;

        // Unchecked: clamps to range
        public static RoomCapacity operator +(RoomCapacity a, int delta) =>
            new(Math.Clamp(a.Value + delta, MinCapacity, MaxCapacity));

        // Checked: throws if out of range
        public static RoomCapacity operator checked +(RoomCapacity a, int delta)
        {
            int result = checked(a.Value + delta);
            return result is >= MinCapacity and <= MaxCapacity
                ? new(result)
                : throw new OverflowException(
                    $"Capacity {result} is outside [{MinCapacity}..{MaxCapacity}]");
        }

        public override string ToString() => $"Capacity({Value})";
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Checked User-Defined Operators ═══╗\n");

        // ── 1. Unchecked arithmetic (wraps silently) ─────────
        Console.WriteLine("── Unchecked Arithmetic ──");
        var counter = new MessageCounter(int.MaxValue - 2);
        Console.WriteLine($"  Start:     {counter}");

        counter++;
        Console.WriteLine($"  After ++:  {counter}");

        counter++;  // This wraps in unchecked context
        Console.WriteLine($"  After ++:  {counter} (near max)");

        counter++;  // Wraps to negative!
        Console.WriteLine($"  After ++:  {counter} (wrapped — silent overflow!)");

        // ── 2. Checked arithmetic (throws on overflow) ───────
        Console.WriteLine("\n── Checked Arithmetic ──");
        var safeCounter = new MessageCounter(int.MaxValue - 1);
        Console.WriteLine($"  Start:     {safeCounter}");

        try
        {
            checked
            {
                safeCounter++;
                Console.WriteLine($"  After ++:  {safeCounter}");
                safeCounter++;  // This will throw!
                Console.WriteLine("  This line should not appear.");
            }
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"  Overflow caught: {ex.Message}");
        }

        // ── 3. Checked addition ──────────────────────────────
        Console.WriteLine("\n── Checked Addition ──");
        var a = new MessageCounter(100);
        var b = new MessageCounter(200);
        var sum = checked(a + b);
        Console.WriteLine($"  {a} + {b} = {sum}");

        try
        {
            var big = MessageCounter.Max;
            _ = checked(big + new MessageCounter(1));
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"  Max + 1 overflow: {ex.Message}");
        }

        // ── 4. Checked cast ──────────────────────────────────
        Console.WriteLine("\n── Checked Cast ──");
        var fromInt = checked((MessageCounter)42);
        Console.WriteLine($"  From 42:   {fromInt}");

        try
        {
            _ = checked((MessageCounter)(-5));
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"  From -5:   {ex.Message}");
        }

        // Unchecked cast allows negative (wraps)
        var uncheckedCast = (MessageCounter)(-5);
        Console.WriteLine($"  Unchecked -5: {uncheckedCast}");

        // ── 5. Bounded capacity ──────────────────────────────
        Console.WriteLine("\n── Bounded Capacity ──");
        var capacity = new RoomCapacity(100);
        Console.WriteLine($"  Start:     {capacity}");

        // Unchecked: clamps to range
        var clamped = capacity + 50_000;
        Console.WriteLine($"  + 50,000 (unchecked): {clamped} (clamped to max)");

        // Checked: throws
        try
        {
            _ = checked(capacity + 50_000);
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"  + 50,000 (checked):   {ex.Message}");
        }

        // ── 6. Summary ──────────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  static operator+          → used in unchecked context");
        Console.WriteLine("  static checked operator+  → used in checked { } block");
        Console.WriteLine("  Your types now work like int/long with checked/unchecked!");
    }
}
