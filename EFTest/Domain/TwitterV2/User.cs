
namespace EFTest.Domain.TwitterV2;

internal class User
{
    // Primary key convention [Id, id , ID] , [{Class}Id]
    public int UserId { get; set; }
    public required string Username { get; set; }
}
