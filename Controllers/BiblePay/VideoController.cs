using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using static BMSCommon.Common;

namespace BiblePay.BMS.Controllers
{
    public partial class BBPController : Controller
	{
		public string GetVideo()
        {
			string sID = Request.Query["id"];
            if (sID == "1")
            {
                ViewBag.VideoFileName = "/bbp/JesusVideo.mp4";
                ViewBag.VideoComments = "Debug video";
                return String.Empty;
            }
			GetBusinessObject bo = new GetBusinessObject();
			bo.ParentID = sID;
			bo.TestNet = IsTestNet(HttpContext);

            List<VideoNew> lVideo = BBPAPI.Interface.Repository.GetVideos(bo);
			if (lVideo.Count > 0)
			{
				ViewBag.VideoPoster = lVideo[0].cover ?? String.Empty;
				if (lVideo[0].url.Contains(".mp4"))
				{
					ViewBag.VideoFileName = "BMS/WatchVideo?video=" + lVideo[0].id + "/video/" + lVideo[0].url;
				}
				else
				{
					ViewBag.VideoFileName =  "/video/" + lVideo[0].id + "/1.m3u8";
				}
				// Tack on the comments for this video.
				ViewBag.VideoComments = GetTimelinePostDiv(HttpContext, sID);
				return String.Empty;
			}
            else
            {
				string data = "Video not found.";
				return data;
            }

		}

		public IActionResult WatchVideo()
		{
			ViewBag.WatchVideo = GetVideo();
			return View();
		}

		public int GetOctetLength(string sData, int nOrdinal)
		{
			string[] vSplit = sData.Split("/");
			if (nOrdinal < vSplit.Length)
			{
				return vSplit[nOrdinal].Length;
			}
			return 0;
		}
		public string GetVideoList()
        {
			GetBusinessObject bo = new GetBusinessObject();
			bo.TestNet = IsTestNet(HttpContext);
			
			List<VideoNew> lVideo = BBPAPI.Interface.Repository.GetVideos(bo);
			lVideo = lVideo.Where( a => a.cover != null).ToList();
			int nPag = (int)GetDouble(Request.Query["pag"]);
			string html = "<div class='row js-list-filter' id='nftlist'>";
			int nTotal = 0;
			int nItemNo = 0;
            bool fAdmin = (HttpContext.GetCurrentUser().Permissions.Administrator == 1);
            int iLength = 100;
			for (int i = nPag; i < nPag + iLength && i < lVideo.Count; i++)
			{
				VideoNew v = lVideo[i];
				string sDesc = v.title ?? String.Empty;
				string sScrollY = sDesc.Length > 100 ? "overflow-y:scroll;" : "";
				string sVisibility = nItemNo < iLength-1 ? "galleryvisibile" : "galleryinvisible";

				string sIntro = "<div class='col-xl-4 " + sVisibility + "'>"
					+"<div id='c_3' class='card border shadow-0 mb-g shadow-sm-hover' data-filter-tags='nft_cool'>"
					+"<div class='d-flex flex-row align-items-center'>";
				sIntro += "<div class='card-body border-faded border-top-0 border-left-0 border-right-0 rounded-top'>";
				string sAsset = "";
				string sImg = "" + "/" + v.cover;
				if (v.url != null)
				{
					int nOctLen = GetOctetLength(v.cover, 0);
					if (nOctLen == 34)
					{
                        sImg = "" + "/" + v.cover;
                    }
                    else
					{
						sImg = "" + "/" + v.id + "/" + v.cover;
					}
                }

                nItemNo++;
				string sUrlToClick = "bbp/watchvideo?id=" + v.id.ToString();
				string sTrash = fAdmin ? GetStandardButton("btndeletevideo", "Delete", "video_delete", 
					"var e={};e.id='" + v.id + "';", String.Empty, "") : String.Empty;
				string sOutro = "</div></div></div>" + sTrash + "</div>";
				string sImage = "<img style='max-width:99%;height:300px;object-fit:fill;' src='" + sImg + "'/>";
				string sAnchor = "<a href='" + sUrlToClick + "'>";
				sAsset = sAnchor + sImage + "</a>";
				string sTextBody = "<div style='border=1px;height:75px;xwidth:340px;" + sScrollY + "'><font style='font-size:11px;'>"
						+ sDesc + "</font></div>";
				string s1 = sIntro + "<b>" + v.title + "</b><br>" + sAsset + sTextBody + sOutro;
				html += s1;
				nTotal++;
			}
			html += "</div>";
			if (nTotal == 0)
				html = "No Videos found.";
			return html;
		}

		public IActionResult VideoList()
        {
            ViewBag.VideoList = GetVideoList();
            return View();
        }
    }
}
