using Library.Data;
using Library.Migrations;
using Library.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    public class BookController : Controller
    {
        private readonly mycontext _context;
        private readonly IHostEnvironment _hostingEnvironment;

        public BookController(mycontext context, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult LibraryHomepage()
        {
            // Fetch all books from the database
            var books = _context.books.ToList();

            // Check if the user is authenticated and fetch their data
            var username = User.Identity.Name;
            var user = _context.users.FirstOrDefault(u => u.Name == username);

            // If user is authenticated, pass their full name; otherwise, pass null
            var Name = user?.Name;

            // Pass the books list and the user's full name to the view
            ViewData["Name"] = Name;

            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> BookList()
        {
            // Load books along with their category using Eager Loading
            var books = await _context.books
                .Include(b => b.Category) // Eagerly load the Category
                .ToListAsync();

            return View(books); // Pass the list of books to the view
        }

        public IActionResult AddBook()
        {
            ViewBag.getcategory = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(IFormFile imagePath, string bookTitle, string authorName, decimal price, int quantity, string description, int categoryId)
        {
            if (ModelState.IsValid)
            {
                if (imagePath != null && imagePath.Length > 0)
                {
                    // Define the folder path where images will be stored
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Book");

                    // Ensure the folder exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name for the image to avoid overwriting
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imagePath.FileName);

                    // Combine the folder path with the unique file name
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to the folder
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagePath.CopyToAsync(fileStream);
                    }

                    // Create a new Book object with the relative image path
                    var book = new Book
                    {
                        BookTitle = bookTitle,
                        AuthorName = authorName,
                        Price = price,
                        Quantity = quantity,
                        Description = description,
                        CategoryID = categoryId, // Default to "Pending" if status is null
                        ImagePath = $"/Book/{uniqueFileName}" // Store the relative path
                    };

                    // Add the new book to the database context and save changes
                    _context.books.Add(book);
                    await _context.SaveChangesAsync();

                    // Redirect to the Index action or a confirmation page
                    return RedirectToAction("Index");
                }

                // Add an error message if no image was uploaded
                ModelState.AddModelError("ImagePath", "Please upload an image.");
            }

            // Reload categories in case of invalid state
            ViewBag.getcategory = _context.Categories.ToList();
            return View();
        }

        public async Task<IActionResult> Borrow(int id)
        {
            // Get the logged-in user's email
            var userEmail = User.Identity.Name ?? HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            // Find the student
            var user = await _context.users.FirstOrDefaultAsync(s => s.Email == userEmail);
            if (user == null)
            {
                return NotFound($"Student not found for email: {userEmail}");
            }

            // Get the selected book
            var book = await _context.books.FirstOrDefaultAsync(b => b.BookID == id);
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            if (book.Quantity <= 0)
            {
                return BadRequest($"Book '{book.BookTitle}' is not available.");
            }

            // Create a new BookStudent record
            var borrow = new Borrow
            {
                BookID = book.BookID,
                UserID = user.UserID,
                BorrowDate = DateTime.Now,
                ReturnDate = DateTime.Now,
                Status = "Pending"
            };

            // Save the record and update the book quantity
            _context.borrows.Add(borrow);
            book.Quantity--; // Decrement available quantity
            await _context.SaveChangesAsync();
            return RedirectToAction("LibraryHomepage", "Book");
        }
        public IActionResult Borrowed()
        {
            // Get all BookStudent records, including related Book and Student data
            var borrow = _context.borrows
                .Include(bs => bs.Book)   // Include related Book
                .Include(bs => bs.User) // Include related Student
                .ToList();

            // Map to BookStudentViewModel for display
            var model = borrow.Select(bs => new Borrow
            {
                Id = bs.Id,
                BookID = bs.BookID,
                UserID = bs.UserID,
                BorrowDate = DateTime.Now,
                ReturnDate = DateTime.Now,
                Status = bs.Status,
                Book = bs.Book,         // Include Book object for display
                User = bs.User    // Include Student object for display
            }).ToList();

            return View(model);
        }
        public IActionResult Approved()
        {
            // Get all BookStudent records, including related Book and Student data
            var borrow = _context.borrows
                .Include(bs => bs.Book)   // Include related Book
                .Include(bs => bs.User) // Include related Student
                .ToList();

            // Map to BookStudentViewModel for display
            var model = borrow.Select(bs => new Borrow
            {
                Id = bs.Id,
                BookID = bs.BookID,
                UserID = bs.UserID,
                BorrowDate = DateTime.Now,
                ReturnDate = DateTime.Now,
                Status = bs.Status,
                Book = bs.Book,         // Include Book object for display
                User = bs.User    // Include Student object for display
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            // Include Book in the query to ensure it's loaded
            var borrow = await _context.borrows
                                        .Include(b => b.Book) // Ensure Book is loaded with Borrow
                                        .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow != null)
            {
                // Update the book status to "Approved"
                borrow.Status = "Approved";

                // Save changes to the database
                await _context.SaveChangesAsync();
            }

            // Redirect back to the Borrowed view
            return RedirectToAction("Borrowed");
        }

        public IActionResult Rejected()
        {
            // Get all BookStudent records, including related Book and Student data
            var borrow = _context.borrows
                .Include(bs => bs.Book)   // Include related Book
                .Include(bs => bs.User) // Include related Student
                .ToList();

            // Map to BookStudentViewModel for display
            var model = borrow.Select(bs => new Borrow
            {
                Id = bs.Id,
                BookID = bs.BookID,
                UserID = bs.UserID,
                BorrowDate = DateTime.Now,
                ReturnDate = DateTime.Now,
                Status = bs.Status,
                Book = bs.Book,         // Include Book object for display
                User = bs.User    // Include Student object for display
            }).ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            // Include Book in the query to ensure it's loaded
            var borrow = await _context.borrows
                                        .Include(b => b.Book) // Ensure Book is loaded with Borrow
                                        .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow != null)
            {
                // Update the book status to "Approved"
                borrow.Status = "Rejected";

                borrow.Book.Quantity++;
                await _context.SaveChangesAsync();
            }

            // Redirect back to the Borrowed view
            return RedirectToAction("Borrowed");
        }
        public IActionResult Returned()
        {
            // Get all BookStudent records, including related Book and Student data
            var borrow = _context.borrows
                .Include(bs => bs.Book)   // Include related Book
                .Include(bs => bs.User) // Include related Student
                .ToList();

            // Map to BookStudentViewModel for display
            var model = borrow.Select(bs => new Borrow
            {
                Id = bs.Id,
                BookID = bs.BookID,
                UserID = bs.UserID,
                BorrowDate = DateTime.Now,
                ReturnDate = DateTime.Now,
                Status = bs.Status,
                Book = bs.Book,         // Include Book object for display
                User = bs.User    // Include Student object for display
            }).ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            // Include Book in the query to ensure it's loaded
            var borrow = await _context.borrows
                                        .Include(b => b.Book) // Ensure Book is loaded with Borrow
                                        .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow != null)
            {
                // Update the book status to "Approved"
                borrow.Status = "Returned";

                borrow.Book.Quantity++;
                await _context.SaveChangesAsync();
            }

            // Redirect back to the Borrowed view
            return RedirectToAction("Borrowed");
        }
        public IActionResult Archived()
        {
            // Get all BookStudent records, including related Book and Student data
            var borrow = _context.borrows
                .Include(bs => bs.Book)   // Include related Book
                .Include(bs => bs.User) // Include related Student
                .ToList();

            // Map to BookStudentViewModel for display
            var model = borrow.Select(bs => new Borrow
            {
                Id = bs.Id,
                BookID = bs.BookID,
                UserID = bs.UserID,
                BorrowDate = DateTime.Now,
                ReturnDate = DateTime.Now,
                Status = bs.Status,
                Book = bs.Book,         // Include Book object for display
                User = bs.User    // Include Student object for display
            }).ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            // Include Book in the query to ensure it's loaded
            var borrow = await _context.borrows
                                        .Include(b => b.Book) // Ensure Book is loaded with Borrow
                                        .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow != null)
            {
                // Update the book status to "Approved"
                borrow.Status = "Archived";

                await _context.SaveChangesAsync();
            }

            // Redirect back to the Borrowed view
            return RedirectToAction("Borrowed");
        }
        public IActionResult History()
        {
            // Get logged-in user's email from session
            var loggedInEmail = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(loggedInEmail))
            {
                // Redirect to login if no user is logged in
                return RedirectToAction("Login", "User");
            }

            // Get the logged-in user from the database
            var loggedInUser = _context.users.FirstOrDefault(u => u.Email == loggedInEmail);
            if (loggedInUser == null)
            {
                // Redirect to login if the user is not found
                return RedirectToAction("Login", "User");
            }

            // Get all borrow records for the logged-in user, including related Book data
            var borrowRecords = _context.borrows
                .Where(bs => bs.UserID == loggedInUser.UserID)  // Filter by logged-in user's ID
                .Include(bs => bs.Book)   // Include related Book
                .ToList();

            // Map to ViewModel for display
            var model = borrowRecords.Select(bs => new Borrow
            {
                Id = bs.Id,
                Book = bs.Book,  // Assuming Book model has a Title property
                BorrowDate = bs.BorrowDate,
                ReturnDate = bs.ReturnDate,
                Status = bs.Status
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // Find the borrow record by ID
            var borrowRecord = _context.borrows
                .FirstOrDefault(bs => bs.Id == id);

            // If the borrow record does not exist, return NotFound
            if (borrowRecord == null)
            {
                // Return NotFound if no matching borrow record exists
                return NotFound();
            }

            // If the borrow record exists, return the quantity of the book
            var book = _context.books.FirstOrDefault(b => b.BookID == borrowRecord.BookID);
            if (book != null)
            {
                // Increment the book quantity as the borrow record is being deleted
                book.Quantity++;
            }

            // Remove the borrow record from the context
            _context.borrows.Remove(borrowRecord);

            // Save changes to the database
            _context.SaveChanges();

            // Redirect to the History page after deletion
            return RedirectToAction("History");
        }
    }
}
