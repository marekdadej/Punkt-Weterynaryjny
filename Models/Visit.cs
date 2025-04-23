using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PunktWeterynaryjny.Models
{
    public enum VisitStatus
    {
        Zarejestrowana,
        WRealizacji,
        Zakończona,
        Anulowana
    }

    public class Visit
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;

        [Required]
        public int PetId { get; set; }
        public Animal Pet { get; set; } = null!;

        [Required]
        public DateTime VisitDate { get; set; }

        public string? Description { get; set; }

        public VisitStatus Status { get; set; }
    }
}