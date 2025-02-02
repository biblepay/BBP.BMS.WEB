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
    public class TicketController : Controller
    {
        
        private static int GetMaxTicketNumber(User u)
        {
            List<Ticket> lTickets = BBPAPI.Interface.Repository.GetDatabaseObjects<Ticket>("ticket");
            if (lTickets.Count == 0) return 0;
            int r = lTickets.Max(a => a.TicketNumber);
            return r;
        }
        private static List<Ticket> GetTicketList(User u,string sUser, string sID)
        {
            List<Ticket> lTickets = BBPAPI.Interface.Repository.GetDatabaseObjects<Ticket>("ticket");
            if (sID == String.Empty)
            {
                lTickets = lTickets.Where(s => s.AssignedTo == sUser && s.Version > 1).ToList();
            }
            else
            {
                lTickets = lTickets.Where(s => s.id == sID).ToList();
            }
            foreach (Ticket t in lTickets)
            {
                t.BioURL = GetAvatarPictureWithName(false, t.ERC20Address);
                t.AssignedToBioURL = GetAvatarPictureWithName(false, t.AssignedTo);
            }
            return lTickets;
        }
        public static string GetTicketListReport(User u,string sUser, string sID)
        {
      
            string html = "<table class=saved>";
            html +=  "<tr><th>Ticket #<th>Name<th>Assigned To<th>Updated</tr>";
            List<Ticket> lTickets = GetTicketList(u,sUser, sID);
            lTickets = lTickets.Where(a => a.Disposition != "Closed").ToList();
            foreach (Ticket t in lTickets)
            {
                string sAnchor = "<a href='ticket/ticketview?id=" + t.id + "'>" + t.TicketNumber + "</a>";
                string sRow = "<tr><td>" + sAnchor
                        + "<td>" + t.Title
                        + "<td>" + t.AssignedToBioURL
                        + "<td>" + t.Updated.ToMilitaryTime() + "</tr>";
                html += sRow;
            }
            html += "</table>";
            return html;
        }

        public static string GetTicketHistory(User u,string sParentID)
        {
            string html = "<table class=saved width=100%>";
            html += "<tr><th>Assigned To<th>Updated<th>Disposition</tr>";
            List<TicketHistory> lTH = BBPAPI.Interface.Repository.GetDatabaseObjects<TicketHistory>("tickethistory","added desc");
                lTH = lTH.Where(s => s.ParentID == sParentID && s.AssignedTo != null).ToList();
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
            //ServerToClient returnVal = new ServerToClient();
            User u0 = GetUser(HttpContext);

            if (o.Action == "ticket_submit")
            {
                Ticket t = new Ticket();
                t.Added = DateTime.Now;
                t.Updated = DateTime.Now;
                t.AssignedTo = GetFormData(o.FormData, "ddAssignTo");
                t.Disposition = GetFormData(o.FormData, "ddDisposition");
                t.Description = GetFormData(o.FormData, "txtDescription");
                t.Title = GetFormData(o.FormData, "txtName");
                t.TicketNumber = GetMaxTicketNumber(HttpContext.GetCurrentUser()) + 1;
                t.ERC20Address = HttpContext.GetCurrentUser().ERC20Address;
                t.id = Guid.NewGuid().ToString();
                t.Version = 2;
                string sErr = String.Empty;
                if (t.Title == String.Empty)
                {
                    sErr = "Title must be populated.";
                }
                else if (t.Description == String.Empty)
                {
                    sErr = "Description must be populated.";
                }
                if (sErr != String.Empty)
                {
                    return Json(MsgBoxJson(HttpContext, "Ticket Error", "Ticket Error", sErr));
                }

                bool f20 = BBPAPI.Interface.Repository.StoreData<Ticket>("options.ticket", t, t.id);
                return this.ReturnURL("ticket/ticketlist");
            }
            else if (o.Action == "ticket_edit")
            {
                string sID = GetFormData(o.FormData, "txtID");
                List<Ticket> tList = GetTicketList(HttpContext.GetCurrentUser(),String.Empty, sID);
                if (tList.Count > 0)
                {
                    Ticket t = tList[0];
                    t.Updated = DateTime.Now;
                    t.AssignedTo = GetFormData(o.FormData, "ddAssignTo");
                    t.Disposition = GetFormData(o.FormData, "ddDisposition");
                    if (t.Disposition == String.Empty)
                    {
                        return Json(MsgBoxJson(HttpContext, "Error", "Error", "Disposition must be populated."));
                    }
                    else if (t.AssignedTo == String.Empty)
                    {
                        return Json(MsgBoxJson(HttpContext, "Error", "Error", "Assigned to must be populated."));

                    }
                    bool f19 = BBPAPI.Interface.Repository.StoreData<Ticket>("options.ticket", t, t.id);
                    TicketHistory th = new TicketHistory();
                    th.ParentID = t.id;
                    th.id = Guid.NewGuid().ToString();
                    th.Added = DateTime.Now;
                    th.AssignedTo = GetFormData(o.FormData, "ddAssignTo");
                    th.Disposition = GetFormData(o.FormData, "ddDisposition");
                    th.Updated = DateTime.Now;
                    th.Notes = GetFormData(o.FormData, "txtNotes");
                    bool f22 = BBPAPI.Interface.Repository.StoreData<TicketHistory>("options.tickethistory", th, th.id);
                }
                return this.ReturnURL("ticket/ticketlist");
            }

            else
            {
                throw new Exception("");
            }
        }

        
        public IActionResult TicketView()
        {
            ViewBag.TicketID = Request.Query["id"].ToString() ?? String.Empty;
            List<Ticket> l = GetTicketList(HttpContext.GetCurrentUser(),String.Empty, ViewBag.TicketID);
            if (l.Count == 0)
            {
                return this.ReturnURL("ticket/ticketlist");
            }

            TicketModel model = GetModel(l[0]);
            ViewBag.Ticket = l[0];
            ViewBag.TicketHistory = GetTicketHistory(HttpContext.GetCurrentUser(),ViewBag.TicketID);
            string sTicketID = ViewBag.TicketID;
            HttpContext.Session.SetString("TicketID", sTicketID);
            

            return View(model);
        }

        private TicketModel GetModel(BMSCommon.Model.Ticket t)
        {
            TicketModel model = new TicketModel();
            model.DispositionList.Add(new SelectListItem { Text = "Change Approval", Value = "ChangeApproval" });
            model.DispositionList.Add(new SelectListItem { Text = "Programming", Value = "Programming" });
            model.DispositionList.Add(new SelectListItem { Text = "Code Review", Value = "CodeReview" });
            model.DispositionList.Add(new SelectListItem { Text = "Release To Production", Value = "ReleaseToProduction" });
            model.DispositionList.Add(new SelectListItem { Text = "Customer Acceptance", Value = "CustomerAcceptance" });
            model.DispositionList.Add(new SelectListItem { Text = "Closed", Value = "Closed" });

            if (t.Disposition != null)
            {
                model.DispositionList.FirstOrDefault(c => c.Value == t.Disposition).Selected = true;
            }

            // Assign To
            TimeSpan ts = new TimeSpan(190, 1, 1, 1);
            DateTime dtLastSeen = DateTime.Now.Subtract(ts);
            List<SelectListItem> li = new List<SelectListItem>();
            List<User> l = BBPAPI.Interface.Repository.GetDatabaseObjects<User>("user");
            l = l.Where(a => a.LastSeen > dtLastSeen).ToList();

            foreach (User u in l)
            {
                if (!String.IsNullOrEmpty(u.NickName))
                {
                    SelectListItem li1 = new SelectListItem();
                    li1.Text = u.NickName;
                    li1.Value = u.ERC20Address;
                    li.Add(li1);
                }
            }
            model.AssignedToList = li;

            if (t.AssignedTo != null)
            model.AssignedToList.FirstOrDefault(c => c.Value == t.AssignedTo).Selected = true;
            return model;
        }

        public IActionResult TicketAdd()
        {
            TicketModel model = GetModel(new Ticket());
            return View(model);
        }

        public IActionResult TicketList()
        {
            var TicketList = GetTicketListReport(HttpContext.GetCurrentUser(), HttpContext.GetCurrentUser().ERC20Address, String.Empty);
            ViewBag.TicketList = TicketList;
            return View();
        }

    }
}
