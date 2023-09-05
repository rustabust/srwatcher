namespace sr_watcher.DTO
{
    public class RedditApiObjectBase
    {
        public string? kind { get; set; }
        public virtual RedditApiData? data { get; set; }
    }

    public class RedditApiListing : RedditApiObjectBase
    {
    }

    public class RedditApiData
    {
        // "after": "t3_162sqn2",
        //"dist": 5,
        //"modhash": null,
        //"geo_filter": "",
        //public RedditApiObjectBase[]? children { get; set; }
        public RedditApiPost[]? children { get; set; }
    }

    public class RedditApiPost
    {
        public RedditApiPostData? data { get; set; }
    }

    public class RedditApiPostData
    { 
        public string? subreddit { get; set; }
        public string? selftext { get; set; }
        public string? author_fullname { get; set; }
        public string? author { get; set; }
        public string? title { get; set; }
        public string? name { get; set; }
        public int ups { get; set; }
        public string? selftext_html { get; set; }
        public string? subreddit_id { get; set; }
        public string? id { get; set; }
        public double created_utc { get; set; }
        public string? url { get; set; }
        public string? permalink { get; set; }
    }

}
