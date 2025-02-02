using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblePay.BMS.Models;
using BiblePay.BMS.Extensions;
using BBPAPI.Model;
using BMSCommon.Model;

namespace BiblePay.BMS.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            User uGlobal = HttpContext.GetCurrentUser();
            string sERC = uGlobal == null ? String.Empty : uGlobal.ERC20Address;
            var items = NavigationModel.BuildNavigation(sERC, false);
            
            return View(items);
        }
    }
}
