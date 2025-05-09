using Library.Data;
using Library.Migrations;
using Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    public class CategoryController : Controller
    {
        private readonly mycontext _context;
        public CategoryController(mycontext context)
        {
            _context = context;
        }
        public IActionResult CategoryList ()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
        [HttpPost]
        public IActionResult AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();

                // Reload the categories after adding the new one
                ViewBag.Categories = _context.Categories.ToList();
                return View();
            }

            // Load categories even if there's a validation error
            ViewBag.Categories = _context.Categories.ToList();
            return View(category);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }


    }
}
