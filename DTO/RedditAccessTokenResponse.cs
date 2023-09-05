namespace sr_watcher.DTO
{
    public class RedditAccessTokenResponse
    {
        /// <summary>
        /// your access token
        /// </summary>
        public string? access_token { get; set; }

        /// <summary>
        /// this should always be "bearer"
        /// </summary>
        public string? token_type { get; set; }

        /// <summary>
        /// unix epoch seconds
        /// </summary>
        public int expires_in { get; set; }

        /// <summary>
        /// a scope string (e.g. "read") can include combinations
        /// </summary>
        public string? scope { get; set; }

        /// <summary>
        /// your refresh token
        /// </summary>
        public string? refresh_token { get; set; }
    }
}
