using sr_watcher.DTO;

namespace sr_watcher.BackgroundServices
{
    public class RedditPollServiceConfig
    {
        public int ExecuteIntervalSeconds { get; set; }

        public string SubredditName { get; set; }
        public RedditAccessTokenResponse RedditAccessInfo { get; set; }
        public int DatabaseSeedCount { get; set; }

        public RedditPollServiceConfig(string subRedditName, RedditAccessTokenResponse redditAccessInfo, int executeIntervalSeconds = 2000)
        {
            this.ExecuteIntervalSeconds = executeIntervalSeconds;
            this.SubredditName = subRedditName;
            this.RedditAccessInfo = redditAccessInfo;
            this.DatabaseSeedCount = 100;
        }
    }
}
