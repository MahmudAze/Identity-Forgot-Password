using MainBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MainBackend.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Slider> Sliders { get; set; }
        public DbSet<SliderDetail> SliderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Blog>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Blog>().HasData(
                new Blog
                {
                    Id = 1,
                    Image = "blog-feature-img-1.jpg",
                    Title = "Flower Power",
                    Description = "Class aptent taciti sociosqu ad litora torquent per conubia nostra, per",
                    CreatedDate = new DateTime(2025, 12, 22),
                    IsDeleted = false
                },
                new Blog
                {
                    Id = 2,
                    Image = "blog-feature-img-3.jpg",
                    Title = "Local Florists",
                    Description = "This is the first post in our blog. Stay tuned for updates!",
                    CreatedDate = new DateTime(2025, 12, 22),
                    IsDeleted = false
                },
                new Blog
                {
                    Id = 3,
                    Image = "blog-feature-img-4.jpg",
                    Title = "Flower Beauty",
                    Description = "This is the first post in our blog. Stay tuned for updates!",
                    CreatedDate = new DateTime(2025, 12, 22),
                    IsDeleted = false
                }
            );
        }
    }
}
