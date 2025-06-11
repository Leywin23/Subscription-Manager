using Microsoft.AspNetCore.Identity;
namespace Subscription_Manager.Models
{
    public class AppUser: IdentityUser
    {
        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
