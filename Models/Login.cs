using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Login
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
