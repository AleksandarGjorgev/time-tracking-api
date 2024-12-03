using Microsoft.AspNetCore.Mvc;
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

        // Pridobi vse beleženje časa ali filtriraj po mesecu
        [HttpGet]
        public IActionResult GetWorkLogs([FromQuery] string month)
        {
            if (!string.IsNullOrEmpty(month))
            {
                var logsForMonth = _context.WorkLogs
                    .Where(w => w.Date.ToString("yyyy-MM").Equals(month))
                    .ToList();
                return Ok(logsForMonth);
            }

            return Ok(_context.WorkLogs.ToList());
        }

        // Dodaj novo beleženje časa
        [HttpPost]
        public IActionResult AddWorkLog(WorkLog workLog)
        {
            if (workLog == null || workLog.Date == DateTime.MinValue)
            {
                return BadRequest("Manjkajo podatki o datumu, začetku ali koncu dela.");
            }

            _context.WorkLogs.Add(workLog);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetWorkLogs), new { id = workLog.Id }, workLog);
        }

        // Izbriši beleženje časa
        [HttpDelete("{id}")]
        public IActionResult DeleteWorkLog(int id)
        {
            var workLog = _context.WorkLogs.Find(id);
            if (workLog == null)
            {
                return NotFound();
            }

            _context.WorkLogs.Remove(workLog);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
