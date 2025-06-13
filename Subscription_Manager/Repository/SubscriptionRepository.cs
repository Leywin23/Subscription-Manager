using Microsoft.EntityFrameworkCore;
using Subscription_Manager.Data;
using Subscription_Manager.Helpers;
using Subscription_Manager.Interfaces;
using Subscription_Manager.Models;

namespace Subscription_Manager.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subscription>> GetAllAsync(QueryObject query)
        {
            var subscriptions = _context.Subscriptions
                .Include(s => s.UserSubscriptions)
                .ThenInclude(us => us.AppUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SubscriptionName))
            {
                subscriptions = subscriptions.Where(s=>s.ServiceName.Contains(query.SubscriptionName));
            }

            return await subscriptions.ToListAsync();
        }

    }
}
