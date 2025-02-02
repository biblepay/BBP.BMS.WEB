using BBPAPI;
using BiblePay.BMS.Extensions;
using BiblePay.BMS.Models;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;

namespace BiblePay.BMS.Controllers
{
	public partial class BBPController : Controller
    {
        private IBBPSvc _ibbpsvc;

        public BBPController(IBBPSvc mybbpsvc)
        {
            _ibbpsvc = mybbpsvc;
        }

		public IActionResult Report1()
        {
            DummyModel model = new DummyModel();
            for (int i = 0; i < 40; i++)
            {
                DummyObject dummy = new DummyObject();
                dummy.A = i.ToString();
                dummy.B = i.ToString();
                dummy.C = i.ToString();
                model.MyItems.Add(dummy);
            }
            DummyObject d1 = new DummyObject();
            d1.FieldName = "country";
            model.MyCols.Add(d1);
            DummyObject d2 = new DummyObject();
            d2.FieldName = "prefix";
            model.MyCols.Add(d2);
            DummyObject d3 = new DummyObject();
            // Remember for filtering, column names must be lowercase:
            d3.FieldName = "description";
            model.MyCols.Add(d3);
            List<PhoneRate> l = BBPAPI.Interface.Phone.GetRatesReport(1);
            ViewBag.DataSource = l;
            return View(model);
        }
        public IActionResult Admin()
        {
            string sMyBar = _ibbpsvc.BarRequest("0");

            if (HttpContext.GetCurrentUser().ERC20Address != "BFjZ9eMmjCNZCBrtvBZSYqxvwhSPwxLBCT")
            {
                Response.Redirect("/gospel/about");
            }
            return View();
        }


        public IActionResult Scratchpad()
        {
            return View();
        }


        public IActionResult MessagePage()
        {
            ViewBag.Title = HttpContext.Session.GetString("msgbox_title");
            ViewBag.Heading = HttpContext.Session.GetString("msgbox_heading");
            ViewBag.Body = HttpContext.Session.GetString("msgbox_body");
            return View();
        }

        public IActionResult Watch()
        {
            //ViewBag.WatchVideo = DSQL.youtube.GetVideo(HttpContext);
            return View();
        }

        public IActionResult Videos()
        {
            //ViewBag.VideoList = DSQL.youtube.GetSomeVideos(HttpContext);
            return View();
        }
        
        public IActionResult Univ()
        {
            return View();
        }
    }
}
