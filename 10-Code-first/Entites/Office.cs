
namespace _10_Code_first.Entites;

public class Office
{
    public int Id { get; set; }
    public required string officeName { get; set; } 
    public Instructor? instructor { get; set; }
}
