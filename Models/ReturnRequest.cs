using PunktWeterynaryjny.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class ReturnRequest
{
	public int Id { get; set; }

	[Required]
	public int OrderId { get; set; }
	public Order Order { get; set; }

	[Required]
	public string UserId { get; set; }

	[Required]
	[StringLength(500)]
	public string Reason { get; set; }

	public DateTime SubmittedAt { get; set; } = DateTime.Now;

	public ReturnStatus Status { get; set; } = ReturnStatus.Oczekuje;
}

public enum ReturnStatus
{
	Oczekuje,
	Zaakceptowany,
	Odrzucony
}
