using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PunktWeterynaryjny.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();

        [Required(ErrorMessage = "Adres wysyłki jest wymagany.")]
        [Display(Name = "Adres wysyłki")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Display(Name = "Telefon kontaktowy")]
        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
        public string ContactPhone { get; set; } = string.Empty;

		[Required(ErrorMessage = "Wybierz metodę płatności.")]
		[Display(Name = "Metoda płatności")]
		public PaymentMethod SelectedPaymentMethod { get; set; }

	}
}
