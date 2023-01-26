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
        public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
        public DbSet<Tag> Tags => Set<Tag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoryTag>()
                .HasKey(st => new { st.StoryId, st.TagName });

            modelBuilder.Entity<StoryTag>()
                .HasOne(st => st.Story)
                .WithMany(s => s.StoryTags)
                .HasForeignKey(st => st.StoryId);

            modelBuilder.Entity<StoryTag>()
                .HasOne(st => st.Tag)
                .WithMany(t => t.StoryTags)
                .HasForeignKey(st => st.TagName);
        }
    }
}