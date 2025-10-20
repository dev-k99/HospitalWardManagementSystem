using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles="Admin")]
public class AdminIndexModel : PageModel
{
    public void OnGet() { }
}
