using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using sr_watcher.Data;
using sr_watcher.DTO;

namespace sr_watcher.BackgroundServices
{
    public class RedditPoller
    {
        #region private fields
        protected RedditPollServiceConfig? _pollerConfig { get; set; }
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceScope _scope;
        private SrWatcherDataContext __dbContext;
        private SrWatcherDataContext? _dbContext
        {
            get
            {
                if (__dbContext == null)
                {
                    _scope = this._scopeFactory.CreateScope();
                    __dbContext = _scope.ServiceProvider.GetRequiredService<SrWatcherDataContext>();
                }
                return __dbContext;
            }
        }
        #endregion private fields

        #region ctor
        public RedditPoller(IServiceScopeFactory scopeFactory, RedditPollServiceConfig config) 
        {
            _scopeFactory = scopeFactory;
            _pollerConfig = config;
        }
        #endregion ctor

        #region public functions
        public void PollForNewPosts()
        {
            try
            {
                // this gets new posts
                string baseUrl = $"https://oauth.reddit.com/r/{_pollerConfig?.SubredditName}/new.json";

                // if there is nothing in the db yet, populate with a couple entries
                string url = baseUrl;
                var myBookmark = _dbContext.RedditApiBookmarks?.FirstOrDefault(a => a.ApiBaseUrl == baseUrl);
                if (myBookmark == null)
                {
                    url += "?limit=" + _pollerConfig?.DatabaseSeedCount.ToString();
                }
                else
                {
                    url += "?before=" + myBookmark.BookmarkValue;
                }

                // poll api, process results, save to db
                doPollAndSaveResults(url);

                // bookmark
                saveBookmark(baseUrl);

                // post processing
                processAuthorTotals();
            }
            catch (Exception ex)
            {
                SrWatcherLogger.LogError("error during PollForNewPosts : ", ex);
            }
        }

        public void PollToUpdatePostMetadata()
        {
            try
            {
                // this list of posts by name. i wonder if there is a limit of names that can be submitted, the api doc doesnt specify...
                string url = "https://oauth.reddit.com/by_id/";
                List<string?> postNames = _dbContext.RedditApiPosts?.Select(a => a.name).ToList();
                if (postNames.Any())
                {
                    string postNamesParam = String.Join(",", postNames);
                    url += postNamesParam;

                    doPollAndSaveResults(url);
                }
            }
            catch (Exception ex)
            {
                SrWatcherLogger.LogError("error during PollToUpdatePostMetadata : ", ex);
            }
        }

        #endregion public functions

        #region private functions
        private async void doPollAndSaveResults(string url)
        {
            try
            {
                string? jsonResponse = await ApiCaller.Instance.CallApi(_pollerConfig.RedditAccessInfo.access_token, url);

                // Process the jsonResponse to deserialize
                List<RedditApiPostData>? redditPostResults = RedditResponseParser.GetRedditPostsDataFromApiListingResponse(jsonResponse);

                processAndSavePollResults(redditPostResults);
            }
            catch (Exception ex)
            {
                SrWatcherLogger.LogError("error during doPollAndSaveResults : ", ex);
            }
        }

        private void processAndSavePollResults(List<RedditApiPostData>? redditPostResults)
        {
            try
            {
                // database saves
                bool changesDetected = false;

                // figure out what inserts and updates we need to do
                if (redditPostResults != null)
                {
                    foreach (var redditPostResult in redditPostResults)
                    {
                        var localRecord = _dbContext.RedditApiPosts?.FirstOrDefault(a => a.id == redditPostResult.id);
                        if (localRecord == null)
                        {
                            // insert
                            localRecord = redditPostResult;
                            _dbContext.RedditApiPosts.Add(localRecord);
                            changesDetected = true;
                        }
                        else
                        {
                            // update
                            // which fields to update...? for now we care about upvotes
                            // this could be expanded to compare and check multiple fields
                            if (localRecord.ups != redditPostResult.ups)
                            {
                                localRecord.ups = redditPostResult.ups;
                                changesDetected = true;
                            }
                        }
                    }

                    if (changesDetected)
                    {
                        _dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                SrWatcherLogger.LogError("error during processAndSavePollResults : ", ex);
            }
        }

        private void processAuthorTotals()
        {
            try
            {
                // post processing
                // catalog all the users we know about and who has the most upvotes
                bool doSaveChanges = false;

                //var authorTotals = _dbContext.RedditApiPosts.GroupBy(a => a.author_fullname).Select(b => new AuthorTotal { Author = b.Key, TotalUps = b.Sum(c => c.ups) }).ToList();
                var authorTotals = _dbContext.RedditApiPosts?.GroupBy(a => a.author).Select(b => new AuthorTotal { Author = b.Key, TotalUps = b.Sum(c => c.ups) }).ToList();
                if (authorTotals != null)
                {
                    foreach (var authorTotal in authorTotals)
                    {
                        var dbAuthorTotal = _dbContext.AuthorTotals?.FirstOrDefault(a => a.Author == authorTotal.Author);
                        if (dbAuthorTotal == null)
                        {
                            dbAuthorTotal = new AuthorTotal
                            {
                                Author = authorTotal.Author,
                                TotalUps = authorTotal.TotalUps,
                                //AuthorName = authorTotal.AuthorName
                            };

                            _dbContext.AuthorTotals?.Add(dbAuthorTotal);
                            doSaveChanges = true;
                        }
                        else
                        {
                            if (dbAuthorTotal.TotalUps != authorTotal.TotalUps)
                            {
                                dbAuthorTotal.TotalUps = authorTotal.TotalUps;
                                doSaveChanges = true;
                            }
                        }
                    }
                    if (doSaveChanges)
                    {
                        _dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                SrWatcherLogger.LogError("error during processAuthorTotals : ", ex);
            }
        }

        private void saveBookmark(string apiBaseUrl)
        {
            try
            {
                var postToBookmark = _dbContext.RedditApiPosts?.OrderByDescending(a => a.created_utc).FirstOrDefault();
                if (postToBookmark != null)
                {
                    var dbBookmark = _dbContext.RedditApiBookmarks?.FirstOrDefault(a => a.ApiBaseUrl == apiBaseUrl);
                    if (dbBookmark == null)
                    {
                        dbBookmark = new RedditApiBookmark
                        {
                            ApiBaseUrl = apiBaseUrl
                        };
                        _dbContext.RedditApiBookmarks?.Add(dbBookmark);
                    }
                    dbBookmark.BookmarkValue = postToBookmark.name;
                    _dbContext.SaveChanges();
                }
            }catch (Exception ex)
            {
                SrWatcherLogger.LogError("error during saveBookmark : ", ex);
            }
        }

        #endregion private functions
    }
}
