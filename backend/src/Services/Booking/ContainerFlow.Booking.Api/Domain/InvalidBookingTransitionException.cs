namespace ContainerFlow.Booking.Api.Domain;

public sealed class InvalidBookingTransitionException : InvalidOperationException
{
    public InvalidBookingTransitionException(string message) : base(message) { }
}