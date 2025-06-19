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

                var appUser = await _userManager.FindByNameAsync(loginDto.UserName);
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
        public async Task<IActionResult> GetUserData() {
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
