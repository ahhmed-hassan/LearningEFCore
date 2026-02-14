namespace _10_Code_first.Entites.ValueObjects;

public class Address
{
    public string Street { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string ZipCode { get; private set; } = null!;

    // EF Core needs a parameterless constructor to materialize objects from the database
    private Address() { }

    public Address(string street, string city, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty.", nameof(zipCode));

        Street = street;
        City = city;
        ZipCode = zipCode;
    }

    // Value object equality: two addresses are equal if all their properties match
    public override bool Equals(object? obj) =>
        obj is Address other &&
        Street == other.Street &&
        City == other.City &&
        ZipCode == other.ZipCode;

    public override int GetHashCode() =>
        HashCode.Combine(Street, City, ZipCode);
}
