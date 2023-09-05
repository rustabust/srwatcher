using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sr_watcher.Data;

namespace sr_watcher.Pages
{
    public class DashboardDataModel
    {
        public string? SubredditName { get; set; }
        public List<sr_watcher.DTO.RedditApiPostData>? TopPosts { get; set; }
        public List<sr_watcher.Data.AuthorTotal>? TopAuthors { get; set; }
        public DateTime LastUpdated { get; set; }
    }


    public class DashboardModel : PageModel
    {
        protected const int SHOW_PER_PAGE = 5;

        public DashboardModel(SrWatcherDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DashboardDataModel? DashboardData { get; set; }
        private SrWatcherDataContext _dbContext { get; set; }

        public void OnGet()
        {
            // look up and display stats about whats going on...
            var post = _dbContext.RedditApiPosts?.FirstOrDefault();
            if (post != null)
            {
                // start with some fake stuff
                this.DashboardData = new DashboardDataModel();
                this.DashboardData.SubredditName = post.subreddit;
                this.DashboardData.TopPosts = _dbContext.RedditApiPosts?.OrderByDescending(a => a.ups).Take(SHOW_PER_PAGE).ToList();
                this.DashboardData.TopAuthors = _dbContext.AuthorTotals?.OrderByDescending(a => a.TotalUps).Take(SHOW_PER_PAGE).ToList(); 
                this.DashboardData.LastUpdated = DateTime.Now;
            }
        }
    }
}
