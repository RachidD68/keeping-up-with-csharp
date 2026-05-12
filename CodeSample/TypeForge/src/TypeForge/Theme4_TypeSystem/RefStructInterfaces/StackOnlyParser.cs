namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: ref struct Interfaces  (C# 13)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  ref structs (like Span<T>, ReadOnlySpan<T>) cannot
//  implement interfaces because boxing a ref struct is
//  impossible — it must stay on the stack.  This meant
//  ref structs couldn't participate in generic abstractions.
//
//  SOLUTION
//  --------
//  C# 13 allows ref structs to implement interfaces, with
//  the constraint that the interface usage cannot cause
//  boxing.  You can use them with generic type parameters
//  that have the `allows ref struct` anti-constraint.
//
//  WHY IT MATTERS
//  ──────────────
//  Span<T> now implements IEnumerable<T>!  Your ref struct
//  parsers, tokenizers, and builders can implement common
//  interfaces without heap allocation.
//
//  TRY IT
//  ──────
//  1. Create a ref struct that implements IDisposable.
//  2. Use `allows ref struct` to accept Span-based types.
//  3. Build a ref struct tokenizer that implements an interface.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates ref struct interfaces for stack-only types.
/// </summary>
public static class StackOnlyParserDemo
{
    // ── Interface that ref structs can implement ──────────────

    /// <summary>
    /// A simple parser interface — ref structs can implement this.
    /// </summary>
    private interface IParser<T>
    {
        bool TryParse(ReadOnlySpan<char> input, out T result);
    }

    // ── ref struct implementing an interface ─────────────────

    /// <summary>
    /// A stack-only type name parser — never allocates on the heap.
    /// In C# 13, ref structs can implement interfaces.
    /// </summary>
    private ref struct TypeNameParser : IParser<string>, IDisposable
    {
        private ReadOnlySpan<char> _remaining;
        private bool _disposed;

        public TypeNameParser(ReadOnlySpan<char> input)
        {
            _remaining = input;
            _disposed = false;
        }

        /// <summary>
        /// Tries to parse the next type name token.
        /// </summary>
        public bool TryParse(ReadOnlySpan<char> input, out string result)
        {
            // Trim leading whitespace
            var trimmed = input.Trim();
            if (trimmed.IsEmpty)
            {
                result = string.Empty;
                return false;
            }

            // Find end of identifier (letters, digits, underscore)
            int end = 0;
            while (end < trimmed.Length &&
                   (char.IsLetterOrDigit(trimmed[end]) || trimmed[end] == '_'))
            {
                end++;
            }

            if (end == 0)
            {
                result = string.Empty;
                return false;
            }

            result = trimmed[..end].ToString();
            return true;
        }

        /// <summary>
        /// Consumes and returns the next token from the remaining input.
        /// </summary>
        public string? NextToken()
        {
            _remaining = _remaining.TrimStart();
            if (_remaining.IsEmpty) return null;

            int end = 0;
            while (end < _remaining.Length &&
                   (char.IsLetterOrDigit(_remaining[end]) ||
                    _remaining[end] == '_' ||
                    _remaining[end] == '<' ||
                    _remaining[end] == '>'))
            {
                end++;
            }

            if (end == 0)
            {
                // Skip non-identifier characters
                _remaining = _remaining[1..];
                return NextToken();
            }

            var token = _remaining[..end].ToString();
            _remaining = _remaining[end..];
            return token;
        }

        public bool HasMore => !_remaining.IsEmpty;

        /// <summary>IDisposable — cleanup works on ref structs now.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _remaining = default;
            }
        }
    }

    /// <summary>
    /// A ref struct span analyzer — processes Span&lt;T&gt; data on the stack.
    /// </summary>
    private ref struct SpanAnalyzer
    {
        private readonly ReadOnlySpan<char> _data;

        public SpanAnalyzer(ReadOnlySpan<char> data) =>
            _data = data;

        public int Length => _data.Length;
        public int LetterCount => _data.ToArray().Count(char.IsLetter);
        public int DigitCount => _data.ToArray().Count(char.IsDigit);

        public override string ToString() =>
            $"Span({Length} chars, {LetterCount} letters, {DigitCount} digits)";
    }

    /// <summary>
    /// Generic method that accepts ref structs via 'allows ref struct'.
    /// </summary>
    private static bool TryParseWith<TParser, TResult>(
        TParser parser,
        ReadOnlySpan<char> input,
        out TResult result)
        where TParser : IParser<TResult>, allows ref struct
    {
        return parser.TryParse(input, out result);
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ ref struct Interfaces ═══╗\n");

        // ── 1. ref struct implementing IParser<T> ────────────
        Console.WriteLine("── ref struct + IParser<T> ──");
        var parser = new TypeNameParser("Dictionary<string, List<int>>");
        Console.WriteLine("  Parsing: Dictionary<string, List<int>>");
        Console.WriteLine("  Tokens:");
        while (parser.HasMore)
        {
            var token = parser.NextToken();
            if (token is not null)
                Console.WriteLine($"    • {token}");
        }

        // ── 2. Using through generic constraint ──────────────
        Console.WriteLine("\n── Generic with 'allows ref struct' ──");
        var parser2 = new TypeNameParser("int");
        if (TryParseWith<TypeNameParser, string>(parser2, "MyClassName", out var parsed))
        {
            Console.WriteLine($"  Parsed: {parsed}");
        }

        // ── 3. ref struct with IDisposable ───────────────────
        Console.WriteLine("\n── ref struct + IDisposable ──");
        Console.WriteLine("  using var parser = new TypeNameParser(input);");
        using (var disposableParser = new TypeNameParser("int x, string y"))
        {
            Console.Write("  Tokens: ");
            while (disposableParser.HasMore)
            {
                var token = disposableParser.NextToken();
                if (token is not null) Console.Write($"{token} ");
            }
            Console.WriteLine();
        } // DisposeAsync called — ref struct cleaned up
        Console.WriteLine("  Parser disposed (stack cleanup).");

        // ── 4. SpanAnalyzer on stack ─────────────────────────
        Console.WriteLine("\n── Stack-Only Span Analysis ──");
        ReadOnlySpan<char> input = "Hello123World456";
        var analyzer = new SpanAnalyzer(input);
        Console.WriteLine($"  Input: \"{input}\"");
        Console.WriteLine($"  Analysis: {analyzer.ToString()}");

        // ── 5. Key constraints ───────────────────────────────
        Console.WriteLine("\n── Key Constraints ──");
        Console.WriteLine("  ref structs CAN:");
        Console.WriteLine("    ✓ Implement interfaces (C# 13)");
        Console.WriteLine("    ✓ Be used in 'using' statements");
        Console.WriteLine("    ✓ Be passed to 'allows ref struct' generics");
        Console.WriteLine();
        Console.WriteLine("  ref structs CANNOT:");
        Console.WriteLine("    ✗ Be boxed (no casting to object/interface variable)");
        Console.WriteLine("    ✗ Be stored in arrays or collections");
        Console.WriteLine("    ✗ Be captured by lambdas or async methods");
        Console.WriteLine("    ✗ Be used as generic type arguments (without 'allows')");
    }
}
