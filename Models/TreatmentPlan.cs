using System;
using System.ComponentModel.DataAnnotations;

namespace PunktWeterynaryjny.Models
{
    public class TreatmentPlan
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }
        public Animal Animal { get; set; }

        [Required(ErrorMessage = "Podaj datę wpisu.")]
        [Display(Name = "Data wpisu")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Podaj opis leczenia.")]
        [Display(Name = "Opis leczenia")]
        public string Description { get; set; }

        [Display(Name = "Zalecenia")]
        public string? Recommendations { get; set; }

        [Display(Name = "Leki")]
        public string? Medicines { get; set; }
    }
}