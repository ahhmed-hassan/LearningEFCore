using _10_Code_first.Entites.ValueObjects;

namespace _10_Code_first.Entites;

public class Instructor
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? OfficeId { get; set; }
    public Address? Address { get; set; }
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
