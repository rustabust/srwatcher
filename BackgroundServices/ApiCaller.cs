using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using sr_watcher.Data;
using System;
using System.Reflection.PortableExecutable;

namespace sr_watcher.BackgroundServices
{
    /// <summary>
    /// Class responsible for making REST API calls. 
    /// Manages rate limiting based on headers returned from the service.
    /// </summary>
    public class ApiCaller
    {
        private static ApiCaller _instance;
        private static readonly object _lock = new object();
        
        private double RateLimitRemaining { get; set; }
        private int RateLimitUsed { get; set; }
        private DateTime RateLimitResetDateTime { get; set; }
        //private bool OfficialRateLimitsSet { get; set; }

        private ApiCaller()
        {
            // set some basic / low default rate limit values here
            // real / official rate limit info should be set after making the first API call
            RateLimitRemaining = 10;
            RateLimitUsed = 0;
            RateLimitResetDateTime = DateTime.Now.AddMinutes(5);
            //OfficialRateLimitsSet = false;
        }

        public static ApiCaller Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ApiCaller();
                    }
                    return _instance;
                }
            }
        }       

        public async Task<string?> CallApi(string accessToken, string url)
        {
            checkRateLimit();

            string? jsonResponse = null;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "sr_watcher/1.0");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");

                // make request
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // update info from response headers
                    processHeaders(response);

                    // read the response
                    jsonResponse = await response.Content.ReadAsStringAsync();
                }
            }
            return jsonResponse;
        }

        private void checkRateLimit()
        {
            // check rate limit info. if we are close to our limit, wait until rate limit period is over until you try. 
            // this would probably be better with some kind of queue system rather than just sleeping the thread...
            if (this.RateLimitRemaining < 5)
            {
                var milliSecondsToSleep = (int)this.RateLimitResetDateTime.Subtract(DateTime.Now).TotalMilliseconds;
                if (milliSecondsToSleep > 0)
                {
                    Thread.Sleep(milliSecondsToSleep);
                }
            }
        }

        private void processHeaders(HttpResponseMessage response)
        {
            // update rate limit info
            this.RateLimitRemaining = getHeaderDouble("x-ratelimit-remaining", response, this.RateLimitRemaining);
            this.RateLimitUsed = getHeaderInt("x-ratelimit-used", response, this.RateLimitUsed);
            int secondsTilReset = getHeaderInt("x-ratelimit-reset", response, -1);
            if (secondsTilReset > 0)
            {
                this.RateLimitResetDateTime = DateTime.Now.AddSeconds(secondsTilReset);
            }
            else
            {
                SrWatcherLogger.LogWarning($"unexpected type or value for secondsTilReset : '{secondsTilReset}'");
            }
            //this.OfficialRateLimitsSet = true;
        }

        private static int getHeaderInt(string headerName, HttpResponseMessage response, int defaultVal)
        {
            int result = defaultVal;
            string? sHeader = getHeaderString(headerName, response);
            if (!int.TryParse(sHeader, out result))
            {
                SrWatcherLogger.LogWarning($"unexpected type or value returned in header '{headerName}'; expected a non-null integer but value was '{sHeader}'");
            }
            return result;
        }

        private static double getHeaderDouble(string headerName, HttpResponseMessage response, double defaultVal)
        {
            double result = defaultVal;
            string? sHeader = getHeaderString(headerName, response);
            if (!double.TryParse(sHeader, out result))
            {
                SrWatcherLogger.LogWarning($"unexpected type or value returned in header '{headerName}'; expected a non-null double but value was '{sHeader}'");
            }
            return result;
        }

        private static string? getHeaderString(string headerName, HttpResponseMessage response)
        {
            string? result = null;
            var values = response.Headers.GetValues(headerName);
            if (values == null || !values.Any())
            {
                string url = "(unknown)";
                if (response.RequestMessage != null && response.RequestMessage.RequestUri != null)
                {
                    url = response.RequestMessage.RequestUri.ToString();
                }
                SrWatcherLogger.LogWarning($"expected header {headerName} was missing in response from api with url={url}");
            }
            else
            {
                result = values.FirstOrDefault();
            }
            return result;
        }
    }
}
