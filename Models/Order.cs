using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PunktWeterynaryjny.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Przyjęte;

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string ContactPhone { get; set; } = string.Empty;

        public List<OrderItem> OrderItems { get; set; } = new();

		public PaymentMethod PaymentMethod { get; set; }

	}
}