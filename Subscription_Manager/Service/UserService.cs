using Microsoft.EntityFrameworkCore;
using Subscription_Manager.Data;
using Subscription_Manager.Dtos.Account;
using Subscription_Manager.Dtos.Subscription;
using Subscription_Manager.Models;
using Subscription_Manager.Interfaces;

namespace Subscription_Manager.Service
{
    public class UserService: IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserSubscriptionOverviewDto> GetUserSubscriptionOverviewAsync(string userId) {
        
            var user = await _context.Users
                .Include(u=>u.UserSubscriptions)
                .ThenInclude(us=>us.Subscription)
                .FirstOrDefaultAsync(u=>u.Id == userId);

            if (user == null) {
                return null;
            }

            var monthly = 0m;
            var yearly = 0m;

            var subscriptionDtos = user.UserSubscriptions.Select(us =>
            {
                var sub = us.Subscription;
                if (sub.Frequency == SubscriptionType.Monthly)
                {
                    monthly += sub.Cost;
                    yearly += sub.Cost * 12;
                }
                else if (sub.Frequency == SubscriptionType.Yearly) {
                    yearly += sub.Cost;
                }

                return new SubscriptionDto
                {
                    id = sub.Id,
                    ServiceName = sub.ServiceName,
                    Cost = sub.Cost,
                    Frequency = sub.Frequency,
                    Description = sub.Description,
                    Category = sub.Category
                };
            }).ToList();

            return new UserSubscriptionOverviewDto
            {
                UserName = user.UserName,
                Email = user.Email,
                MonthlyTotal = Math.Round(monthly, 2),
                YearlyTotal = Math.Round(yearly, 2),
                Subscriptions = subscriptionDtos
            };

        }
    }
}
