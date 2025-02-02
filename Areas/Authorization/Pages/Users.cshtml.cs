using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BiblePay.BMS.Areas.Authorization.Pages
{
    [Authorize]
    public class UserModel : PageModel
    {
    }
}
