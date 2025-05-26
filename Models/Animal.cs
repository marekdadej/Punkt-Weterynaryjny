using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PunktWeterynaryjny.Models
{
    public class Animal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj imię zwierzaka.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj gatunek zwierzaka.")]
        public string Species { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj rasę zwierzaka.")]
        public string Breed { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data urodzenia")]
        public DateTime BirthDate { get; set; }

        [ForeignKey("Owner")]
        [BindNever]
        public string OwnerId { get; set; } = string.Empty;

        [BindNever]
        public IdentityUser? Owner { get; set; }
    }
}