using Helth_And_Nutrition.Areas.Category.Models;
using Helth_And_Nutrition.Areas.Dashboard.Models;
using Helth_And_Nutrition.Areas.Items.Models;
using Helth_And_Nutrition.Areas.SubCategory.Models;
using Helth_And_Nutrition.Models;
using Microsoft.EntityFrameworkCore;

namespace Helth_And_Nutrition.Service
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions options) : base(options)
		{ }
		public DbSet<CategoryModel> Category { get; set; }
		public DbSet<SubCategoryModel> SubCategory { get; set; }
		public DbSet<ItemModel> Item { get; set; }
		public DbSet<UserModel> User { get; set; }
		public DbSet<AuditLog> AuditLog { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SubCategoryModel>()
				.Property(s => s.SubCategoryID)
				.ValueGeneratedOnAdd();
		}

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//    optionsBuilder.UseSqlServer("");
		//}
	}
}
