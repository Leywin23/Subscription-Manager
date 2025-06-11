using Microsoft.AspNetCore.Identity;
namespace Subscription_Manager.Models
{
    public class AppUser: IdentityUser
    {
        public List<Subscription> Subscriptions = new List<Subscription>();
    }
}
