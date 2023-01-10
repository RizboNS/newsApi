using Microsoft.EntityFrameworkCore;
using newsApi.Models;

namespace newsApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<ImageDb> ImageDbs => Set<ImageDb>();
        public DbSet<Story> Stories => Set<Story>();
        public DbSet<Tag> Tags => Set<Tag>();
    }
}