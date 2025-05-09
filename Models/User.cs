using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [MaxLength(250)]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MaxLength(100)]
        public string Password { get; set; }

        [MaxLength(250)]
        public string ImagePath { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; }
    }
}
