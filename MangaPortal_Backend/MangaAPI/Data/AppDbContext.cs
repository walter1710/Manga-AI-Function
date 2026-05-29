using MangaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<PageRegion> PageRegions { get; set; }

        public DbSet<ChapterPageAnnotation> ChapterPageAnnotations { get; set; }
    }
}