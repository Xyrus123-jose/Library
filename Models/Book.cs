using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Book
    {

        [Key]
        public int BookID { get; set; }

        [Required]
        [MaxLength(200)]
        public string BookTitle { get; set; }

        [Required]
        [MaxLength(150)]
        public string AuthorName { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryID { get; set; }

        public Category Category { get; set; } // Navigation property

        [MaxLength(250)]
        public string ImagePath { get; set; }

     
    }
}
