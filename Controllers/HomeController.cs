using Helth_And_Nutrition.Areas.Category.Models;
using Helth_And_Nutrition.Areas.Items.Models;
using Helth_And_Nutrition.Areas.SubCategory.Models;
using Helth_And_Nutrition.Models;
using Helth_And_Nutrition.Service;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Helth_And_Nutrition.Controllers
{
    public class HomeController : Controller
    {
        #region context
        private readonly DataContext _context;
        private readonly IMongoDatabase _database;

        public HomeController(DataContext context, IMongoClient mongoClient)
        {
            _context = context;
            _database = mongoClient.GetDatabase("Helth_And_Nutrition");
        }
        #endregion
        public IActionResult Index()
        {
            List<CategoryModel> categoryModels = _context.Category.ToList();
            return View(categoryModels);
        }
        public IActionResult SubCategoryPage(int categoryID)
        {
            List<SubCategoryModel> subCategoryModels = _context.SubCategory
                .Where(subcategory => subcategory.CategoryID == categoryID).ToList();
            var categoryName = _context.Category
                              .Where(c => c.CategoryID == categoryID)
                              .Select(c => c.CategoryName)
                              .FirstOrDefault();
            var Image = _context.Category
                              .Where(c => c.CategoryID == categoryID)
                              .Select(c => c.ImageURL)
                              .FirstOrDefault();
            ViewBag.CategoryName = categoryName;
            ViewBag.Image = Uri.EscapeUriString("Category/" + Image);
            return View(subCategoryModels);
        }
        public IActionResult ItemsPage(int subCategoryID)
        {
            List<ItemModel> itemModels = _context.Item.Where(item => item.SubCategoryID == subCategoryID).ToList();
            var subCategoryName = _context.SubCategory
                  .Where(c => c.SubCategoryID == subCategoryID)
                  .Select(c => c.SubCategoryName)
                  .FirstOrDefault();
            var categoryName = _context.Category
                  .Where(c => c.CategoryID == itemModels[0].CategoryID)
                  .Select(c => c.CategoryName)
                  .FirstOrDefault();
            var Image = _context.SubCategory
                              .Where(c => c.SubCategoryID == subCategoryID)
                              .Select(c => c.ImageURL)
                              .FirstOrDefault();
            ViewBag.SubCategoryName = subCategoryName;
            ViewBag.CategoryName = categoryName;
            ViewBag.Image = Uri.EscapeUriString("SubCategory/" + Image);
            return View(itemModels);
        }

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
                vitaminAndMineralsModel.DV = detail.Value.Split("|")[1].Replace("%", "");
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
        public IActionResult DetailPage(int itemID)
        {
            List<ItemModel> itemModels = _context.Item.Where(item => item.ItemID == itemID).ToList();
            ItemModel item = new ItemModel();
            item = itemModels[0];

            int targetItemId = itemID;
            List<BsonDocument> result = new List<BsonDocument>();

            // Vitamins
            string collectionName = "Vitamins";
            result = FindVitaminsOrMinerales(itemID, collectionName);
            if (result.Count() > 0)
            {
                var detailsList = ProcessJson(result.ToJson().ToString());

                List<VitaminAndMineralsModel> vitaminAndMineralsModels = fillVitaminAndMineralsModels(detailsList);
                ViewBag.VitaminsList = vitaminAndMineralsModels;
            }
            //Minerals
            collectionName = "Minerals";
            result = FindVitaminsOrMinerales(itemID, collectionName);
            if (result.Count() > 0)
            {
                var detailsList = ProcessJson(result.ToJson().ToString());

                List<VitaminAndMineralsModel> vitaminAndMineralsModels = fillVitaminAndMineralsModels(detailsList);
                ViewBag.MineralsList = vitaminAndMineralsModels;
            }
            // NutritionalInfo
            collectionName = "NutritionalInfo";
            result = FindVitaminsOrMinerales(itemID, collectionName);
            if (result.Count() > 0)
            {
                var detailsList = ProcessJson(result.ToJson().ToString());

                List<NutritionalInfoModel> nutritionalInfoModels = fillNutritionalInfoModels(detailsList);
                ViewBag.NutritionalInfoList = nutritionalInfoModels;
            }

            // NutritionalInfo
            collectionName = "Pros&Cons";
            result = FindVitaminsOrMinerales(itemID, collectionName);
            if (result.Count() > 0)
            {
                var detailsList = ProcessJson(result.ToJson().ToString());

                List<NutritionalInfoModel> nutritionalInfoModels = fillNutritionalInfoModels(detailsList);
                ViewBag.ProsConsList = nutritionalInfoModels;
            }
            return View(item);
        }
    }
}
