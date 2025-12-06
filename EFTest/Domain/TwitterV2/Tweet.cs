

namespace EFTest.Domain.TwitterV2;

internal class Tweet
{
    public int TweetId { get; set; }
    public int UserId { get; set; }
    public required string TweetText { get; set; }
    public DateTime CreatedAt { get; set; }
}
