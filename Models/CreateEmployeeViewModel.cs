using System.ComponentModel.DataAnnotations;

namespace PunktWeterynaryjny.Models
{
	public class CreateEmployeeViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Compare("Password")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}
}
