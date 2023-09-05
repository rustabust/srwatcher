using sr_watcher.Data;

namespace sr_watcher.BackgroundServices
{
    public class RedditPollService : IHostedService
    {
        private Timer? _timer;

        private RedditPollServiceConfig? pollerConfig { get; set; }

        private readonly IServiceScopeFactory _scopeFactory;

        public RedditPollService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Configure(RedditPollServiceConfig config)
        {
            this.pollerConfig = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SrWatcherLogger.LogInfo("RedditPoller service is starting.");
            
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(this.pollerConfig.ExecuteIntervalSeconds)); 

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            // todo:
            // dowork executes independently of the timer. i.e. the timer does not pause while we execute dowork.
            // this has potential for piling up on itself if execution starts to take longer than the timer interval
            // if this is a real concern, should implement a semaphore around this code...

            SrWatcherLogger.LogInfo($"RedditPoller.DoWork task executed at: {DateTime.Now}");

            using (var scope = this._scopeFactory.CreateScope())
            {
                // todo: these two calls could be refactored into separate classes extended from each other. 
                var redditPoller = new RedditPoller(this._scopeFactory, this.pollerConfig);
                redditPoller.PollForNewPosts();
                redditPoller.PollToUpdatePostMetadata();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            SrWatcherLogger.LogInfo("RedditPoller service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
