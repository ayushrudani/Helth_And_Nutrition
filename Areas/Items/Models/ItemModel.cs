using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helth_And_Nutrition.Areas.Category.Models;
using Helth_And_Nutrition.Areas.SubCategory.Models;
using Microsoft.EntityFrameworkCore;

namespace Helth_And_Nutrition.Areas.Items.Models
{
	public class ItemModel
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int? ItemID { get; set; }

		[Required]
		[MaxLength(100)]
		public string ItemName { get; set; }


		public string? ImageURL { get; set; }


		public DateTime? Created { get; set; }


		public DateTime? Modified { get; set; }

		[NotMapped]
		public string? CategoryName { get; set; }

		[NotMapped]
		public string? SubCategoryName { get; set; }

		public int? SubCategoryID { get; set; }

		public int? CategoryID { get; set; }

		[ForeignKey("SubCategoryID")]
		[InverseProperty("Items")]
		[DeleteBehavior(DeleteBehavior.NoAction)]
		public virtual SubCategoryModel? SubCategory { get; set; }

		[ForeignKey("CategoryID")]
		[InverseProperty("Items")]
		[DeleteBehavior(DeleteBehavior.Cascade)]
		public virtual CategoryModel? Category { get; set; }

		[NotMapped]
		[Required(ErrorMessage = "Select File")]
		public IFormFile? File { get; set; }
	}
}
