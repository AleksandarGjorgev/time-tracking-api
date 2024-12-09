using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Get current user data (based on JWT token)
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Uporabnik ni prijavljen." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Uporabnik ni bil najden." });
            }

            // Return user data excluding sensitive fields
            var userDto = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.EmploymentType,
                user.JobTitle,
                user.IsActive
            };

            return Ok(userDto);
        }

        // Update account details
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountModel model)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Uporabnik ni prijavljen." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Uporabnik ni bil najden." });
            }

            // Update user properties only if they are provided
            if (!string.IsNullOrEmpty(model.Username)) user.UserName = model.Username;
            if (!string.IsNullOrEmpty(model.FullName)) user.FullName = model.FullName;
            if (!string.IsNullOrEmpty(model.Email)) user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Phone)) user.PhoneNumber = model.Phone;
            if (!string.IsNullOrEmpty(model.EmploymentType)) user.EmploymentType = model.EmploymentType;
            if (!string.IsNullOrEmpty(model.JobTitle)) user.JobTitle = model.JobTitle;
            if (model.IsActive.HasValue) user.IsActive = model.IsActive.Value;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "Račun uspešno posodobljen." });
            }

            return BadRequest(new { message = "Napaka pri posodabljanju računa.", errors = result.Errors });
        }

        // Change password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Uporabnik ni prijavljen." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Uporabnik ni bil najden." });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { message = "Geslo uspešno spremenjeno." });
            }

            return BadRequest(new { message = "Napaka pri spreminjanju gesla.", errors = result.Errors });
        }

        // Helper method to extract user ID from JWT token
        private string GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            return jsonToken?.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
        }
    }

    // Models
    public class UpdateAccountModel
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string EmploymentType { get; set; }
        public string JobTitle { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
