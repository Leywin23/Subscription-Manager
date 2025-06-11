namespace Subscription_Manager.Models
{
    public class UserSubscription
    {
        public string AppUserId { get; set; } = null;
        public AppUser AppUser { get; set; } = null;

        public int SubscriptionId {  get; set; }
        public Subscription Subscription { get; set; } = null;
    }
}
