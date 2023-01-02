using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebFrontEnd.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;        

        public Models.ShortUrl ShortUrl { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;            
        }

        public Task OnGet()
        {
            return Task.CompletedTask;
        }
    }
}