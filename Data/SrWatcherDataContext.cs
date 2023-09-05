using Microsoft.EntityFrameworkCore;
using sr_watcher.DTO;

namespace sr_watcher.Data
{
    public class SrWatcherDataContext : DbContext
    {
        public SrWatcherDataContext(DbContextOptions<SrWatcherDataContext> options) : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("DataSource=mydatabase.db");

        public DbSet<RedditApiPostData>? RedditApiPosts { get; set; }
        public DbSet<RedditApiBookmark>? RedditApiBookmarks { get; set; }
        public DbSet<AuthorTotal>?   AuthorTotals { get; set; }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public class RedditApiBookmark
    {
        public string? Id { get; set; }
        public string? ApiBaseUrl
        {
            get { return this.Id; }
            set { this.Id = value; }
        }

        /// <summary>
        /// for reddit api posts this will be a listing full name but possible this table reused for other apis
        /// </summary>
        public string? BookmarkValue { get; set; }
    }

    public class AuthorTotal
    {
        public string? Id { get; set; }
        public string? Author
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        public int? TotalUps { get; set; }
        //public string? AuthorName { get; set; }
    }

}
