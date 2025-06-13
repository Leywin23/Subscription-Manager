using Subscription_Manager.Dtos.Account;
using Subscription_Manager.Models;
using System.ComponentModel.DataAnnotations;

namespace Subscription_Manager.Dtos.Subscription
{
    public class SubscriptionDto
    {
        public int id {  get; set; }
        public string ServiceName { get; set; }
        public decimal Cost { get; set; }
        public SubscriptionType Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public List<UserDto>? Users { get; set; }
    }
}
