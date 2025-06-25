//using Microsoft.EntityFrameworkCore;
//using Subscription_Manager.Data;
//using Subscription_Manager.Dtos.Account;
//using Subscription_Manager.Dtos.Subscription;
//using Subscription_Manager.Models;
//using Subscription_Manager.Interfaces;

//namespace Subscription_Manager.Service
//{
//    public class UserService: IUserService
//    {
//        private readonly AppDbContext _context;

//        public UserService(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<UserSubscriptionOverviewDto> GetUserSubscriptionOverviewAsync(string userId) {

//            var user = await _context.Users
//                .Include(u=>u.UserSubscriptions)
//                .ThenInclude(us=>us.Subscription)
//                .FirstOrDefaultAsync(u=>u.Id == userId);

//            if (user == null) {
//                return null;
//            }

//            var monthly = 0m;
//            var yearly = 0m;

//            var subscriptionDtos = user.UserSubscriptions.Select(us =>
//            {
//                var sub = us.Subscription;
//                if (sub.Frequency == SubscriptionType.Monthly)
//                {
//                    monthly += sub.Cost;
//                    yearly += sub.Cost * 12;
//                }
//                else if (sub.Frequency == SubscriptionType.Yearly) {
//                    yearly += sub.Cost;
//                }

//                return new SubscriptionDto
//                {
//                    id = sub.Id,
//                    ServiceName = sub.ServiceName,
//                    Cost = sub.Cost,
//                    Frequency = sub.Frequency,
//                    Description = sub.Description,
//                    Category = sub.Category
//                };
//            }).ToList();

//            return new UserSubscriptionOverviewDto
//            {
//                UserName = user.UserName,
//                Email = user.Email,
//                MonthlyTotal = Math.Round(monthly, 2),
//                YearlyTotal = Math.Round(yearly, 2),
//                Subscriptions = subscriptionDtos
//            };

//        }
//    }
//}

using Microsoft.EntityFrameworkCore;
using Subscription_Manager.Data;
using Subscription_Manager.Dtos.Account;
using Subscription_Manager.Dtos.Subscription;
using Subscription_Manager.Models;
using Subscription_Manager.Interfaces;
using System.Diagnostics; // Upewnij się, że masz ten using!

namespace Subscription_Manager.Service
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserSubscriptionOverviewDto> GetUserSubscriptionOverviewAsync(string userId)
        {
            Debug.WriteLine($"[UserService] -> GetUserSubscriptionOverviewAsync called for userId: {userId}");

            var user = await _context.Users
                .Include(u => u.UserSubscriptions)
                .ThenInclude(us => us.Subscription)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                Debug.WriteLine($"[UserService] User with ID {userId} NOT FOUND.");
                return null;
            }

            Debug.WriteLine($"[UserService] User found: {user.UserName} (ID: {user.Id})");
            Debug.WriteLine($"[UserService] Number of user subscriptions loaded by Include: {user.UserSubscriptions.Count}");

            // Sprawdź dokładnie, co zawiera lista UserSubscriptions
            if (user.UserSubscriptions.Any())
            {
                foreach (var us in user.UserSubscriptions)
                {
                    Debug.WriteLine($"[UserService]   UserSubscription (AppUserId: {us.AppUserId}, SubscriptionId: {us.SubscriptionId})");
                    if (us.Subscription != null)
                    {
                        Debug.WriteLine($"[UserService]     -> Linked Subscription: ID={us.Subscription.Id}, Name={us.Subscription.ServiceName}, Cost={us.Subscription.Cost}, Freq={us.Subscription.Frequency}, Desc={us.Subscription.Description}, Cat={us.Subscription.Category}");
                    }
                    else
                    {
                        Debug.WriteLine($"[UserService]     -> ERROR: Subscription is NULL for UserSubscription ID: {us.SubscriptionId}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("[UserService] No UserSubscriptions found for this user after Include.");
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
                else if (sub.Frequency == SubscriptionType.Yearly)
                {
                    yearly += sub.Cost;
                }

                return new SubscriptionDto
                {
                    id = sub.Id,
                    ServiceName = sub.ServiceName,
                    Cost = sub.Cost,
                    Frequency = sub.Frequency,
                    StartDate = sub.StartDate,
                    Description = sub.Description,
                    Category = sub.Category
                };
            }).ToList();

            Debug.WriteLine($"[UserService] Calculated Monthly Total: {monthly}");
            Debug.WriteLine($"[UserService] Calculated Yearly Total: {yearly}");
            Debug.WriteLine($"[UserService] Number of SubscriptionDtos prepared: {subscriptionDtos.Count}");

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
