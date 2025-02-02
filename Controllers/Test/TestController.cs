using BiblePay.BMS.DSQL;
using BiblePay.BMS.Models;
using BMSCommon.Model;
using Google.Authenticator;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.DOMItem;


namespace BiblePay.BMS.Controllers
{
    public class TestController : Controller
    {

		public ActionResult AssociateTwoFA()
		{
			TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
			string sSecret = "1";
			var setupInfo = twoFactor.GenerateSetupCode("TEST_UNCHAINED",
				"you@biblepay.org", sSecret, false, 3);
			ViewBag.SetupCode = setupInfo.ManualEntryKey;
			ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
			return View("TestPage");
		}

		public JsonResult Check2FA([FromBody] ClientToServer o)
		{
			TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
			string sSecret = "1";
			string sCode = GetFormData(o.FormData, "txtCode");
			bool isValid = twoFactor.ValidateTwoFactorPIN(sSecret, sCode);
			string sNarr = isValid ? "Success" : "Failed";
			return this.ShowModalDialog(o, "auth", sNarr, "");
		}


		public ActionResult TestPage()
        {
            Models.City c = new City();
            c.Name1 = "Satoshi 1";
            c.Id = 1;
            return View(c);
        }
        



		public async Task<JsonResult> TestSection10([FromBody] BMSCommon.Model.ClientToServer o)
        {
            City c = new City();
				ControllerExtensions2.BindObject<City>(c, o);
			c.Zip++;
            return await this.RenderDivToClient<City>("_TestSection10", c, "partial-section10", true);
        }

        


        [ValidateAntiForgeryToken]
        public JsonResult TestSection1([FromBody] ClientToServer o)
        {
			ViewBag.txtName = GetFormData(o.FormData,"txtName");
			if (ViewBag.txtName.Length < 1)
			{
				ViewBag.txtError = "<font color=red>YOU MUST ENTER IT</font>";
			}
			else
			{
				ViewBag.txtError = "";
			}

            return this.RenderDivToClient<City>("_TestSection1", null, "partial-load-section1", true).Result;
		}



        public JsonResult TestSection2([FromBody] ClientToServer o)
        {
            ViewBag.txtCity = GetFormData(o.FormData, "txtCity");
            return this.RenderDivToClient<string>("_TestSection2", null, "partial-load-section2", true).Result;
        }

        
        public async Task<JsonResult> TestSection3([FromBody] BMSCommon.Model.ClientToServer o)
        {
            ViewBag.txtState = GetFormData(o.FormData, "txtState2");
            return await this.RenderDivToClient<string>("_TestSection3", null, "partial-load-section3", true);
        }


        [HttpPost]
        public JsonResult AutoCity(string Prefix)
        {
            List<City> ObjList = new List<City>()
            {
                new City {Id=1,Name1="Autumn" },
                new City {Id=2,Name1="Bobert" },
                new City {Id=3,Name1="Pune" },
                new City {Id=4,Name1="Delhi" },
                new City {Id=5,Name1="Dehradun" },
                new City {Id=6,Name1="Noida" },
                new City {Id=7,Name1="New Delhi" }

            };
            //Searching records from list using LINQ query  
            var Name = (from N in ObjList
                        where N.Name1.ToLower().StartsWith(Prefix.ToLower())
                        select new { N.Name1 });
            return Json(Name);
        }
        
    }
}
