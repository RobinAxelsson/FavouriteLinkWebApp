using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FavouriteLinkWebApp.Pages
{
    public class AddLinkModel : PageModel
    {
        public LinkClient _linkClient;
        public AddLinkModel(LinkClient linkClient)
        {
            _linkClient = linkClient;
        }
        [BindProperty]
        public Link link { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            bool wasAdded = false;
            Func<string, bool> notNull = x => !String.IsNullOrEmpty(x);

            if (notNull(link.Name) && notNull(link.Group) && notNull(link.Url))
                wasAdded = await _linkClient.TryAddLink(link);

            return wasAdded ? RedirectToPage("/Index") : RedirectToPage("/Error");
        }
    }
}
