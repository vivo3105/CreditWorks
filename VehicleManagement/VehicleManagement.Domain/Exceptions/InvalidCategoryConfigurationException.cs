namespace VehicleManagement.Domain.Exceptions;

public sealed class InvalidCategoryConfigurationException
    : DomainException
{
    public InvalidCategoryConfigurationException(string message)
        : base(message)
    {
    }
}