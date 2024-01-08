using Helth_And_Nutrition.Areas.Category.Models;
using Helth_And_Nutrition.Areas.SubCategory.Models;
using Helth_And_Nutrition.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helth_And_Nutrition.Areas.SubCategory.Controllers
{
	[Area("SubCategory")]
	[Route("SubCategory/{controller}/{action}")]
	public class SubCategoryController : Controller
	{
		#region context
		private readonly DataContext _context;

		public SubCategoryController(DataContext context)
		{
			_context = context;
		}
		#endregion

		#region SubCategoryList
		public IActionResult SubCategoryList()
		{
			List<SubCategoryModel> SubCategoryList;
			var query = from subCategory in _context.SubCategory
						join category in _context.Category on subCategory.CategoryID equals category.CategoryID
						#region SubCategoryModel
						select new SubCategoryModel
						{
							SubCategoryID = subCategory.SubCategoryID,
							SubCategoryName = subCategory.SubCategoryName,
							SubCategoryCode = subCategory.SubCategoryCode,
							Created = subCategory.Created,
							Modified = subCategory.Modified,
							CategoryID = subCategory.CategoryID,
							ImageURL = subCategory.ImageURL,
							CategoryName = category.CategoryName
						};
			#endregion
			SubCategoryList = query.ToList();
			if (SubCategoryList.Count > 0)
			{
				return View(SubCategoryList);
			}
			else
			{
				ViewBag.SubCategory = "Null";
				return View();
			}
		}
		#endregion

		#region SetCategoryDropDown
		public void SetCategoryDropDown()
		{
			List<CategoryModel> categoryModelList;
			categoryModelList = _context.Category.ToList();
			if (categoryModelList.Count > 0)
			{
				List<CategoryDropDown> categoryDropDownList = new List<CategoryDropDown>();
				foreach (var item in categoryModelList)
				{
					CategoryDropDown categoryDropDown = new CategoryDropDown();
					categoryDropDown.CategoryID = Convert.ToInt32(item.CategoryID);
					categoryDropDown.CategoryName = item.CategoryName;
					categoryDropDownList.Add(categoryDropDown);
				}
				ViewBag.CategoryDropDownList = categoryDropDownList;
			}
		}
		#endregion

		#region SubCategoryAddEdit
		public IActionResult SubCategoryAddEdit()
		{
			SetCategoryDropDown();
			return View();
		}
		#endregion

		[HttpPost]

		#region SubCategoryAddEdit
		public IActionResult SubCategoryAddEdit(SubCategoryModel subCategoryModel)
		{
			var tableName = "SubCategory";
			if (ModelState.IsValid)
			{
				#region SetImageURL
				if (subCategoryModel.File != null && subCategoryModel.File.Length > 0)
				{
					var path = Path.Combine(Environment.CurrentDirectory, "wwwroot", "Images", "SubCategory", subCategoryModel.SubCategoryName + "." + subCategoryModel.File.ContentType.Split('/')[1]);
					using (FileStream stream = new FileStream(path, FileMode.Create))
					{
						subCategoryModel.File.CopyTo(stream);
					}
					subCategoryModel.ImageURL = subCategoryModel.SubCategoryName + "." + subCategoryModel.File.ContentType.Split('/')[1];
				}
				else if (subCategoryModel.SubCategoryID == null)
				{

					return View();
				}
				#endregion

				subCategoryModel.Modified = DateTime.Now;

				if (subCategoryModel.SubCategoryID != null)
				{
					#region UpdateSubCategory
					try
					{
						var sql = "UPDATE [SubCategory] SET [SubCategoryName] = {0}, [ImageURL] = {1},[SubCategoryCode] = {2}, [Modified] = {3},[CategoryID] = {4} WHERE [SubCategoryID] = {5}";
						#region SubCategoryModelParameter
						var parameters = new object[]
										{
							subCategoryModel.SubCategoryName,
							subCategoryModel.ImageURL,
							subCategoryModel.SubCategoryCode,
							subCategoryModel.Modified,
							subCategoryModel.CategoryID,
							subCategoryModel.SubCategoryID
										};
						#endregion
						_context.Database.ExecuteSqlRaw(sql, parameters);
						TempData["Message"] = "Record Updated Successfully...";
					}
					catch (Exception ex)
					{
						var operation = "Update";
						_context.Database.ExecuteSqlRaw("EXEC LogAuditInfo @p0, @p1, @p2", tableName, operation, 0);
						TempData["Message"] = ex.Message;
					}
					#endregion

				}
				else
				{
					subCategoryModel.Created = DateTime.Now;
					#region InsertSubCategory
					try
					{
						var sql = "INSERT INTO [SubCategory] ([CategoryID], [SubCategoryCode], [SubCategoryName], [Created], [ImageURL], [Modified]) VALUES ({0}, {1}, {2}, {3}, {4}, {5})";

						#region SubCategoryModelParameter
						var parameters = new object[]
										{
							subCategoryModel.CategoryID,
							subCategoryModel.SubCategoryCode,
							subCategoryModel.SubCategoryName,
							subCategoryModel.Created,
							subCategoryModel.ImageURL,
							subCategoryModel.Modified
										};
						#endregion

						_context.Database.ExecuteSqlRaw(sql, parameters);
						TempData["Message"] = "Record Inserted Successfully...";
					}
					catch (Exception ex)
					{

						var operation = "Insert";
						_context.Database.ExecuteSqlRaw("EXEC LogAuditInfo @p0, @p1, @p2", tableName, operation, 0);
						TempData["Message"] = ex.Message;
					}
					#endregion
				}

				return RedirectToAction("SubCategoryList");
			}
			else
			{
				SetCategoryDropDown();
				return View();
			}
		}
		#endregion

		#region SubCategoryEdit
		public IActionResult SubCategoryEdit(int id)
		{
			SubCategoryModel subCategoryModel = _context.SubCategory.Find(id);
			if (subCategoryModel == null)
			{
				TempData["Message"] = "Something Wrong Plese Try Again!!!!";
				return RedirectToAction("SubCategoryList");
			}

			ViewBag.ID = id;
			SetCategoryDropDown();
			return View("SubCategoryAddEdit", subCategoryModel);

		}
		#endregion

		#region SubCategoryDelete
		public IActionResult SubCategoryDelete(int id)
		{
			SubCategoryModel subCategoryModel = _context.SubCategory.Find(id);
			if (subCategoryModel == null)
			{
				TempData["Message"] = "Something Wrong Please Try Again!!!!";
				return RedirectToAction("SubCategoryList");
			}

			try
			{
				var sql = "DELETE FROM [SubCategory] WHERE [SubCategoryID] = {0}";
				_context.Database.ExecuteSqlRaw(sql, id);
				TempData["Message"] = "Record Deleted Successfully...";
			}
			catch (Exception ex)
			{
				var tableName = "SubCategory";
				var operation = "Delete";
				_context.Database.ExecuteSqlRaw("EXEC LogAuditInfo @p0, @p1, @p2", tableName, operation, 0);
				TempData["Message"] = ex.Message;
			}
			return RedirectToAction("SubCategoryList");
		}
		#endregion
	}
}
