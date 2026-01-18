using MainBackend.Data;
using MainBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainBackend.ViewComponents
{
    public class BlogViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public BlogViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<Blog> blogs = await _context.Blogs
                .Where(m => !m.IsDeleted).ToListAsync();

            return await Task.FromResult(View(blogs));
        } 
    }
}
