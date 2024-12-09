using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace TimeTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbsenceRecordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AbsenceRecordController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Helper method to extract user ID from JWT token
        private string GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            return jsonToken?.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
        }

        // Record absence
        [HttpPost("record-absence")]
        public async Task<IActionResult> RecordAbsence([FromBody] AbsenceRecordModel model)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Neveljaven ali manjkajoč token." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Uporabnik ni bil najden" });
            }

            var existingRecord = await _context.AbsenceRecords
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Date == model.Date);

            if (existingRecord != null)
            {
                return Conflict(new { success = false, message = "Za izbran dan že obstaja odsotnost." });
            }

            var absenceRecord = new AbsenceRecord
            {
                UserId = user.Id,
                Date = model.Date,
                AbsenceType = model.AbsenceType,
                Description = model.Description ?? string.Empty
            };

            await _context.AbsenceRecords.AddAsync(absenceRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Odsotnost je bila uspešno zabeležena!" });
        }

        // Get absence records for the authenticated user
        [HttpGet("get-absence-records")]
        public async Task<IActionResult> GetAbsenceRecords()
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Neveljaven ali manjkajoč token." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Uporabnik ni bil najden" });
            }

            var absenceRecords = await _context.AbsenceRecords
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            return Ok(new { success = true, data = absenceRecords });
        }

        // Update absence record
        [HttpPut("update-absence/{id}")]
        public async Task<IActionResult> UpdateAbsence(int id, [FromBody] AbsenceRecordModel model)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Neveljaven ali manjkajoč token." });
            }

            var absenceRecord = await _context.AbsenceRecords.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (absenceRecord == null)
            {
                return NotFound(new { success = false, message = "Odsotnost ni bila najdena." });
            }

            absenceRecord.Date = model.Date;
            absenceRecord.AbsenceType = model.AbsenceType;
            absenceRecord.Description = model.Description ?? string.Empty;

            _context.AbsenceRecords.Update(absenceRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Odsotnost je bila uspešno posodobljena." });
        }

        // Delete absence record
        [HttpDelete("delete-absence/{id}")]
        public async Task<IActionResult> DeleteAbsence(int id)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Neveljaven ali manjkajoč token." });
            }

            var absenceRecord = await _context.AbsenceRecords.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (absenceRecord == null)
            {
                return NotFound(new { success = false, message = "Odsotnost ni bila najdena." });
            }

            _context.AbsenceRecords.Remove(absenceRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Odsotnost je bila uspešno izbrisana." });
        }
    }

    // Model for absence record
    public class AbsenceRecordModel
    {
        public DateTime Date { get; set; } // Single day of absence
        public required string AbsenceType { get; set; } // e.g., "Sick", "Vacation"
        public string? Description { get; set; } // Reason description
    }
}
