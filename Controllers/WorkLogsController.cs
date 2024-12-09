using Microsoft.AspNetCore.Authorization;
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
    public class WorkLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkLogsController(ApplicationDbContext context)
        {
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

            if (jsonToken == null)
            {
                return null;
            }

            // Get user ID from the token claim
            var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            return userId;
        }

        // Get work logs for the currently authenticated user
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<WorkLog>>> GetMyWorkLogs()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = true, message = "Token ni prisoten v glavi zahtevka." });
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return Unauthorized(new { success = false, message = "Neveljaven token." });
                }

                var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Uporabnik ni prijavljen." });
                }

                // Query to return only necessary data (user's work logs)
                var workLogs = await _context.WorkLogs
                    .Where(w => w.UserId == userId)
                    .Select(w => new WorkLog
                    {
                        Id = w.Id,
                        UserId = w.UserId,
                        Date = w.Date,
                        StartTime = w.StartTime,
                        EndTime = w.EndTime,
                        BreakStart = w.BreakStart,
                        BreakEnd = w.BreakEnd
                    })
                    .ToListAsync();


                return Ok(new { success = true, data = workLogs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Napaka pri nalaganju delovnih zapisov.", error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult<WorkLog>> PostWorkLog([FromBody] CreateWorkLogDto workLogDto)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Uporabnik ni prijavljen." });
            }

            try
            {
                // Map the DTO to the WorkLog entity
                var workLog = new WorkLog
                {
                    UserId = userId,
                    Date = workLogDto.Date,
                    StartTime = workLogDto.StartTime,
                    EndTime = workLogDto.EndTime,
                    BreakStart = workLogDto.BreakStart,
                    BreakEnd = workLogDto.BreakEnd
                };

                _context.WorkLogs.Add(workLog);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMyWorkLogs), new { id = workLog.Id }, new { success = true, data = workLog });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Napaka pri dodajanju delovnega zapisa.", error = ex.Message });
            }
        }


        // Update an existing work log (only for the logged-in user or admin)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWorkLog(int id, [FromBody] WorkLog workLog)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Uporabnik ni prijavljen." });
            }

            var existingWorkLog = await _context.WorkLogs.FindAsync(id);
            if (existingWorkLog == null)
            {
                return NotFound(new { success = false, message = "Delovni zapis ni bil najden." });
            }

            if (existingWorkLog.UserId != userId && !User.IsInRole("Admin"))
            {
                return Unauthorized(new { success = false, message = "Ne morete spreminjati tega delovnega zapisa." });
            }

            existingWorkLog.Date = workLog.Date;
            existingWorkLog.StartTime = workLog.StartTime;
            existingWorkLog.EndTime = workLog.EndTime;
            existingWorkLog.BreakStart = workLog.BreakStart;
            existingWorkLog.BreakEnd = workLog.BreakEnd;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Delovni zapis je bil uspešno posodobljen." });
        }

        // Delete a work log (only for the logged-in user or admin)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkLog(int id)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Uporabnik ni prijavljen." });
            }

            var workLog = await _context.WorkLogs.FindAsync(id);
            if (workLog == null)
            {
                return NotFound(new { success = false, message = "Delovni zapis ni bil najden." });
            }

            if (workLog.UserId != userId && !User.IsInRole("Admin"))
            {
                return Unauthorized(new { success = false, message = "Ne morete brisati tega delovnega zapisa." });
            }

            _context.WorkLogs.Remove(workLog);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Delovni zapis je bil uspešno izbrisan." });
        }
    }
}

public class CreateWorkLogDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStart { get; set; }
    public TimeSpan? BreakEnd { get; set; }
}
