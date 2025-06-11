using System.ComponentModel.DataAnnotations;

namespace Subscription_Manager.Dtos.Subscription
{
    public class SubscriptionDto
    {
        public string ServiceName { get; set; }
        public decimal Cost { get; set; }
        public string Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
    }
}
