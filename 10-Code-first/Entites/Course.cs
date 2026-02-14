using _10_Code_first.Entites.ValueObjects;

namespace _10_Code_first.Entites;

public class Course
{
    public int Id { get; set; }
    public required string CourseName { get; set; }
    public decimal Price { get; set; }
    public required CourseTag Tag { get; set; }
    public ICollection<Section> Sections { get; set; } = new List<Section>();

}
