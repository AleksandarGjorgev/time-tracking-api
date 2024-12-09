using Microsoft.AspNetCore.Identity;
using System;

namespace TimeTrackingAPI.Models
{
    // Razširitev IdentityUser za dodajanje lastnosti, specifičnih za uporabnika
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }  // Polno ime uporabnika
        public string Phone { get; set; } = string.Empty;  // Telefon uporabnika
        public string EmploymentType { get; set; } = string.Empty;  // Vrsta zaposlitve (npr. full-time, part-time)
		public string JobTitle { get; set; } = string.Empty;  // Delovno mesto uporabnika
        public bool IsActive { get; set; }  // Ali je uporabnik aktiven
    }

    // Razred za beleženje odsotnosti uporabnika
    public class AbsenceRecord
    {
        public int Id { get; set; }  // Unikatna ID vrednost za zapis odsotnosti
        public string UserId { get; set; }  // Povezava na uporabnika, ki je bil odsoten
        public DateTime Date { get; set; }  // Datum odsotnosti
        public string AbsenceType { get; set; } = string.Empty;  // Vrsta odsotnosti (npr. bolniška, dopust, varstvo)
        public string Description { get; set; } = string.Empty;  // Dodatni opis (npr. razlog odsotnosti)
    }


    // Razred za beleženje delovnega časa uporabnikov
    public class WorkLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } // UserId will be set by the server.
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }
    }

}
