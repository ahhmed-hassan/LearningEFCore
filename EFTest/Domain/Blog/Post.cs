using System.Xml.Linq;

namespace EFTest.Domain.Blog;

internal class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public User Author { get; set; } = null!;
    public List<Comment> Comments { get; set; } = new();
}
