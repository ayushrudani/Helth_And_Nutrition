using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helth_And_Nutrition.Areas.Items.Models;
using Helth_And_Nutrition.Areas.SubCategory.Models;

namespace Helth_And_Nutrition.Areas.Category.Models
{
	public class CategoryModel
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int? CategoryID { get; set; }

		[Required(ErrorMessage = "Enter Category Name")]
		public string CategoryName { get; set; }

		[Required(ErrorMessage = "Enter Category Code")]
		public string CategoryCode { get; set; }

		public string? ImageURL { get; set; }

		public DateTime? Created { get; set; } = null;

		public DateTime? Modified { get; set; }

		// Navigation Property
		public virtual ICollection<SubCategoryModel>? SubCategories { get; set; }

		// Navigation Property
		public virtual ICollection<ItemModel>? Items { get; set; }

		[NotMapped]
		public IFormFile? File { get; set; }
	}
	public class CategoryDropDown
	{
		[NotMapped]
		public int CategoryID { get; set; }

		[NotMapped]
		public string CategoryName { get; set; }
	}
}
