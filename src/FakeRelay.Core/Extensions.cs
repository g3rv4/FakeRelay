using System.Diagnostics.CodeAnalysis;

namespace FakeRelay.Core;

public static class Extensions
{
    public static bool StartsWithCI(this string s, string prefix) =>
        s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

    public static bool IsNullOrEmpty([NotNullWhen(returnValue: false)]this string? s) => string.IsNullOrEmpty(s);

    public static bool HasValue([NotNullWhen(returnValue: true)]this string? s) => !s.IsNullOrEmpty();
}
