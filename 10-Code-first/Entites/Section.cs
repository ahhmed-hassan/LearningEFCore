namespace _10_Code_first.Entites;

public class Section
{
    public int Id { get; set; }
    public required string SectionName { get; set; }
    public int CourseId { get; set; }
    public int? InstructorId { get; set; }
}
