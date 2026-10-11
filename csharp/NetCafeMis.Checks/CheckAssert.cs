using System.Globalization;

namespace NetCafeMis.Checks;

internal static class CheckAssert
{
    internal static void Equal(object? expected, object? actual)
    {
        bool same;
        if (IsNumber(expected) && IsNumber(actual))
            same = Convert.ToDecimal(expected, CultureInfo.InvariantCulture) == Convert.ToDecimal(actual, CultureInfo.InvariantCulture);
        else
            same = Equals(expected, actual) || (expected is string && string.Equals(
                (string)expected, Convert.ToString(actual, CultureInfo.InvariantCulture), StringComparison.Ordinal));
        if (!same) throw new InvalidOperationException($"Expected {expected ?? "null"}, got {actual ?? "null"}");
    }

    private static bool IsNumber(object? value) => value is decimal or int or long or double or float;

    internal static void Fails(Type type, Action action)
    {
        try { action(); }
        catch (Exception failure)
        {
            if (type.IsInstanceOfType(failure)) return;
            throw new InvalidOperationException("Expected " + type.Name + ", got " + failure.GetType().Name, failure);
        }
        throw new InvalidOperationException("Expected " + type.Name);
    }
}
