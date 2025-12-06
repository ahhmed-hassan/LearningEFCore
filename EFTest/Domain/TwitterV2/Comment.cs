namespace EFTest.Domain.TwitterV2;

internal class Comment
{
    public int CommentId { get; set; }
    public int TweetId { get; set; }
    public int UserId { get; set; }
    public required string CommentText { get; set; }
    public DateTime CreatedAt { get; set; }

}
