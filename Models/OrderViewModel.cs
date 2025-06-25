namespace PunktWeterynaryjny.Models
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; } = new();
		public string? ReturnReason { get; set; }
		public string? ClientEmail { get; set; }
	}
}