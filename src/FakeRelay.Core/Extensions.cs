namespace FakeRelay.Core;

public static class Extensions
{
    public static bool StartsWithCI(this string s, string prefix) =>
        s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
}
