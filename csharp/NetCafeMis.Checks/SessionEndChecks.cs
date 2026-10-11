namespace NetCafeMis.Checks;

/** Service and direct completion must preserve the same session invariants. */
public static class SessionEndChecks
{
    private static int checks;
    internal static int Count => checks;

    private static DateTime TimeAt(int hour, int minute)
    {
        return new DateTime(2026, 10, 5, hour, minute, 0);
    }

    private static void Equal(object? expected, object? actual)
    {
        checks++;
        CheckAssert.Equal(expected, actual);
    }

    private static void Fails(Type type, Action action)
    {
        checks++;
        CheckAssert.Fails(type, action);
    }


    private static void End(NetCafe cafe, Session session, DateTime? time, bool direct)
    {
        if (direct) session.EndSession(time);
        else cafe.EndSession(session, time);
    }

    private static void Completion(bool direct)
    {
        NetCafe cafe = SampleData.Sample();
        Session session = cafe.Sessions[3];
        Equal(1, session.FoodServices.Count);
        Equal(true, session.FoodServices[0] == cafe.Services[2]);
        DateTime?[] invalid = { null, TimeAt(11, 0), TimeAt(10, 59), TimeAt(11, 1), TimeAt(11, 14).AddSeconds(59) };
        foreach (DateTime? proposed in invalid)
        {
            Fails(typeof(InvalidTimeException), () => End(cafe, session, proposed, direct));
            Equal(null, session.TimeEnd);
            Equal(SessionStatus.ACTIVE, session.SessionStatus);
            Equal(ComputerStatus.IN_USE, session.Computer.ComputerStatus);
            Equal(1, session.FoodServices.Count);
            Equal(3, cafe.Services.Count);
        }

        End(cafe, session, TimeAt(12, 0), direct);
        Equal(TimeAt(12, 0), session.TimeEnd);
        Equal(SessionStatus.COMPLETED, session.SessionStatus);
        Equal(ComputerStatus.AVAILABLE, session.Computer.ComputerStatus);
        // Repeating through either API must fail without rewriting the actual end.
        Fails(typeof(SessionStateException), () => session.EndSession(TimeAt(12, 1)));
        Fails(typeof(SessionStateException), () => cafe.EndSession(session, TimeAt(12, 1)));
        Equal(TimeAt(12, 0), session.TimeEnd);
        Equal(ComputerStatus.AVAILABLE, session.Computer.ComputerStatus);
        FoodService lateOrder = new FoodService("SV04", session, cafe.Employees[0], TimeAt(11, 30));
        lateOrder.AddItem(cafe.Foods[0], 1, (decimal)(13000));
        Fails(typeof(SessionStateException), () => session.RecordFoodService(lateOrder));
        Equal(1, session.FoodServices.Count);

        Receipt receipt = cafe.IssueReceipt("R03", session, cafe.Employees[0]);
        Equal(12000, receipt.GrossComputerAmount);
        Equal(20000, receipt.GrossServiceAmount);
        Equal(32000, receipt.TotalAmount);
        cafe.Pay("P02", receipt, TimeAt(12, 0), PaymentMethod.CASH, receipt.TotalAmount);
        Equal(81000, cafe.CollectedAmount());
        Session next = cafe.OpenSession("S05", session.Account, session.Computer, TimeAt(12, 0));
        Equal(SessionStatus.ACTIVE, next.SessionStatus);
        Equal(ComputerStatus.IN_USE, next.Computer.ComputerStatus);

        // Ending exactly at the last order timestamp is valid through both APIs.
        NetCafe boundaryCafe = SampleData.Sample();
        Session boundary = boundaryCafe.Sessions[3];
        End(boundaryCafe, boundary, TimeAt(11, 15), direct);
        Equal(TimeAt(11, 15), boundary.TimeEnd);
        Equal(ComputerStatus.AVAILABLE, boundary.Computer.ComputerStatus);
        Equal(23000, boundaryCafe.IssueReceipt("R03", boundary, boundaryCafe.Employees[0]).TotalAmount);
    }

    private static void Orders()
    {
        NetCafe cafe = SampleData.Sample();
        Session session = cafe.Sessions[3];
        Employee employee = cafe.Employees[0];
        Food food = cafe.Foods[0];
        IList<FoodService> view = session.FoodServices;
        Fails(typeof(NotSupportedException), () => view.Clear());
        Fails(typeof(NotSupportedException), () => view.RemoveAt(0));
        Fails(typeof(NotSupportedException), () => view.Add(cafe.Services[0]));
        Fails(typeof(ValidationException), () => session.RecordFoodService(null));
        Fails(typeof(InvalidOrderException), () => session.RecordFoodService(cafe.Services[0]));

        FoodService beforeStart = new FoodService("SV04", session, employee, TimeAt(10, 59));
        beforeStart.AddItem(food, 1, (decimal)(13000));
        Fails(typeof(InvalidTimeException), () => session.RecordFoodService(beforeStart));
        FoodService empty = new FoodService("SV04", session, employee, TimeAt(11, 30));
        Fails(typeof(InvalidOrderException), () => session.RecordFoodService(empty));
        Fails(typeof(DuplicateIdException), () => session.RecordFoodService(cafe.Services[2]));
        FoodService duplicate = new FoodService("SV03", session, employee, TimeAt(11, 30));
        duplicate.AddItem(food, 1, (decimal)(13000));
        Fails(typeof(DuplicateIdException), () => session.RecordFoodService(duplicate));
        Equal(1, view.Count);
        Equal(3, cafe.Services.Count);

        // A failure midway through building an order must not attach a partial order.
        Food unknown = new Food("F99", "Unknown", (decimal)(13000));
        Fails(typeof(EntityNotFoundException), () => cafe.Order("SV04", session, employee, TimeAt(11, 30),
                new Food[] { food, unknown }, new int[] { 1, 1 },
                new decimal?[] { (decimal)(13000), (decimal)(13000) }));
        Fails(typeof(InvalidOrderException), () => cafe.Order("SV04", session, employee, TimeAt(11, 30),
                new Food[] { food, food }, new int[] { int.MaxValue, 1 }));
        Equal(1, view.Count);
        Equal(3, cafe.Services.Count);

        FoodService recorded = cafe.Order("SV04", session, employee, TimeAt(11, 30), new Food[] { food }, new int[] { 1 });
        Equal(2, view.Count);
        Equal(4, cafe.Services.Count);
        Equal(true, view[1] == recorded);
        Equal(true, cafe.Services[3] == recorded);
        Fails(typeof(InvalidOrderException), () => recorded.AddItem(food, 1, (decimal)(13000)));
        Fails(typeof(InvalidTimeException), () => session.EndSession(TimeAt(11, 20)));
        Equal(null, session.TimeEnd);
        Equal(SessionStatus.ACTIVE, session.SessionStatus);
        Equal(ComputerStatus.IN_USE, session.Computer.ComputerStatus);
        session.EndSession(TimeAt(11, 30));
        Equal(TimeAt(11, 30), session.TimeEnd);
        Equal(ComputerStatus.AVAILABLE, session.Computer.ComputerStatus);

        // Registration remains a responsibility of the cafe's service.
        NetCafe otherCafe = SampleData.Sample();
        Session foreign = otherCafe.Sessions[3];
        Fails(typeof(EntityNotFoundException), () => cafe.EndSession(foreign, TimeAt(12, 0)));
        Equal(null, foreign.TimeEnd);
        Equal(SessionStatus.ACTIVE, foreign.SessionStatus);
        Equal(ComputerStatus.IN_USE, foreign.Computer.ComputerStatus);
    }

    public static void Run()
    {
        checks = 0;
        Completion(false);
        Completion(true);
        Orders();
        Console.WriteLine("SessionEndChecks OK: " + checks + " checks");
    }
}
