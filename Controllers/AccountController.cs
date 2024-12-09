using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Registracija uporabnika
        // Registracija uporabnika
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.Phone,
                    EmploymentType = model.EmploymentType,
                    JobTitle = model.JobTitle,
                    IsActive = model.IsActive
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Generiraj JWT token za registriranega uporabnika
                    var token = GenerateJwtToken(user);
                    return Ok(new
                    {
                        token,
                        message = "Registracija uspešna!"
                    });
                }

                return BadRequest(result.Errors);
            }

            return BadRequest("Invalid data");
        }


        // Prijava uporabnika
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new { token, message = "Prijava uspešna!" });
                }
                return Unauthorized(new { message = "Neveljavno uporabniško ime ali geslo" });
            }

            return BadRequest("Invalid data");
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("YourSuperSecretKey1234567890123456");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("FullName", user.FullName),
                    new Claim("EmploymentType", user.EmploymentType),
                    new Claim("JobTitle", user.JobTitle),
                    new Claim("IsActive", user.IsActive.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }


    // Model za registracijo
    public class RegisterModel
    {
        public string Username { get; set; }  // Uporabniško ime
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }  // Telefon uporabnika
        public string Password { get; set; }
        public string EmploymentType { get; set; }  // Vrsta zaposlitve (npr. full-time, part-time)
        public string JobTitle { get; set; }  // Delovno mesto
        public bool IsActive { get; set; }  // Ali je uporabnik aktiven
    }

    // Model za prijavo
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
