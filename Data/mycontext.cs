using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Data
{
    public class mycontext : DbContext
    {
        public mycontext(DbContextOptions<mycontext> options) : base(options)
        {
        }
        public DbSet<User> users { get; set; }
        public DbSet<Book> books { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Borrow> borrows { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }

    }
}