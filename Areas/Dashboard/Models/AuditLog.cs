using System.ComponentModel.DataAnnotations;

namespace Helth_And_Nutrition.Areas.Dashboard.Models
{
	public class AuditLog
	{
		[Key]
		public int AuditLogId { get; set; }

		public string TableName { get; set; }

		public string Operation { get; set; }

		public DateTime OperationTime { get; set; }

		public bool IsDone { get; set; }

	}
}
