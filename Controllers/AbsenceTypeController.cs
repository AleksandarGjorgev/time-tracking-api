using Microsoft.AspNetCore.Mvc;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbsenceTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AbsenceTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Pridobi vse vrste odsotnosti
        [HttpGet]
        public IActionResult GetAbsenceTypes()
        {
            return Ok(_context.AbsenceTypes.ToList());
        }

        // Dodaj novo vrsto odsotnosti
        [HttpPost]
        public IActionResult AddAbsenceType(AbsenceType absenceType)
        {
            if (string.IsNullOrEmpty(absenceType.Name))
            {
                return BadRequest("Ime odsotnosti je obvezno.");
            }

            _context.AbsenceTypes.Add(absenceType);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetAbsenceTypes), new { id = absenceType.Id }, absenceType);
        }

        // Izbriši vrsto odsotnosti
        [HttpDelete("{id}")]
        public IActionResult DeleteAbsenceType(int id)
        {
            var absenceType = _context.AbsenceTypes.Find(id);
            if (absenceType == null)
            {
                return NotFound();
            }

            _context.AbsenceTypes.Remove(absenceType);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
