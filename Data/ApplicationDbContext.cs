using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PunktWeterynaryjny.Models;

namespace PunktWeterynaryjny.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Visit> Visits { get; set; }  
        public DbSet<TreatmentPlan> TreatmentPlans { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Product>().HasData(
				new Product { Id = 1, Name = "Lek przeciwbólowy", Description = "Skuteczny lek na ból", Price = 19.99m },
				new Product { Id = 2, Name = "Witamina dla psa", Description = "Suplement witaminowy", Price = 29.50m },
				new Product { Id = 3, Name = "Preparat na pch³y", Description = "Ochrona przed pch³ami", Price = 49.00m }
			);
		}
	}
}