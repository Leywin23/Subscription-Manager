using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subscription_Manager.Data;
using Subscription_Manager.Dtos.Account;
using Subscription_Manager.Dtos.Subscription;
using Subscription_Manager.Helpers;
using Subscription_Manager.Interfaces;
using Subscription_Manager.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

namespace Subscription_Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISubscriptionRepository _subRepo;


        public SubscriptionController(AppDbContext context, ISubscriptionRepository subRepo)
        {
            _context = context;
            _subRepo = subRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions([FromQuery] QueryObject query)
        {
            try
            {
                var subscriptions = await _subRepo.GetAllAsync(query);
                if (subscriptions.Count == 0)
                {
                    return NotFound("No subscriptions found.");
                }

                var subscriptionDtos = subscriptions.Select(s => new SubscriptionDto
                {
                    id = s.Id,
                    ServiceName = s.ServiceName,
                    Cost = s.Cost,
                    Frequency = s.Frequency,
                    StartDate = s.StartDate,
                    Description = s.Description,
                    Category = s.Category,
                    Users = s.UserSubscriptions.Select(us => new UserDto
                    {
                        Id = us.AppUser.Id,
                        UserName = us.AppUser.UserName,
                        Email = us.AppUser.Email
                    }).ToList()
                }).ToList();

                return Ok(subscriptionDtos);
            }
            catch (Exception ex) {
                return StatusCode(500, "An error occurred while retrieving subscription");
            }
        }
        //[HttpPost]
        //public async Task<IActionResult> AddSubscription([FromBody] SubscriptionDto subscriptionDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var subscription = new Subscription
        //    {
        //        ServiceName = subscriptionDto.ServiceName,
        //        Cost = subscriptionDto.Cost,
        //        Frequency = subscriptionDto.Frequency,
        //        StartDate = DateTime.Now,
        //        Description = subscriptionDto.Description,
        //        Category = subscriptionDto.Category,
        //    };

        //    await _context.Subscriptions.AddAsync(subscription);
        //    await _context.SaveChangesAsync();

        //    return Ok(subscription);
        //}

        [HttpPost]
        [Authorize] // <--- DODAJ TEN ATRYBUT
        public async Task<IActionResult> AddSubscription([FromBody] SubscriptionDto subscriptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Pobierz ID zalogowanego użytkownika z tokenu
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                // To nie powinno się zdarzyć z [Authorize], ale to dobre zabezpieczenie
                return Unauthorized("Użytkownik nie jest zalogowany lub ID użytkownika nie znaleziono w tokenie.");
            }

            // 2. Utwórz nową subskrypcję
            var subscription = new Subscription
            {
                ServiceName = subscriptionDto.ServiceName,
                Cost = subscriptionDto.Cost,
                Frequency = subscriptionDto.Frequency,
                StartDate = DateTime.Now, // Użyj DateTime.Now dla nowej subskrypcji
                Description = subscriptionDto.Description,
                Category = subscriptionDto.Category,
            };

            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync(); // Zapisz subskrypcję, aby otrzymała ID

            // 3. Utwórz powiązanie subskrypcji z użytkownikiem
            var userSubscription = new UserSubscription
            {
                AppUserId = userId,
                SubscriptionId = subscription.Id // Użyj ID nowo utworzonej subskrypcji
            };

            await _context.UserSubscriptions.AddAsync(userSubscription);
            await _context.SaveChangesAsync(); // Zapisz powiązanie

            // Możesz zwrócić pełny obiekt Subskrypcji, włącznie z ID
            // Możesz też użyć CreatedAtAction, co jest bardziej typowe dla POST, zwracając 201 Created
            return CreatedAtAction(nameof(AddSubscription), new { id = subscription.Id }, new SubscriptionDto
            {
                id = subscription.Id,
                ServiceName = subscription.ServiceName,
                Cost = subscription.Cost,
                Frequency = subscription.Frequency,
                StartDate = subscription.StartDate,
                Description = subscription.Description,
                Category = subscription.Category,
                // Nie zwracamy tutaj Userów, bo to Subskrypcja, a nie dane użytkownika
            });
        }
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteSubscription(int id)
        {
            Debug.WriteLine($"[SubscriptionController] -> DeleteSubscription called for ID: {id}"); // Dodaj to!
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                Debug.WriteLine($"[SubscriptionController] Subscription with ID: {id} NOT FOUND in DB."); // Dodaj to!
                return NotFound($"Subscription with id {id} not found.");
            }
            Debug.WriteLine($"[SubscriptionController] Found subscription ID: {id}. Preparing to remove."); // Dodaj to!
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            Debug.WriteLine($"[SubscriptionController] Subscription ID: {id} successfully deleted from DB."); // Dodaj to!

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditSubscription(int id, [FromBody] SubscriptionDto subscriptionDto)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null)
            {
                return NotFound($"Subscription with ID {id} not found.");
            }

            // Aktualizacja danych subskrypcji, w tym tytułu
            subscription.ServiceName = subscriptionDto.ServiceName;
            subscription.Cost = subscriptionDto.Cost;
            subscription.Frequency = subscriptionDto.Frequency;
            subscription.StartDate = subscriptionDto.StartDate;
            subscription.Description = subscriptionDto.Description;
            subscription.Category = subscriptionDto.Category;

            await _context.SaveChangesAsync();
            return Ok(subscription);
        }
        [HttpPost("assign/{subscriptionId}")]
        [Authorize]
        public async Task<IActionResult> AssignSubscriptionToUser(int subscriptionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var subscription = await _context.Subscriptions.Include(s=>s.UserSubscriptions).FirstOrDefaultAsync(s=>s.Id == subscriptionId);

            if (subscription == null)
                return NotFound($"Subscription with ID {subscriptionId} not found.");

            var user = await _context.Users.Include(u => u.UserSubscriptions).FirstOrDefaultAsync(u => u.Id == userId);

            bool alreadyAssigned = await _context.UserSubscriptions
                .AnyAsync(us=>us.AppUserId == userId && us.SubscriptionId == subscriptionId);

            if (alreadyAssigned)
                return BadRequest("This subscription is already assigned to the user.");

            var userSubscription = new UserSubscription
            {
                AppUserId = userId,
                SubscriptionId = subscriptionId,
                AppUser = user,
                Subscription = subscription
            };


            await _context.UserSubscriptions.AddAsync(userSubscription);
            await _context.SaveChangesAsync();
            return Ok("Subscription assigned to user successfully.");
        }
        [HttpDelete("assign/{subscriptionId}")]
        [Authorize]
        public async Task<IActionResult> RemoveSubscriptionFromAccount(int subscriptionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var userSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.AppUserId == userId && us.SubscriptionId == subscriptionId);

            if (userSubscription == null)
                return NotFound("Subscription is not assigned to the user.");

            _context.UserSubscriptions.Remove(userSubscription);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
