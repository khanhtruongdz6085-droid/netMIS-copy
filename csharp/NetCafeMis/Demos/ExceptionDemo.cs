namespace NetCafeMis;

/** Run separately to see how the caller handles business errors. */
public static class ExceptionDemo
{
    public static void Run()
    {
        NetCafe cafe = SampleData.Sample();
        try
        {
            cafe.AddCustomer(new NormalCustomer("C01", "Other customer"));
        }
        catch (DuplicateIdException failure)
        {
            Console.WriteLine("DuplicateIdException: " + failure.Message);
        }

        try
        {
            cafe.IssueReceipt("R03", cafe.Sessions[3], cafe.Employees[0]);
        }
        catch (SessionStateException failure)
        {
            Console.WriteLine("SessionStateException: " + failure.Message);
        }

        try
        {
            cafe.Pay("P02", cafe.Receipts[1], new DateTime(2026, 10, 5, 12, 0, 0),
                     PaymentMethod.CASH, 1m);
        }
        catch (PaymentAmountMismatchException failure)
        {
            Console.WriteLine("PaymentAmountMismatchException: " + failure.Message);
        }
        Console.WriteLine("Collected remains=" + cafe.CollectedAmount());
    }
}
