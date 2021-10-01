using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FavouriteLinkWebApp.Pages
{
    public class DeleteModel : PageModel
    {
        public async Task<IActionResult> OnGet(string id, string group)
        {
            Console.WriteLine("Name: " + id + " Group: " + group);
            var _linkClient = new LinkClient();
            bool wasDeleted = false;
            Func<string, bool> notNull = x => !String.IsNullOrEmpty(x);

            if (notNull(id) && notNull(group))
            {
                wasDeleted = await _linkClient.TryDeleteLink(id, group);
                return wasDeleted ? RedirectToPage("/Index") : RedirectToPage("/Error");
            }
            return Page();
        }
    }
}
