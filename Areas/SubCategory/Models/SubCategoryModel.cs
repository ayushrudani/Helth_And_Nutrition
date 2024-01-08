using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helth_And_Nutrition.Areas.Category.Models;
using Helth_And_Nutrition.Areas.Items.Models;

namespace Helth_And_Nutrition.Areas.SubCategory.Models
{
	public class SubCategoryModel
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int? SubCategoryID { get; set; }

		[Required]
		[MaxLength(100)]
		public string SubCategoryName { get; set; }

		[Required]
		[MaxLength(100)]
		public string SubCategoryCode { get; set; }

		public string? ImageURL { get; set; }

		public DateTime? Created { get; set; }

		public DateTime? Modified { get; set; }
		public int? CategoryID { get; set; }

		[ForeignKey("CategoryID")]
		public virtual CategoryModel? Category { get; set; }

		public virtual ICollection<ItemModel>? Items { get; set; }
		[NotMapped]
		public IFormFile? File { get; set; }
		[NotMapped]
		public string? CategoryName { get; set; }
	}
	public class SubCategoryDropDown
	{
		[NotMapped]
		public int SubCategoryID { get; set; }

		[NotMapped]
		public string SubCategoryName { get; set; }
	}
}
