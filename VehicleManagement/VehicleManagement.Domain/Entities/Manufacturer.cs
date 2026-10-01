namespace VehicleManagement.Domain.Entities;

public sealed class Manufacturer
{
    private Manufacturer()
    {
        // Required by EF Core.
    }

    public Manufacturer(string name)
    {
        SetName(name);
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public void Rename(string name)
    {
        SetName(name);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Manufacturer name is required.",
                nameof(name));
        }

        Name = name.Trim();
    }
}