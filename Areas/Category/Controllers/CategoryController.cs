using Helth_And_Nutrition.Areas.Category.Models;
using Helth_And_Nutrition.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helth_And_Nutrition.Areas.Category.Controllers
{
	[Area("Category")]
	[Route("Category/{controller}/{action}")]
	public class CategoryController : Controller
	{
		#region context
		private readonly DataContext _context;

		public CategoryController(DataContext context)
		{
			_context = context;
		}
		#endregion

		#region CategoryList
		public IActionResult CategoryList()
		{
			List<CategoryModel> CategoryList;
			CategoryList = _context.Category.ToList();
			if (CategoryList.Count > 0)
			{
				return View(CategoryList);
			}
			else
			{
				ViewBag.Category = "Null";
				return View();
			}
		}
		#endregion

		#region CategoryAddEdit
		public IActionResult CategoryAddEdit()
		{
			return View();
		}
		#endregion

		[HttpPost]
		#region CategoryAddEdit
		public IActionResult CategoryAddEdit(CategoryModel categoryModel)
		{
			var tableName = "Category";
			if (ModelState.IsValid)
			{
				#region SetImageURL
				if (categoryModel.File != null && categoryModel.File.Length > 0)
				{
					var path = Path.Combine(Environment.CurrentDirectory, "wwwroot", "Images", "Category", categoryModel.CategoryName + "." + categoryModel.File.ContentType.Split('/')[1]);
					using (FileStream stream = new FileStream(path, FileMode.Create))
					{
						categoryModel.File.CopyTo(stream);
					}
					categoryModel.ImageURL = categoryModel.CategoryName + "." + categoryModel.File.ContentType.Split('/')[1];
				}
				else if (categoryModel.CategoryID == null)
				{

					return View();
				}
				#endregion

				categoryModel.Modified = DateTime.Now;
				if (categoryModel.CategoryID != null)
				{
					#region UpdateCategory
					try
					{
						var sql = "UPDATE [Category] SET [CategoryName] = {0}, [ImageURL] = {1},[CategoryCode] = {2}, [Modified] = {3} WHERE [CategoryID] = {4}";
						#region CategoryModelParameter
						var parameters = new object[]
										{
							categoryModel.CategoryName,
							categoryModel.ImageURL,
							categoryModel.CategoryCode,
							categoryModel.Modified,
							categoryModel.CategoryID
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
					categoryModel.Created = DateTime.Now;
					#region InsertCategory
					try
					{
						var sql = "INSERT INTO [Category] ([CategoryCode], [CategoryName], [Created], [ImageURL], [Modified]) VALUES ({0}, {1}, {2}, {3}, {4})";
						#region CategoryModelParameter
						var parameters = new object[]
										{
							categoryModel.CategoryCode,
							categoryModel.CategoryName,
							categoryModel.Created,
							categoryModel.ImageURL,
							categoryModel.Modified
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
				return RedirectToAction("CategoryList");
			}
			else
			{
				return View();
			}
		}
		#endregion

		#region CategoryEdit
		public IActionResult CategoryEdit(int id)
		{
			CategoryModel categoryModel = _context.Category.Find(id);
			if (categoryModel == null)
			{
				TempData["Message"] = "Something Wrong Plese Try Again!!!!";
				return RedirectToAction("CategoryList");
			}

			ViewBag.ID = id;
			return View("CategoryAddEdit", categoryModel);

		}
		#endregion

		#region CategoryDelete
		public IActionResult CategoryDelete(int id)
		{
			CategoryModel categoryModel = _context.Category.Find(id);
			if (categoryModel == null)
			{
				TempData["Message"] = "Something Wrong Plese Try Again!!!!";
				return RedirectToAction("CategoryList");
			}

			try
			{
				var sql = "DELETE FROM [Category] WHERE [CategoryID] = {0}";
				_context.Database.ExecuteSqlRaw(sql, id);
				TempData["Message"] = "Record Deleted Successfully...";
			}
			catch (Exception ex)
			{
				var tableName = "Category";
				var operation = "Delete";
				_context.Database.ExecuteSqlRaw("EXEC LogAuditInfo @p0, @p1, @p2", tableName, operation, 0);
				TempData["Message"] = ex.Message;
			}
			return RedirectToAction("CategoryList");
		}
		#endregion

	}
}
