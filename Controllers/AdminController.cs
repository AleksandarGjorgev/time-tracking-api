using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Ensure only admins can access this controller
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all users
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.UserName
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading users.", error = ex.Message });
            }
        }

        // Get all work logs for a specific user
        [HttpGet("worklogs/{userId}")]
        public async Task<ActionResult> GetWorkLogsByUser(string userId)
        {
            try
            {
                var workLogs = await _context.WorkLogs
                    .Where(w => w.UserId == userId)
                    .Select(w => new
                    {
                        w.Id,
                        w.Date,
                        w.StartTime,
                        w.EndTime,
                        w.BreakStart,
                        w.BreakEnd
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = workLogs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading work logs.", error = ex.Message });
            }
        }
        

        // Get all absence records for a specific user
        [HttpGet("absences/{userId}")]
        public async Task<ActionResult> GetAbsencesByUser(string userId)
        {
            try
            {
                var absences = await _context.AbsenceRecords
                    .Where(a => a.UserId == userId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Date,
                        a.AbsenceType,
                        a.Description
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = absences });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading absences.", error = ex.Message });
            }
        }

        // Update a user's work log
        [HttpPut("worklogs/{id}")]
        public async Task<IActionResult> UpdateWorkLog(int id, [FromBody] WorkLog updatedWorkLog)
        {
            var existingWorkLog = await _context.WorkLogs.FindAsync(id);
            if (existingWorkLog == null)
            {
                return NotFound(new { success = false, message = "Work log not found." });
            }

            try
            {
                existingWorkLog.Date = updatedWorkLog.Date;
                existingWorkLog.StartTime = updatedWorkLog.StartTime;
                existingWorkLog.EndTime = updatedWorkLog.EndTime;
                existingWorkLog.BreakStart = updatedWorkLog.BreakStart;
                existingWorkLog.BreakEnd = updatedWorkLog.BreakEnd;

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Work log successfully updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating work log.", error = ex.Message });
            }
        }

        // Delete a user's work log
        [HttpDelete("worklogs/{id}")]
        public async Task<IActionResult> DeleteWorkLog(int id)
        {
            var workLog = await _context.WorkLogs.FindAsync(id);
            if (workLog == null)
            {
                return NotFound(new { success = false, message = "Work log not found." });
            }

            try
            {
                _context.WorkLogs.Remove(workLog);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Work log successfully deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting work log.", error = ex.Message });
            }
        }

        // Update a user's absence record
        [HttpPut("absences/{id}")]
        public async Task<IActionResult> UpdateAbsence(int id, [FromBody] AbsenceRecord updatedAbsence)
        {
            var existingAbsence = await _context.AbsenceRecords.FindAsync(id);
            if (existingAbsence == null)
            {
                return NotFound(new { success = false, message = "Absence record not found." });
            }

            try
            {
                existingAbsence.Date = updatedAbsence.Date;
                existingAbsence.AbsenceType = updatedAbsence.AbsenceType;
                existingAbsence.Description = updatedAbsence.Description;

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Absence record successfully updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating absence record.", error = ex.Message });
            }
        }

        // Delete a user's absence record
        [HttpDelete("absences/{id}")]
        public async Task<IActionResult> DeleteAbsence(int id)
        {
            var absence = await _context.AbsenceRecords.FindAsync(id);
            if (absence == null)
            {
                return NotFound(new { success = false, message = "Absence record not found." });
            }

            try
            {
                _context.AbsenceRecords.Remove(absence);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Absence record successfully deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting absence record.", error = ex.Message });
            }
        }
    }
}
