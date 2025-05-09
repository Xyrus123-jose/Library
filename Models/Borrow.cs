using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Borrow
    {

        [Key]
        public int Id { get; set; }

        // Foreign Key for Book
        [Required]
        public int BookID { get; set; }
        public Book Book { get; set; } // Navigation property

        // Foreign Key for Student
        [Required]
        public int UserID { get; set; }
        public User User { get; set; } // Navigation property

        // Borrow Date
        [Required]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; }

        // Return Date
        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

    }

}
