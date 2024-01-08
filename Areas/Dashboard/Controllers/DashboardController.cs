using Helth_And_Nutrition.Areas.Dashboard.Models;
using Helth_And_Nutrition.Service;
using Microsoft.AspNetCore.Mvc;

namespace Helth_And_Nutrition.Areas.Dashboard.Controllers
{

    [Area("Dashboard")]
	[Route("Dashboard/{controller}/{action}")]
	public class DashboardController : Controller
	{
		private readonly DataContext _context;

		public DashboardController(DataContext context)
		{
			_context = context;
		}
		public IActionResult Dashboard()
		{
			ViewBag.CategoryCount = _context.Category.ToList().Count;
			ViewBag.SubCategoryCount = _context.SubCategory.ToList().Count;
			ViewBag.ItemsCount = _context.Item.ToList().Count;
			ViewBag.UserCount = _context.User.ToList().Count;
			List<AuditLog> auditLogs = _context.AuditLog.OrderByDescending(e => e.AuditLogId).Take(10).ToList();
			ViewBag.IsSeeAll = false;
			return View(auditLogs);
		}

		public IActionResult SeeAllAuditLogs()
		{
			List<AuditLog> auditLogs = _context.AuditLog.ToList();
			ViewBag.IsSeeAll = true;
			return View("Dashboard", auditLogs);
		}
	}
}
