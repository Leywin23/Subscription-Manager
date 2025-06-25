using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Subscription_Manager.Data;
using Subscription_Manager.Dtos.Account;
using Subscription_Manager.Interfaces;
using Subscription_Manager.Models;
using System.Security.Claims;


namespace Subscription_Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, AppDbContext context, IUserService userService)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var appUser = new AppUser
                {
                    UserName = registerDto.UserName,
                    Email = registerDto.Email,
                };

                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);
                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                    if (roleResult.Succeeded)
                    {
                        return Ok(new NewUserDto
                        {
                            UserName = appUser.UserName,
                            Email = appUser.Email,
                            Token = _tokenService.CreateToken(appUser)
                        });
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return BadRequest(createdUser.Errors);
                }
            }
            catch (Exception)
            {
                // Tu zostawiamy 500, bo to naprawdę błąd serwera
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                //var appUser = await _userManager.FindByNameAsync(loginDto.UserName);
                var appUser = await _userManager.FindByEmailAsync(loginDto.UserName); // Szukaj po emailu, bo frontend wysyła email w polu UserName
                if (appUser != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginDto.Password, false);
                    if (result.Succeeded)
                    {
                        return Ok(new NewUserDto
                        {
                            UserName = appUser.UserName,
                            Email = appUser.Email,
                            Token = _tokenService.CreateToken(appUser)
                        });
                    }
                    else return Unauthorized("username not found and/or password incorrect");
                }
                else return NotFound("Invalid username");
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        [HttpPost("userdata")]
        [Authorize]
        public async Task<IActionResult> GetUserData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var result = await _userService.GetUserSubscriptionOverviewAsync(userId);

            if (result == null)
                return NotFound("User not found.");

            return Ok(result);

        }
    }
}

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Subscription_Manager.Data;
//using Subscription_Manager.Dtos.Account;
//using Subscription_Manager.Interfaces;
//using Subscription_Manager.Models;
//using System.Security.Claims;
//using Microsoft.EntityFrameworkCore; // Dodaj ten using dla Include i FirstOrDefaultAsync
//using System.Linq; // Dodaj ten using dla Any() i Select()

//namespace Subscription_Manager.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AccountController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<AppUser> _userManager;
//        private readonly SignInManager<AppUser> _signInManager;
//        private readonly ITokenService _tokenService;
//        private readonly IUserService _userService;

//        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, AppDbContext context, IUserService userService)
//        {
//            _tokenService = tokenService;
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _context = context;
//            _userService = userService;
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var appUser = new AppUser
//                {
//                    UserName = registerDto.UserName,
//                    Email = registerDto.Email,
//                };

//                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);
//                if (createdUser.Succeeded)
//                {
//                    var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
//                    if (roleResult.Succeeded)
//                    {
//                        return Ok(new NewUserDto
//                        {
//                            UserName = appUser.UserName,
//                            Email = appUser.Email,
//                            Token = _tokenService.CreateToken(appUser)
//                        });
//                    }
//                    else
//                    {
//                        return StatusCode(500, roleResult.Errors);
//                    }
//                }
//                else
//                {
//                    return BadRequest(createdUser.Errors);
//                }
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, new { error = "An unexpected error occurred during registration." });
//            }
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> login([FromBody] LoginDto loginDto)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var appUser = await _userManager.FindByEmailAsync(loginDto.UserName);
//                if (appUser != null)
//                {
//                    var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginDto.Password, false);
//                    if (result.Succeeded)
//                    {
//                        return Ok(new NewUserDto
//                        {
//                            UserName = appUser.UserName,
//                            Email = appUser.Email,
//                            Token = _tokenService.CreateToken(appUser)
//                        });
//                    }
//                    else
//                    {
//                        return Unauthorized("Invalid username and/or password.");
//                    }
//                }
//                else
//                {
//                    return NotFound("Invalid username.");
//                }
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, new { error = "An unexpected error occurred during login." });
//            }
//        }

//        [Authorize]
//        [HttpGet("userdata")] // Zmieniono z Post na Get, aby ułatwić testowanie w przeglądarce
//        public async Task<IActionResult> GetUserData()
//        {
//            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userId))
//            {
//                return Unauthorized("User ID not found in token.");
//            }

//            // Zmodyfikuj zapytanie bezpośrednio tutaj, ABY ZOBACZYĆ SUROWE DANE
//            // To jest tylko tymczasowe dla celów diagnostycznych!
//            var user = await _context.Users
//                .Include(u => u.UserSubscriptions)
//                .ThenInclude(us => us.Subscription)
//                .FirstOrDefaultAsync(u => u.Id == userId);

//            if (user == null)
//            {
//                return NotFound("User not found.");
//            }

//            // --- POCZĄTEK NOWEGO KODU DEBUGOWANIA ---
//            // Zwróć surowe dane UserSubscriptions, aby sprawdzić, czy zostały załadowane
//            if (user.UserSubscriptions == null || !user.UserSubscriptions.Any())
//            {
//                return Ok(new { Message = "User found, but no subscriptions loaded directly from DB.", UserId = userId, UserSubscriptionsCount = 0 });
//            }
//            else
//            {
//                // Zwróć dane w formacie, który pozwoli zobaczyć załadowane subskrypcje
//                var debugOutput = user.UserSubscriptions.Select(us => new
//                {
//                    us.AppUserId,
//                    us.SubscriptionId,
//                    // Poprawka: upewnij się, że obie strony operatora ?: zwracają obiekt o tym samym kształcie i nullability
//                    SubscriptionDetails = us.Subscription != null ? new
//                    {
//                        Id = (int?)us.Subscription.Id, // Zmieniono na nullable int
//                        us.Subscription.ServiceName,
//                        Cost = (decimal?)us.Subscription.Cost, // Zmieniono na nullable decimal
//                        Frequency = us.Subscription.Frequency.ToString(), // Konwersja Enum na string
//                        us.Subscription.Description,
//                        us.Subscription.Category
//                    } : new
//                    {
//                        Id = (int?)null,
//                        ServiceName = (string)null,
//                        Cost = (decimal?)null,
//                        Frequency = (string)null,
//                        Description = (string)null,
//                        Category = (string)null
//                    }
//                }).ToList();

//                return Ok(new { Message = "Debug Data: Raw User Subscriptions", UserSubscriptionsCount = debugOutput.Count, Data = debugOutput });
//            }
//            // --- KONIEC NOWEGO KODU DEBUGOWANIA ---

//            // Poniższy kod jest oryginalnym, który byłby normalnie używany po usunięciu powyższego bloku.
//            // var overview = await _userService.GetUserSubscriptionOverviewAsync(userId);
//            // if (overview == null)
//            // {
//            //     return NotFound("User data or subscriptions not found.");
//            // }
//            // return Ok(overview);
//        }
//    }
//}