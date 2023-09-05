using sr_watcher.DTO;
using System.Text.Json;

namespace sr_watcher.Common
{
    public class SrwSession
    {
        public SrwSession(ISession session)
        {
            _session = session;
        }

        private ISession _session;

        /// <summary>
        /// todo : is there a better design for this that requires less overhead. can these strings be defined privately in the properties
        /// </summary>
        private const string SRW_SESSION_REDDIT_API_AUTHORIZED = "redditapiauthorized"; 
        private const string SRW_SESSION_REDDIT_API_AUTH_STATE = "redditapiauthstate";
        private const string SRW_SESSION_REDDIT_API_AUTH_ACCESS_TOKEN_RESP = "redditapiauthaccesstokenresp";
        private const string SRW_SESSION_WATCHER_STARTED = "srwatcherstarted";

        public bool RedditApiAuthorized
        {
            get
            {
                string? sVal = _session.GetString(SRW_SESSION_REDDIT_API_AUTHORIZED);
                bool result = getSafeBool(sVal);
                return result;
            }
            set
            {
                _session.SetString(SRW_SESSION_REDDIT_API_AUTHORIZED, value.ToString());
            }
        }

        public string? RedditAuthState
        {
            get
            {
                string? result = _session.GetString(SRW_SESSION_REDDIT_API_AUTH_STATE);
                return result;
            }
            set
            {
                if (value != null)
                {
                    string sVal = value;
                    _session.SetString(SRW_SESSION_REDDIT_API_AUTH_STATE, sVal);
                }
            }
        }

        /// <summary>
        /// there must be a better way to store this object without having to serialize and deserialize
        /// </summary>
        public RedditAccessTokenResponse RedditAccessTokenResponse
        {
            get
            {
                string? sTokenResp = _session.GetString(SRW_SESSION_REDDIT_API_AUTH_ACCESS_TOKEN_RESP);
                RedditAccessTokenResponse result = JsonSerializer.Deserialize<RedditAccessTokenResponse>(sTokenResp);
                return result;
            }
            set
            {
                if (value != null)
                {
                    string sResponse = JsonSerializer.Serialize<RedditAccessTokenResponse>(value);
                    _session.SetString(SRW_SESSION_REDDIT_API_AUTH_ACCESS_TOKEN_RESP, sResponse);
                }
            }
        }

        public bool SrWatcherStarted
        {
            get
            {
                string? sVal = _session.GetString(SRW_SESSION_WATCHER_STARTED);
                bool result = getSafeBool(sVal);
                return result;
            }
            set
            {
                _session.SetString(SRW_SESSION_WATCHER_STARTED, value.ToString());
            }
        }

        private static bool getSafeBool(string? s)
        {
            bool result = false;
            Boolean.TryParse(s, out result);
            return result;
        }

    }
}
