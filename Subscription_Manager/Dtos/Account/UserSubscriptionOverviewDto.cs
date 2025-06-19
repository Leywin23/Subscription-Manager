using Subscription_Manager.Dtos.Subscription;

namespace Subscription_Manager.Dtos.Account
{
    public class UserSubscriptionOverviewDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public decimal MonthlyTotal { get; set; }
        public decimal YearlyTotal { get; set; }
        public List<SubscriptionDto> Subscriptions { get; set; }
    }
}
