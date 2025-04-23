using System.ComponentModel.DataAnnotations;

namespace PunktWeterynaryjny.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nazwa leku")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Opis")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cena")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}