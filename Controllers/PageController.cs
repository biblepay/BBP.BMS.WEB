using Microsoft.AspNetCore.Mvc;

namespace BiblePay.BMS.Controllers
{
	public class PageController : Controller
    {
        public IActionResult Chat() => View();
        public IActionResult Confirmation() => View();
        public IActionResult Contacts() => View();
        public IActionResult Error() => View();
        public IActionResult Login() => View();
        public IActionResult LoginAlt() => View();

    }
}
