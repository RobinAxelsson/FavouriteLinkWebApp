using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FavouriteLinkWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly LinkClient _linkClient;
        public List<Link> _links = new List<Link>();

        public IndexModel(ILogger<IndexModel> logger, LinkClient linkClient)
        {
            _logger = logger;
            _linkClient = linkClient;
        }

        public async Task<IActionResult> OnGet()
        {
            _links = await _linkClient.GetAllLinksAsync();
            return Page();
        }
    }
}
