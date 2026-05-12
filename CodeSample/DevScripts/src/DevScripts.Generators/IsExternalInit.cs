// Polyfill for netstandard2.0 — enables init-only properties and records
// when targeting older frameworks (required for Roslyn source generators).

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
