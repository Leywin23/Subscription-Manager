using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Subscription_Manager.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ServiceName { get; set; }
        [Required]
        public decimal Cost {  get; set; }
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SubscriptionType Frequency {  get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public string? Description {  get; set; }
        public string? Category {  get; set; }
        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();

    }

    public enum SubscriptionType
    {
        Monthly = 1,
        Yearly = 2
    }
}
