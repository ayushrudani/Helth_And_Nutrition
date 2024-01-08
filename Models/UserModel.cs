using System.ComponentModel.DataAnnotations;

namespace Helth_And_Nutrition.Models
{
    public class UserModel
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(10)]
        public string PhoneNumber { get; set; }
    }
}
