using Subscription_Manager.Models;

namespace Subscription_Manager.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(AppUser user);
    }
}
