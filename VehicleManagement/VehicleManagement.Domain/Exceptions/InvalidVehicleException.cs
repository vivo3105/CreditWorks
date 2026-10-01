namespace VehicleManagement.Domain.Exceptions;

public sealed class InvalidVehicleException : DomainException
{
    public InvalidVehicleException(string message)
        : base(message)
    {
    }
}