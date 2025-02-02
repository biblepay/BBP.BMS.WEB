using BMSCommon.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BiblePay.BMS.Controllers
{
    public partial class BBPController : Controller
	{
		public string GetWellsReport()
        {
			List<Well> lWells = BBPAPI.Interface.Repository.GetWellsReport();
            List<Pin> pins = BBPAPI.Interface.Repository.GetWellsPinsReport();
			string html = "";

            for (int i = 0; i < lWells.Count; i++)
            {
                Well dr = lWells[i];
                string sWell = "<div class='row'>"
                    + "<div class='card border' style='width:100%;'>"
                        + "<h3> Well #" + dr.id.ToString() 
                        + " - " + dr.added.ToShortDateString() + "</h3>";
                //string sCDN = "https://unchained.biblepay.org/wwwroot/wells2/";
                string sCDN = "/wwwroot/wells2/";

                string sPrefix = "well" + dr.id.ToString() + "_";
                sWell += "<table><tr><td><h2>Site Selection:</h2>"
                     + "<img width=500 height=250 src='" + sCDN + "" + sPrefix + "location.jpg'/></td>";
                var matchH2 = pins.FirstOrDefault(URL => URL.URL.Contains(sPrefix + "handover2"));

                sWell += "<td><h2>Handover:</h2>"
                        + "<img width=500 height=250 src='" + sCDN + sPrefix + "handover.jpg'/>";
                if (matchH2 != null)
                {
                        sWell +="<img width=500 height=250 src='" + sCDN + sPrefix + "handover2.jpg'/>"
                        + "</td>";
                }
                sWell += "</tr>";
                string sWT = sPrefix + "watertest";
                string sDed = sPrefix + "dedication";
                var matchWaterTest = pins.FirstOrDefault(URL => URL.URL.Contains(sWT));
                var matchDed = pins.FirstOrDefault(URL =>   URL.URL.Contains(sDed));
                if (matchDed != null)
                {
                    sWell += "<tr><td><h2>Dedication:</h2>"
                    + "   <video width='500' height='350' style='background-color:black' controls>"
                    + "   <source src='" + sCDN + sPrefix + "dedication.mp4' type='video/mp4' />"
                    + "   </td></tr>";

                }
                if (matchWaterTest != null)
                {
                    sWell += "<tr><td><h2>Water Test:</h2>"
                        + "<img width=500 height=250 src='" + sCDN + sPrefix + "watertest.jpg'/>"
                    + "   </td></tr>";

                }
                sWell += "</table>";
                sWell += "</div></div>";

                html += sWell + "";

            }
			return html;
		}

		public IActionResult Wells()
        {
			ViewBag.WellsReport = GetWellsReport();
            return View();
        }
    }
}
