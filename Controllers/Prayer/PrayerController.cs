using BiblePay.BMS.Extensions;
using BiblePay.BMS.Models;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.ControllerExtensions;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using static BMSCommon.Extensions;

namespace BiblePay.BMS.Controllers
{
    public class PrayerController : Controller
    {
        

        private static List<Prayer> GetPrayerList(User u,string sUser, string sID)
        {
            List<Prayer> lPrayers = BBPAPI.Interface.Repository.GetDatabaseObjects<Prayer>("prayer");
            lPrayers = lPrayers.Where(a => a.Deleted == 0).ToList();

            foreach (Prayer p in lPrayers)
            {
                p.BioURL = GetAvatarPictureWithName(false, p.BBPAddress);
            }
            return lPrayers;
        }
        public static string GetPrayerListReport(User u, string sUser, string sID)
        {
      
            string html = "<table class=saved>";
            html +=  "<tr><th>Prayer ID<th>Subject<th width=25%>Created By<th>Added</tr>";
            List<Prayer> lPrayers = GetPrayerList(u,sUser, sID);
            foreach (Prayer p in lPrayers)
            {
                string sAnchor = "<a href='prayer/prayerview?id=" + p.id + "'>View</a>";
                string sRow = "<tr><td>" + sAnchor
                        + "<td>" + p.Subject
                        + "<td>" + p.BioURL
                        + "<td>" + p.Added.ToMilitaryTime() + "</tr>";
                html += sRow;
            }
            html += "</table>";
            return html;
        }

        public static string GetTicketHistory(User u, string sParentID)
        {
            string html = "<table class=saved width=100%>";
            html += "<tr><th>Assigned To<th>Updated<th>Disposition</tr>";
            List<TicketHistory> lTH = BBPAPI.Interface.Repository.GetDatabaseObjects<TicketHistory>("tickethistory","added desc");
                lTH = lTH.Where(s => s.ParentID == sParentID && s.AssignedTo != null && s.AssignedTo != String.Empty).ToList();
            foreach (TicketHistory th in lTH)
            {
                th.AssignedToBioURL = GetAvatarPictureWithName(false, th.AssignedTo);
                string sRow = "<tr><td>" + th.AssignedToBioURL
                        + "<td>" + th.Added.ToMilitaryTime()
                        + "<td>" + th.Disposition + "</tr>";
                sRow += "<tr><td colspan=3> " + th.Notes + "</td></tr>";
                html += sRow;
            }
            html += "</table>";
            return html;
        }

        public class CustomResult : IActionResult
        {
            private readonly string _errorMessage;
            private readonly int _statusCode;
            private readonly string _postedFileName;
            public CustomResult(string errorMessage, int statusCode, string sPostedFileName)
            {
                _errorMessage = errorMessage;
                _statusCode = statusCode;
                _postedFileName = sPostedFileName;
            }

            public async Task ExecuteResultAsync(ActionContext context)
            {
                var objectResult = new ObjectResult(_errorMessage)
                {
                    StatusCode = _statusCode
                };
                context.HttpContext.Response.Headers.Add("name", _postedFileName);
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                string sURL = "https://forum.biblepay.org/180180.png";
                context.HttpContext.Response.Headers.Add("url", sURL);
                await objectResult.ExecuteResultAsync(context);
            }
        }


        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveMyFiles(List<IFormFile> UploadFiles)
        {
            try
            {
                var httpPostedFile = UploadFiles[0];
                if (httpPostedFile != null)
                {
                    string sReferrer = Request.Headers["REFERER"].ToStr() + "<eof>";
                    string sID = Common.ExtractXML(sReferrer, "id=", "<eof>");
                    string sSavePath = "upload/tickets/" + sID + "/" + httpPostedFile.FileName;
                    string sGuid = Guid.NewGuid().ToString() + Path.GetExtension(httpPostedFile.FileName);
                    string sTempPath = Path.GetTempPath();
                    sSavePath = "upload/tickets/" + sID + "/" + sGuid;
                    string sFullTempFileName = Path.Combine(sTempPath, sGuid);
                    using (var stream = new FileStream(sFullTempFileName, System.IO.FileMode.Create))
                    {
                        await httpPostedFile.CopyToAsync(stream);
                    }
                    Pin p = new Pin();
                    p = BBPAPI.Utilities.PinLogic.StoreFile(HttpContext.GetCurrentUser(), sFullTempFileName, sSavePath, "TICKET");
                    Response.Headers.Add("name", sGuid);
                    return Ok("File uploaded");
                }
                else
                {
                    return Ok(404);
                }
            }
            catch (Exception e)
            {
                return Ok(204);
            }
            return Ok(204);
        }



        [HttpPost]
        public JsonResult ProcessDoCallback([FromBody] ClientToServer o)
        {
            User u0 = GetUser(HttpContext);
            if (o.Action == "submit")
            {
                Prayer p = new Prayer();
                p.Added = DateTime.Now;
                p.Disposition = GetFormData(o.FormData, "ddDisposition");
                p.Body = GetFormData(o.FormData, "txtBody");
                p.Subject = GetFormData(o.FormData, "txtSubject");
                p.BBPAddress = HttpContext.GetCurrentUser().BBPAddress;
                p.id = Guid.NewGuid().ToString();
                p.Deleted = 0;
                string sErr = String.Empty;
                if (p.Subject == String.Empty)
                {
                    sErr = "Subject must be populated.";
                }
                else if (p.Body == String.Empty)
                {
                    sErr = "Description must be populated.";
                }
                if (sErr != String.Empty)
                {
                    return Json(MsgBoxJson(HttpContext, "Error", "Error", sErr));
                }

                p.UserId = HttpContext.GetCurrentUser().id;

                bool f20 = BBPAPI.Interface.Repository.StoreData<Prayer>("prayer", p, p.id);
                return this.ReturnURL("prayer/prayerlist");
            }
            else if (o.Action == "prayer_edit")
            {
                string sID = GetFormData(o.FormData, "txtID");
                List<Prayer> tList = GetPrayerList(HttpContext.GetCurrentUser(), String.Empty, sID);
                if (tList.Count > 0)
                {
                    Prayer t = tList[0];
                    t.Disposition = GetFormData(o.FormData, "ddDisposition");
                    if (t.Disposition == String.Empty)
                    {
                        return Json(MsgBoxJson(HttpContext, "Error", "Error", "Disposition must be populated."));
                    }
                    bool f19 = BBPAPI.Interface.Repository.StoreData<Prayer>("prayer", t, t.id);
                }
                return this.ReturnURL("prayer/prayerlist");
            }
            else if (o.Action=="prayer_delete")
            {
                string sID = GetFormData(o.FormData, "txtID");
                List<Prayer> tList = GetPrayerList(HttpContext.GetCurrentUser(), String.Empty, sID);
                tList = tList.Where(a => a.id == sID).ToList();
                if (tList.Count > 0)
                {

                    Prayer t = tList[0];
                    if (t.UserId == HttpContext.GetCurrentUser().id || t.UserId=="")
                    {
                        t.Deleted = 1;

                        bool f19 = BBPAPI.Interface.Repository.StoreData<Prayer>("prayer", t, t.id);
                    }
                }
                return this.ReturnURL("prayer/prayerlist");

            }
            else if (o.Action == "prayer_addcomment")
            {
                string sID = GetFormData(o.FormData, "txtID");
                List<Prayer> tList = GetPrayerList(HttpContext.GetCurrentUser(), String.Empty, sID);
                TicketHistory th = new TicketHistory();
                th.ParentID = sID;
                th.AssignedTo = HttpContext.GetCurrentUser().id;
                th.id = Guid.NewGuid().ToString();
                th.Added = DateTime.Now;
                th.Disposition = GetFormData(o.FormData, "ddDisposition");
                th.Updated = DateTime.Now;
                th.Notes = GetFormData(o.FormData, "txtNotes");
                bool f22 = BBPAPI.Interface.Repository.StoreData<TicketHistory>("options.tickethistory", th, th.id);
                return this.ReturnURL("prayer/prayerlist");
            }

            else
            {
                throw new Exception("");
            }
        }

        
        public IActionResult PrayerView()
        {
            ViewBag.PrayerID = Request.Query["id"].ToString() ?? String.Empty;
            List<Prayer> l = GetPrayerList(HttpContext.GetCurrentUser(), String.Empty, ViewBag.PrayerID);
            l = l.Where(a => a.id == ViewBag.PrayerID).ToList();

            if (l.Count == 0)
            {
                return this.ReturnURL("prayer/prayerlist");
            }
            PrayerModel model = GetModel(l[0]);
            ViewBag.Prayer = l[0];

            ViewBag.Comments = GetTicketHistory(HttpContext.GetCurrentUser(),ViewBag.PrayerID);
            string sID = ViewBag.PrayerID;
            HttpContext.Session.SetString("PrayerID", sID);
            return View(model);
        }

        private PrayerModel GetModel(BMSCommon.Model.Prayer p)
        {
            PrayerModel model = new PrayerModel();
            model.DispositionList.Add(new SelectListItem { Text = "Sick", Value = "Sick" });
            model.DispositionList.Add(new SelectListItem { Text = "Healed", Value = "Healed" });
            model.DispositionList.Add(new SelectListItem { Text = "Unknown", Value = "Unknown" });
            if (p.Disposition != null)
            {
                model.DispositionList.FirstOrDefault(c => c.Value == p.Disposition).Selected = true;
            }

            // Assign To
            return model;
        }

        public IActionResult PrayerAdd()
        {
            PrayerModel model = GetModel(new Prayer());
            return View(model);
        }

        public IActionResult PrayerList()
        {
            ViewBag.PrayerList = GetPrayerListReport(HttpContext.GetCurrentUser(),HttpContext.GetCurrentUser().BBPAddress, String.Empty);
            return View();
        }
    }
}

