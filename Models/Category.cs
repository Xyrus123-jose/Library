using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Category
    {

        [Key]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }
    }
}
