using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PunktWeterynaryjny.Models
{
    public class Animal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj imię zwierzaka.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Podaj gatunek zwierzaka.")]
        public string Species { get; set; }

        [Required(ErrorMessage = "Podaj rasę zwierzaka.")]
        public string Breed { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data urodzenia")]
        public DateTime BirthDate { get; set; }

        [ForeignKey("Owner")]
        public string OwnerId { get; set; }
        public IdentityUser Owner { get; set; }
    }
}