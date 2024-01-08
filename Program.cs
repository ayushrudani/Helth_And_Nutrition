using Helth_And_Nutrition.Service;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using OpenAI.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("MyConString")));

builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://localhost:27017"));

var apiKey = "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";
builder.Services.AddSingleton(new OpenAIAPI(apiKey));




var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");

	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

	endpoints.MapControllerRoute(
		name: "area",
		pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
});
app.Run();
