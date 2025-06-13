using Subscription_Manager.Helpers;
using Subscription_Manager.Models;

namespace Subscription_Manager.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllAsync(QueryObject query);
    }
}
