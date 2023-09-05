using System.Text.Json;

namespace sr_watcher.DTO
{
    public static class RedditResponseParser
    {
        public static List<RedditApiPostData?> GetRedditPostsDataFromApiListingResponse(string jsonResponse)
        {
            List<RedditApiPostData?> result = null;
            try
            {
                RedditApiListing listing = JsonSerializer.Deserialize<RedditApiListing>(jsonResponse);

                if (listing != null && listing.data != null && listing.data.children != null)
                {
                   result = listing.data.children.Select(a => a.data).ToList(); 
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing Api Response : ", ex);
            }
            return result;
        }
    }
}
