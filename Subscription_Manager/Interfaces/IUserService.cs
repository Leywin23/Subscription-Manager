using Subscription_Manager.Dtos.Account;

namespace Subscription_Manager.Interfaces
{
    public interface IUserService
    {
        Task<UserSubscriptionOverviewDto> GetUserSubscriptionOverviewAsync(string userId);
    }
}
