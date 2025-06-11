using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subscription_Manager.Data;
using Subscription_Manager.Dtos.Subscription;
using Subscription_Manager.Models;
using System.Security.Claims;

namespace Subscription_Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubscriptionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            var subscriptions = await _context.Subscriptions.ToListAsync();
            if (subscriptions.Count == 0)
            {
                return NotFound("No subscriptions found.");
            }
            return Ok(subscriptions);
        }
        [HttpPost]
        public async Task<IActionResult> AddSubscription([FromBody] SubscriptionDto subscriptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subscription = new Subscription
            {
                ServiceName = subscriptionDto.ServiceName,
                Cost = subscriptionDto.Cost,
                Frequency = subscriptionDto.Frequency,
                StartDate = DateTime.Now,
                Description = subscriptionDto.Description,
                Category = subscriptionDto.Category,
            };

            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();

            return Ok(subscription);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSubscription(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound($"Subscription with id {id} not found.");
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
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

            var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s=>s.Id == subscriptionId);

            if (subscription == null)
                return NotFound($"Subscription with ID {subscriptionId} not found.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            bool alreadyAssigned = await _context.UserSubscriptions
                .AnyAsync(us=>us.AppUserId == userId && us.SubscriptionId == subscriptionId);

            if (alreadyAssigned)
                return BadRequest("This subscription is already assigned to the user.");

            var userSubscription = new UserSubscription
            {
                AppUserId = userId,
                SubscriptionId = subscriptionId,
            };

            await _context.UserSubscriptions.AddAsync(userSubscription);
            await _context.SaveChangesAsync();
            return Ok("Subscription assigned to user successfully.");
        }
    }
}
