using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PunktWeterynaryjny.Models
{
    public enum VisitStatus
    {
        Zarejestrowana,
        W_Realizacji,
        Zakończona,
        Anulowana
    }

    public class Visit
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }

        [Required(ErrorMessage = "Wybierz zwierzaka.")]
        [Display(Name = "Zwierzak")]
        public int PetId { get; set; }
        public Animal? Pet { get; set; }

        [Required(ErrorMessage = "Podaj datę wizyty.")]
        [Display(Name = "Data wizyty")]
        public DateTime VisitDate { get; set; }

        [Display(Name = "Opis wizyty")]
        public string? Description { get; set; }

        [Display(Name = "Czy to wizyta wyjazdowa?")]
        public bool IsOutVisit { get; set; } = false;

        [Display(Name = "Adres wizyty wyjazdowej")]
        public string? OutVisitAddress { get; set; }

        public VisitStatus Status { get; set; }
    }
}