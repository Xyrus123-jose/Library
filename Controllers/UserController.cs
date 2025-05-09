using Library.Data;
using Library.Migrations;
using Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    public class UserController : Controller
    {
        private readonly mycontext _context;
        private readonly IHostEnvironment _hostingEnvironment;

        public UserController(mycontext context, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Home()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> UserList()
        {
            // Load books along with their category using Eager Loading
            var users = await _context.users.ToListAsync();

            return View(users); // Pass the list of books to the view
        }
        public IActionResult Layout()
        {
            return View();
        }
        // GET: User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(IFormFile imagePath, string name, string email, string phone, string address, string password)
        {
            if (ModelState.IsValid)
            {
                if (imagePath != null && imagePath.Length > 0)
                {
                    // Define the folder path where images will be stored
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "User");

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

                    // Create a new User object with the full image path
                    var user = new User
                    {
                        Name = name,
                        Email = email,
                        Phone = phone,
                        Address = address,
                        Password = password,
                        Role = "User",
                        ImagePath = $"/User/{uniqueFileName}" // Store the relative path
                    };

                    // Add the new user to the database context and save changes
                    _context.users.Add(user);
                    await _context.SaveChangesAsync();

                    // Redirect to a different action, e.g., to the login page or home page
                    return RedirectToAction("LibraryHomepage", "Book");
                }

                ModelState.AddModelError("ImagePath", "Please upload an image.");
            }

            // If the form is not valid or no image was uploaded, return the form view again
            return View();
        }

        [HttpGet]
        public JsonResult GetLoggedInStudent()
        {
            var loggedInEmail = HttpContext.Session.GetString("Email");
            if (!string.IsNullOrEmpty(loggedInEmail))
            {
                var loggedInStudent = _context.users.FirstOrDefault(s => s.Email == loggedInEmail);
                if (loggedInStudent != null)
                {
                    return Json(new { success = true, studentId = loggedInStudent.UserID });
                }
            }
            return Json(new { success = false });
        }
        public IActionResult Login()
        {
            return View();
        }
        // Store user data in session
        //HttpContext.Session.SetString("UserName", user.Name);
        //            HttpContext.Session.SetString("Email", user.Email);
        //            HttpContext.Session.SetString("UserImage", user.ImagePath);
        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(Login model)
        {
            var user = _context.users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
            if (user != null)
            {
                // Store user details in session
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("UserImage", user.ImagePath);
                HttpContext.Session.SetString("UserRole", user.Role);

                // Redirect based on role
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Home", "User");
                }
                else if (user.Role == "User")
                {
                    return RedirectToAction("LibraryHomepage", "Book");
                }
            }
            return View(model);
        }

        public IActionResult AddUser()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(IFormFile imagePath, string name, string email, string phone, string address, string password, string role)
        {
            if (ModelState.IsValid)
            {
                if (imagePath != null && imagePath.Length > 0)
                {
                    // Define the folder path where images will be stored
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "User");

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

                    // Create a new User object with the full image path
                    var user = new User
                    {
                        Name = name,
                        Email = email,
                        Phone = phone,
                        Address = address,
                        Password = password,
                        Role = role,
                        ImagePath = $"/User/{uniqueFileName}" // Store the relative path
                    };

                    // Add the new user to the database context and save changes
                    _context.users.Add(user);
                    await _context.SaveChangesAsync();

                    // Redirect to a different action, e.g., to the login page or home page
                    return RedirectToAction("AddUser", "User");
                }

                ModelState.AddModelError("ImagePath", "Please upload an image.");
            }

            // If the form is not valid or no image was uploaded, return the form view again
            return View();

        }

    }

}








