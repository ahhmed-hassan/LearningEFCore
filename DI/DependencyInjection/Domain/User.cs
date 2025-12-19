
namespace DependencyInjection.Domain;

internal class User
{
    public string Username { get; private set; }
    public SubScriptionTier SubscriptionTier { get; private set; }
    public string Email { get; private set; }

    public string ReferralCode { get; private set; } = Guid.NewGuid().ToString().Substring(0, 8);
    public int ReferralCredits { get; set; } = 0;
    public User(string username,string email,  SubScriptionTier subscriptionTier)
    {
        Username = username;
        SubscriptionTier = subscriptionTier;
        Email = email;
    }
}
