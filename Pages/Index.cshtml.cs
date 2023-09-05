using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.IO;
using sr_watcher.Pages.Shared;
using System.Text;
using System;
using System.Text.Json;
using sr_watcher.DTO;
using System.Net.Http.Headers;
using sr_watcher.BackgroundServices;
using System.Diagnostics.CodeAnalysis;

namespace sr_watcher.Pages
{
    public class IndexModel : BasePage
    {
        private readonly RedditPollService _redditPollerService;
        protected const string REDDIT_AUTH_REDIRECT_URL = "https://localhost:44312/?handler=AuthReturn";
        private const string REDDIT_API_CLIENT_ID = "";
        private const string REDDIT_API_CLIENT_SECRET = "";
        private const string SUBREDDIT_NAME = "homebrewing";
        public IndexModel(ILogger<IndexModel> logger, RedditPollService redditPollerService) : base(logger)
        {
            _redditPollerService = redditPollerService;
        }
        
        /// <summary>
        /// initial index page just shows a button to start the watcher. 
        /// </summary>
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetStartWatcher() 
        {
            // 1. authorize
            if (!this.SrwSession.RedditApiAuthorized)
            {
                authorizeRedditApi();
            }
            else
            {
                // 2. start thread(s) to watch
                // todo : take the subreddit name in from the ui
                // todo : play with / rethink the timing. the service will run at x interval but the apicaller will run based on rate limiting...
                RedditPollServiceConfig pollerConfig = new RedditPollServiceConfig(SUBREDDIT_NAME, SrwSession.RedditAccessTokenResponse, 30); 
                _redditPollerService.Configure(pollerConfig);

                // need to either await this or cause some delay so that this thread doesnt stop/cancel before the service has a chance to start
                await _redditPollerService.StartAsync(new CancellationToken(false));

                // 3. set the status of the watcher
                SrwSession.SrWatcherStarted = true;

                // 4. show the dashboard
                return RedirectToPage("/Dashboard");

            }
            return Page();
        }     

        public async Task<IActionResult> OnGetAuthReturn(string error, string code, string state)
        {
            // check for errors / valid input
            checkForAuthReturnErrors(error, code, state);

            var accessTokenResponse = await getOAuthToken(code);

            saveOauthInfo(accessTokenResponse);

            // go back to start up routine?
            return await OnGetStartWatcher();
        }

        protected void authorizeRedditApi()
        {
            if (!this.SrwSession.RedditApiAuthorized)
            {
                SrwSession.RedditAuthState = Guid.NewGuid().ToString();
                string duration = "permanent"; // alternatively "temporary"
                string scope = "read";

                string url = $"https://www.reddit.com/api/v1/authorize?client_id={REDDIT_API_CLIENT_ID}&response_type=code&state={SrwSession.RedditAuthState}&redirect_uri={REDDIT_AUTH_REDIRECT_URL}&duration={duration}&scope={scope}";

                Response.Redirect(url);
            }
        }
        
        private void checkForAuthReturnErrors(string error, string code, string state)
        {
            // check for errors / valid input
            // todo:this could be moved to a separate function
            if (error != null)
            {
                switch (error)
                {
                    case "access_denied":
                        throw new Exception("reddit authorization routine returned error access denied.");
                    default:
                        throw new Exception($"reddit authorization routine returned an undocumented error code ({error})");
                }
            }
            if (state != SrwSession.RedditAuthState)
            {
                throw new Exception("auth state id returned from reddit did not match the requested value."); //todo : add error handling, logging, redirect to error page
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new Exception("reddit authorization routine returned a null or empty authorization code");
            }
        }

        private async Task<RedditAccessTokenResponse> getOAuthToken(string code)
        {
            // use the code to get the token
            //todo move this to more generic function
            var client = new HttpClient();
            string uri = "https://www.reddit.com/api/v1/access_token";

            // basic oauth header                        
            var authenticationString = $"{REDDIT_API_CLIENT_ID}:{REDDIT_API_CLIENT_SECRET}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            // user agent
            var productValue = new ProductInfoHeaderValue("sr_watcher", "1.0");
            requestMessage.Headers.UserAgent.Add(productValue);

            var nvc = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", REDDIT_AUTH_REDIRECT_URL)
            };
            requestMessage.Content = new FormUrlEncodedContent(nvc);

            //var response = await client.PostAsync(uri, new FormUrlEncodedContent(nvc)); //getting 403 forbidden here            
            var response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            // todo: validate access token response object looks right and deserialization happened ok
            // save the token in session
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var accessTokenResponse = JsonSerializer.Deserialize<RedditAccessTokenResponse>(jsonResponse);

            return accessTokenResponse;
        }

        private void saveOauthInfo(RedditAccessTokenResponse accessTokenResponse)
        {
            SrwSession.RedditAccessTokenResponse = accessTokenResponse;

            // save flag to session indicating we have credentials
            // could consider just checking for the credentials in session instead of having a separate bool flag here...
            this.SrwSession.RedditApiAuthorized = true;
        }
    }
}