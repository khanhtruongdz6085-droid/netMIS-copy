namespace NetCafeMis.Exceptions;

public sealed class PaymentAmountMismatchException : Exception
{
    public PaymentAmountMismatchException(string message) : base(message) { }
}
