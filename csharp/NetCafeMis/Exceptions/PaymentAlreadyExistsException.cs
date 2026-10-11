namespace NetCafeMis.Exceptions;

public sealed class PaymentAlreadyExistsException : Exception
{
    public PaymentAlreadyExistsException(string message) : base(message) { }
}
