namespace NetCafeMis.Models;

internal static class Checks
{
    internal static string Id(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("ID/name is required");
        return value.Trim();
    }

    internal static string Name(string? value) => Id(value);

    internal static decimal Money(decimal? value)
    {
        if (value is null || value < 0)
            throw new ValidationException("Amount must be non-null and non-negative");
        return value.Value;
    }

    internal static T Required<T>(T? value, string field) where T : class =>
        value ?? throw new ValidationException(field + " is required");

    internal static T RequiredValue<T>(T? value, string field) where T : struct =>
        value ?? throw new ValidationException(field + " is required");

    // C# permits casting arbitrary integers to enums; Java enum instances cannot do this.
    internal static T EnumValue<T>(T? value, string field) where T : struct, Enum
    {
        T result = RequiredValue(value, field);
        if (!Enum.IsDefined(result)) throw new ValidationException(field + " is invalid");
        return result;
    }
}
