using System.Globalization;

namespace NetCafeMis.Checks;

internal static class Program
{
    private static int Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        if (args.Length > 1) return Usage();
        switch (args.Length == 0 ? "all" : args[0].ToLowerInvariant())
        {
            case "all":
                NetCafeChecks.Run();
                DiscountChecks.Run();
                AccountChecks.Run();
                EarlyEndChecks.Run();
                SessionEndChecks.Run();
                CSharpCompatibilityChecks.Run();
                int total = NetCafeChecks.Count + DiscountChecks.Count + AccountChecks.Count
                    + EarlyEndChecks.Count + SessionEndChecks.Count + CSharpCompatibilityChecks.Count;
                Console.WriteLine($"All checks passed: {total} checks");
                break;
            case "business": NetCafeChecks.Run(); break;
            case "discount": DiscountChecks.Run(); break;
            case "accounts": AccountChecks.Run(); break;
            case "early-end": EarlyEndChecks.Run(); break;
            case "session-end": SessionEndChecks.Run(); break;
            case "csharp": CSharpCompatibilityChecks.Run(); break;
            default: return Usage();
        }
        return 0;
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage: NetCafeMis.Checks [all|business|discount|accounts|early-end|session-end|csharp]");
        return 2;
    }
}
