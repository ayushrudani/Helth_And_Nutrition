using Helth_And_Nutrition.Areas.Category.Models;
using Helth_And_Nutrition.Areas.Items.Models;
using Helth_And_Nutrition.Areas.SubCategory.Models;
using Helth_And_Nutrition.Models;
using Helth_And_Nutrition.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using OpenAI.API;
using OpenAI.API.Completions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Helth_And_Nutrition.Areas.Items.Controllers
{
	[Area("Items")]
	[Route("Items/{controller}/{action}")]
	public class ItemsController : Controller
	{
		#region context
		private readonly DataContext _context;
		private readonly IMongoDatabase _database;
		private readonly OpenAIAPI _openAIAPI;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ItemsController(IMongoClient mongoClient, DataContext context, OpenAIAPI openAIAPI, IWebHostEnvironment webHostEnvironment)
		{
			_database = mongoClient.GetDatabase("Helth_And_Nutrition");
			_context = context;
			_openAIAPI = openAIAPI;
			_webHostEnvironment = webHostEnvironment;
		}



		#endregion

		#region ItemList
		public IActionResult ItemsList()
		{
			List<ItemModel> ItemsList;
			var query = from items in _context.Item
						join category in _context.Category on items.CategoryID equals category.CategoryID
						join subCategory in _context.SubCategory on items.SubCategoryID equals subCategory.SubCategoryID
						#region ItemModel
						select new ItemModel
						{
							ItemID = items.ItemID,
							ItemName = items.ItemName,
							CategoryName = category.CategoryName,
							SubCategoryName = subCategory.SubCategoryName,
							Created = items.Created,
							Modified = items.Modified,
							CategoryID = items.CategoryID,
							SubCategoryID = items.SubCategoryID,
							ImageURL = items.ImageURL,
						};
			#endregion
			ItemsList = query.ToList();
			if (ItemsList.Count > 0)
			{
				return View(ItemsList);
			}
			else
			{
				ViewBag.Items = "Null";
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

		#region SetSubCategoryDropDown
		public void SetSubCategoryDropDown()
		{
			List<SubCategoryModel> subCategoryModelList;
			subCategoryModelList = _context.SubCategory.ToList();
			if (subCategoryModelList.Count > 0)
			{
				List<SubCategoryDropDown> subCategoryDropDownList = new List<SubCategoryDropDown>();
				foreach (var item in subCategoryModelList)
				{
					SubCategoryDropDown subCategoryDropDown = new SubCategoryDropDown();
					subCategoryDropDown.SubCategoryID = Convert.ToInt32(item.SubCategoryID);
					subCategoryDropDown.SubCategoryName = item.SubCategoryName;
					subCategoryDropDownList.Add(subCategoryDropDown);
				}
				ViewBag.SubCategoryDropDownList = subCategoryDropDownList;
			}
		}
		#endregion

		#region ItemsAddEdit
		public IActionResult ItemsAddEdit()
		{
			Console.WriteLine("Normal Item Call");
			SetCategoryDropDown();
			SetSubCategoryDropDown();
			return View();
		}
		#endregion

		#region ItemsAddEdit
		[HttpPost]
		public IActionResult ItemsAddEdit(ItemModel itemModel)
		{
			var tableName = "Item";
			if (ModelState.IsValid)
			{

				#region SetImageURL
				if (itemModel.File != null && itemModel.File.Length > 0)
				{
					// Your existing logic for image cropping
					using (var stream = itemModel.File.OpenReadStream())
					{
						using (var image = Image.Load(stream))
						{
							// Crop the image to 500x570 pixels
							image.Mutate(x => x
								.Resize(new ResizeOptions
								{
									Size = new Size(500, 570),
									Mode = ResizeMode.Crop
								}));

							// Save the cropped image to a folder
							var uploadsFolder = Path.Combine(Environment.CurrentDirectory, "wwwroot", "Images", "Items");
							var imageName = itemModel.ItemName + "." + itemModel.File.ContentType.Split('/')[1]; // Adjust the file name generation as needed
							var imagePath = Path.Combine(uploadsFolder, imageName);

							// Ensure the uploads folder exists
							if (!Directory.Exists(uploadsFolder))
								Directory.CreateDirectory(uploadsFolder);

							// Save the image to the uploads folder
							image.Save(imagePath, new JpegEncoder());

							// Update ItemModel properties
							itemModel.ImageURL = imageName; // Assuming you want to store the file name, adjust as needed
						}
					}
				}
				else if (itemModel.ItemID == null)
				{
					SetCategoryDropDown();
					SetSubCategoryDropDown();
					return View();
				}
				#endregion


				itemModel.Modified = DateTime.Now;
				if (itemModel.ItemID != null)
				{
					#region UpdateItems
					try
					{
						var sql = "UPDATE [Item] SET [ItemName] = {0}, [ImageURL] = {1}, [Modified] = {2},[CategoryID] = {3},[SubCategoryID] = {4} WHERE [ItemID] = {5}";
						#region ItemModelParameter
						var parameters = new object[]
										{
				itemModel.ItemName,
				itemModel.ImageURL,
				itemModel.Modified,
				itemModel.CategoryID,
				itemModel.SubCategoryID,
				itemModel.ItemID
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
					itemModel.Created = DateTime.Now;

					#region InsertItemsSQL
					try
					{
						var sql = "INSERT INTO [Item] ([CategoryID], [SubCategoryID], [ItemName], [Created], [ImageURL], [Modified]) VALUES ({0}, {1}, {2}, {3}, {4}, {5})";
						#region ItemModelParameter
						var parameters = new object[]
										{
				itemModel.CategoryID,
				itemModel.SubCategoryID,
				itemModel.ItemName,
				itemModel.Created,
				itemModel.ImageURL,
				itemModel.Modified
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
				return RedirectToAction("ItemsList");
			}
			else
			{
				SetCategoryDropDown();
				SetSubCategoryDropDown();
				return View(itemModel);
			}

		}
		#endregion

		#region ItemsEdit
		public IActionResult ItemsEdit(int id)
		{
			ItemModel itemModel = _context.Item.Find(id);
			if (itemModel == null)
			{
				TempData["Message"] = "Something Wrong Plese Try Again!!!!";
				return RedirectToAction("ItemList");
			}

			ViewBag.ID = id;
			SetCategoryDropDown();
			SetSubCategoryDropDown();
			return View(itemModel);

		}
		#endregion

		#region ItemsDelete
		public IActionResult ItemsDelete(int id)
		{
			ItemModel itemModel = _context.Item.Find(id);
			if (itemModel == null)
			{
				TempData["Message"] = "Something Wrong Please Try Again!!!!";
				return RedirectToAction("ItemList");
			}
			try
			{
				var sql = "DELETE FROM [Item] WHERE [ItemID] = {0}";
				_context.Database.ExecuteSqlRaw(sql, id);
				TempData["Message"] = "Record Deleted Successfully...";
			}
			catch (Exception ex)
			{
				var tableName = "Item";
				var operation = "Delete";
				_context.Database.ExecuteSqlRaw("EXEC LogAuditInfo @p0, @p1, @p2", tableName, operation, 0);
				TempData["Message"] = ex.Message;
			}
			return RedirectToAction("ItemsList");
		}
		#endregion

		#region SubCategoryDropDownByCategoryId
		public IActionResult SubCategoryDropDownByCategoryId(int CategoryId)
		{
			List<SubCategoryDropDown> subCategoryDropDowns = new List<SubCategoryDropDown>();
			var Data = _context.SubCategory
				.Where(sc => sc.CategoryID == CategoryId)
				.Select(sc => new { sc.SubCategoryID, sc.SubCategoryName })
				.ToList();
			foreach (var data in Data)
			{
				SubCategoryDropDown subCategoryDropDown = new SubCategoryDropDown();
				subCategoryDropDown.SubCategoryID = Convert.ToInt32(data.SubCategoryID);
				subCategoryDropDown.SubCategoryName = data.SubCategoryName.ToString();
				subCategoryDropDowns.Add(subCategoryDropDown);
			}
			var vModel = subCategoryDropDowns;
			return Json(vModel);
		}
		#endregion

		#region CategoryDropDownBySubCategoryId
		public IActionResult CategoryDropDownBySubCategoryId(int SubCategoryId)
		{
			CategoryDropDown categoryDropDown = new CategoryDropDown();
			var ForId = _context.SubCategory
				.Where(sc => sc.SubCategoryID == SubCategoryId)
				.Select(sc => new { sc.CategoryID })
				.ToList();
			int CategoryId = (int)ForId[0].CategoryID;
			var Data = _context.Category
				.Where(sc => sc.CategoryID == CategoryId)
				.Select(sc => new { sc.CategoryID, sc.CategoryName })
				.ToList();
			categoryDropDown.CategoryID = (int)Data[0].CategoryID;
			categoryDropDown.CategoryName = Data[0].CategoryName;
			var vModel = categoryDropDown;
			return Json(vModel);
		}
		#endregion

		#region ProcessJson
		public static List<KeyValuePair<string, string>> ProcessJson(string jsonData)
		{
			// Replace ObjectId(...) with an empty string
			jsonData = jsonData.Replace("ObjectId(", "").Replace(")", "");

			// Parse the modified JSON array
			JArray jsonArray = JArray.Parse(jsonData);

			// Create a list to store key-value pairs
			List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();

			// Iterate over each object in the array
			foreach (var jsonItem in jsonArray.Children<JObject>())
			{
				// Extract keys and values
				foreach (var property in jsonItem.Properties())
				{
					// Add key-value pair to the list
					keyValuePairs.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
				}
			}

			return keyValuePairs;
		}
		#endregion

		#region FindVitaminsOrMinerales
		public List<BsonDocument> FindVitaminsOrMinerales(int itemID, string collectionName)
		{
			var collectionVitamins = _database.GetCollection<BsonDocument>(collectionName);


			var filterVitamin = Builders<BsonDocument>.Filter.Eq("ItemId", itemID);

			// Perform the find operation
			return collectionVitamins.Find(filterVitamin).ToList();
		}
		#endregion

		#region fillVitaminAndMineralsModels
		public List<VitaminAndMineralsModel> fillVitaminAndMineralsModels(dynamic detailsList)
		{
			List<VitaminAndMineralsModel> vitaminAndMineralsModels = new List<VitaminAndMineralsModel>();

			foreach (var detail in detailsList)
			{
				//Console.WriteLine($"Key: {detail.Key}, Value: {detail.Value}");
				if (detail.Key.Equals("Name") || detail.Key.Equals("_id") || detail.Key.Equals("__RequestVerificationToken") || detail.Key.Equals("ItemId"))
				{
					continue;
				}
				VitaminAndMineralsModel vitaminAndMineralsModel = new VitaminAndMineralsModel();
				vitaminAndMineralsModel.Name = detail.Key;
				vitaminAndMineralsModel.Value = detail.Value.Split("|")[0];
				vitaminAndMineralsModel.DV = detail.Value.Split("|")[1];
				vitaminAndMineralsModels.Add(vitaminAndMineralsModel);
			}
			return vitaminAndMineralsModels;
		}
		#endregion

		#region fillNutritionalInfoModels
		public List<NutritionalInfoModel> fillNutritionalInfoModels(dynamic detailsList)
		{
			List<NutritionalInfoModel> nutritionalInfoModels = new List<NutritionalInfoModel>();

			foreach (var detail in detailsList)
			{
				//Console.WriteLine($"Key: {detail.Key}, Value: {detail.Value}");
				if (detail.Key.Equals("Name") || detail.Key.Equals("_id") || detail.Key.Equals("__RequestVerificationToken") || detail.Key.Equals("ItemId"))
				{
					continue;
				}
				NutritionalInfoModel nutritionalInfoModel = new NutritionalInfoModel();
				nutritionalInfoModel.Name = detail.Key;
				nutritionalInfoModel.Value = detail.Value;
				nutritionalInfoModels.Add(nutritionalInfoModel);
			}
			return nutritionalInfoModels;
		}
		#endregion

		#region VitaminsAndMineralsPage
		public IActionResult VitaminsAndMineralsPage(int itemID, string itemName, string collectionName, string errorMessage = null)
		{

			ViewBag.ItemName = itemName;
			ViewBag.ItemID = itemID;
			ViewBag.CollectionName = collectionName;
			ViewBag.ErrorMessage = errorMessage;
			List<BsonDocument> result = FindVitaminsOrMinerales(itemID, collectionName);
			if (result.Count() > 0)
			{
				var detailsList = ProcessJson(result.ToJson().ToString());
				if (collectionName == "NutritionalInfo" || collectionName == "Pros&Cons")
				{
					List<NutritionalInfoModel> nutritionalInfoModels = fillNutritionalInfoModels(detailsList);
					ViewBag.List = nutritionalInfoModels;
				}
				else
				{
					List<VitaminAndMineralsModel> vitaminAndMineralsModels = fillVitaminAndMineralsModels(detailsList);
					ViewBag.List = vitaminAndMineralsModels;
				}
				ViewBag.ListIsNull = false;
				ViewBag.Optration = "Edit";
			}
			else
			{
				ViewBag.ListIsNull = true;
			}
			return View();
		}
		#endregion

		#region GenerateVitaminsAndMinerals
		public IActionResult GenerateVitaminsAndMinerals(int itemID, string itemName, string collectionName)
		{
			string Name = itemName;
			ViewBag.ItemID = itemID;
			ViewBag.CollectionName = collectionName;
			string CollectionName = collectionName;
			//string apiKey = "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";
			CompletionRequest completion = new CompletionRequest();
			ItemModel itemModel = new ItemModel();
			try
			{
				string answer = string.Empty;
				//completion.Prompt = "Act As nutrition boat and give me a Calorie of " + Name + "per 100 grams in number only. do not use any kind of string there give only numeruic value for Example input: give me a Calorie of potato, Examplpe Output:110";
				//completion.Prompt = "Act As nutrition boat and give me a pipe(|) seprated Fast facts,Calorie and Protein per 100 grams of" + Name + ". in proper JASON format output example:{\"Description\":\"1. It has 95% water, making it an excellent way to stay hydrated.|2. It is high in vitamin C and other antioxidants.\",\"Calorie\":110,\"Protein\":\"2.57g\"}";
				if (collectionName == "Vitamins")
				{
					completion.Prompt = "[{\r\n  \"Name\": \"Name\",\r\n  \"Vitamin A\": \"64.6 IU | 1%\", Vitamin \":\"60.9 mg | 101%\"\r\n}]\r\n Act As nutrition boat and give me more correct information like this for" + Name + "also give info about only vitamins which is available for it. give only json format like this not anything else.";
				}
				else if (collectionName == "Minerals")
				{
					completion.Prompt = "[{\r\n  \"Name\": \"Name\",\r\n  \"Calcium\":\"6 mg | 1%\",\r\n\"Iron\":\"0.3 mg | 2%\"\r\n}]\r\n Act As nutrition boat and give me more information like this for" + Name + "also give info about only Minerals which is available for it. give only json format like this not anything else.";
				}
				else if (collectionName == "NutritionalInfo")
				{
					completion.Prompt = "[{\r\n  \"Name\": \"Name\",\r\n \"Energy\":\"34.2 cal\",\r\n\"Carbs\":\"7.8 g\"\r\n}]\r\n Act As nutrition boat and give me more information like this for" + Name + "also give info about only Nutritional Information which is available for it.give only json format like this not anything else.";
				}
				else if (collectionName == "Pros&Cons")
				{
					completion.Prompt = "[{\r\n  \"Name\": \"Name\",\r\n \"Pros1\":\"pros1 sentence\",\r\n\"Pros2\":\"pros2 sentence\",\r\n\"Cons1\":\"cons1 sentence\",\r\n\"Cons2\":\"cons2 sentence\"\r\n}]\r\n Act As nutrition boat and give me more pros and cons like this for" + Name + ". give me at least 3 pros and cons. if more available than give it. give only json format like this not anything else.";
				}
				completion.Model = OpenAI_API.Models.Model.DavinciText;
				completion.MaxTokens = 400;
				var result = _openAIAPI.Completions.CreateCompletionAsync(completion);
				if (result != null)
				{
					foreach (var item in result.Result.Completions)
					{
						answer = item.Text;
					}
				}
				answer = answer.Replace("</code>", "");
				JArray jsonArray = JArray.Parse(answer);
				List<BsonDocument> bsonDocumentList = new List<BsonDocument>();
				foreach (JToken token in jsonArray)
				{
					string jsonDocument = token.ToString();
					BsonDocument bsonDocument = BsonDocument.Parse(jsonDocument);
					bsonDocumentList.Add(bsonDocument);
				}
				var detailsList = ProcessJson(bsonDocumentList.ToJson().ToString());
				if (collectionName == "NutritionalInfo" || collectionName == "Pros&Cons")
				{
					List<NutritionalInfoModel> nutritionalInfoModels = fillNutritionalInfoModels(detailsList);
					ViewBag.List = nutritionalInfoModels;
				}
				else
				{
					List<VitaminAndMineralsModel> vitaminAndMineralsModels = fillVitaminAndMineralsModels(detailsList);
					ViewBag.List = vitaminAndMineralsModels;
				}
				ViewBag.IsNull = false;
				Console.WriteLine("Done");

			}
			catch (Exception ex)
			{
				ViewBag.IsNull = true;
				ViewBag.ItemID = itemID;
				ViewBag.CollectionName = collectionName;
				return Redirect($"/Items/Items/VitaminsAndMineralsPage?itemName={itemName}&itemID={itemID}&collectionName={collectionName}&errorMessage=Yes");
			}
			return View("VitaminsAndMineralsPage");
		}
		#endregion

		#region ProcessForm
		[HttpPost]
		public IActionResult ProcessForm(Dictionary<string, string> formData)
		{
			var document = new BsonDocument();
			int itemID = 0;
			string collectionName = "";

			foreach (var kvp in formData)
			{
				if (kvp.Key == "ItemId")
				{
					itemID = Convert.ToInt32(kvp.Value);
					document.Add(new BsonElement(kvp.Key, BsonValue.Create(Convert.ToInt32(kvp.Value))));
					continue;
				}
				else if (kvp.Key == "CollectionName")
				{
					collectionName = kvp.Value;
					continue;
				}
				document.Add(new BsonElement(kvp.Key, BsonValue.Create(kvp.Value)));
			}
			List<BsonDocument> result = FindVitaminsOrMinerales(itemID, collectionName);
			var collection = _database.GetCollection<BsonDocument>(collectionName);
			if (result.Count > 0)
			{
				var filter = Builders<BsonDocument>.Filter.Eq("ItemId", itemID);
				collection.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
			}
			else
			{
				collection.InsertOne(document);
			}
			return Redirect($"/Items/Items/ItemsEdit?id={itemID}");
		}
		#endregion
	}
}
