using Microsoft.AspNetCore.Mvc.RazorPages;
using sr_watcher.Common;

namespace sr_watcher.Pages.Shared
{
    public abstract class BasePage : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private SrwSession _srwSession { get; set; }

        /// <summary>
        /// todo: is session the best place for this stuff? what other options? viewbag, cache, etc.
        /// </summary>
        public SrwSession SrwSession
        {
            get
            {
                if (_srwSession == null)
                {
                    _srwSession = new SrwSession(HttpContext.Session);
                }

                return _srwSession;
            }
        }

        /// <summary>
        /// todo: resolve nullable warning and all other warnings
        /// </summary>
        /// <param name="logger"></param>
        public BasePage(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
    }
}
