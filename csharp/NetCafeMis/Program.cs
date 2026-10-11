using System.Globalization;
using System.Text;

namespace NetCafeMis;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        if (args.Length > 1) return Usage();
        switch (args.Length == 0 ? "sample" : args[0].ToLowerInvariant())
        {
            case "sample": SampleData.Run(); break;
            case "discount": DiscountDemo.Run(); break;
            case "early-end": EarlyEndDemo.Run(); break;
            case "exceptions": ExceptionDemo.Run(); break;
            default: return Usage();
        }
        return 0;
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage: NetCafeMis [sample|discount|early-end|exceptions]");
        return 2;
    }
}
