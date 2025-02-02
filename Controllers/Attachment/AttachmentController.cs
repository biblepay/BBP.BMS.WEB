using BBPAPI;
using BBPAPI.Model;
using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BiblePay.BMS.Controllers
{
    public class AttachmentController : Controller
    {

        public class FreshUpload
        {
            public JsonResult NotAllowedError { get; set; }
            public string BareFileName { get; set; }
            public string FullFileName { get; set; }
            public FileInfo FileInfo { get; set; }
            public string TempFileName { get; set; }
            public string FullDiskFileName { get; set; }
            public string StorjDestination { get; set; }
            public FreshUpload(Controller c, IFormFile fil)
            {
				BareFileName = Path.GetFileName(fil.FileName);
                FullFileName = fil.FileName;
				NotAllowedError = IsAllowableExtensionModal(c, BareFileName);
				FileInfo fi = new FileInfo(FullFileName);
				TempFileName = Guid.NewGuid().ToString() + fi.Extension;
				FullDiskFileName = Path.Combine(Path.GetTempPath(),TempFileName);
				using (var stream = new FileStream(FullDiskFileName, System.IO.FileMode.Create))
				{
                    fil.CopyTo(stream);
				}
                StorjDestination = "upload/photos/" + TempFileName;
			}
		}

        public static JsonResult IsAllowableExtensionModal(Controller c, string sPath)
        {
            JsonResult o1 = null;
            bool f = DSQL.UI.IsAllowableExtension(sPath);
            if (f)
            {
                return o1;
            }
            string modal = DSQL.UI.GetModalDialog("Save Attachment", "Extension not allowed");
      		ServerToClient returnVal = new ServerToClient();
	        returnVal.returntype = "modal";
            returnVal.returnbody = modal;
            string s1 = JsonConvert.SerializeObject(returnVal);
            return c.Json(s1);
    	}


        [HttpPost]
        public async Task<IActionResult> UploadFileAttachment(List<IFormFile> file)
        {
            string sParentID = Request.Query["parentid"].ToString() ?? String.Empty;
            if (file.Count < 1)
            {
				throw new Exception("no file");
            }
			try
			{
				for (int i = 0; i < file.Count; i++)
                {
                    FreshUpload fresh = new FreshUpload(this, file[i]);
                    if (fresh.NotAllowedError != null)
                        return fresh.NotAllowedError;
                    Attachment a = new Attachment();
                    a.id = Guid.NewGuid().ToString();
                    a.FileName = fresh.BareFileName;
                    a.Version = 2;
                    UploadFileObject ufo = new UploadFileObject();
                    ufo.SourceFilePath = fresh.FullDiskFileName;
                    ufo.StorjDestinationPath = fresh.StorjDestination;
                    ufo.OverriddenBBPPrivateKey = HttpContext.GetCurrentUser().BBPPrivKeyMainNet;
                    UploadFileResult ufoOut = BBPAPI.Interface.PinLogic.UploadFile(ufo).Result;
                    a.URL = ufoOut.URL;
                    a.ParentID = sParentID;
                    // Store the attachment object
                    BBPAPI.Interface.Repository.StoreAttachment(a);
                    ServerToClient returnVal = new ServerToClient();
                    returnVal.returnbody = String.Empty;
                    returnVal.returntype = "uploadsuccessredirect";
                    returnVal.returnurl = a.URL;
                    string o1 = JsonConvert.SerializeObject(returnVal);
                    return Json(o1);
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!!";
                throw (ex);
            }
            throw new Exception("No files");
        }


        public static string GetAttachmentGallery(User u, string sParentID)
        {
            string html = "<div class='fs-lg fw-300 p-5 bg-white border-faded rounded mb-g'><div class='row js-list-filter' id='al1'>";
            List<Attachment> l = BBPAPI.Interface.Repository.GetDatabaseObjects<Attachment>("attachment");
            l = l.Where(s => s.Version >= 2 && s.ParentID == sParentID).ToList();
            for (int i = 0; i < l.Count; i++)
            {
                string sURL = "" + "/wwwroot/" +   l[i].URL;
                string sBgImg = "" + "/wwwroot/" + l[i].URL; 

                string sItem = "<div class='col-xl-4'><div class='card border shadow-0 mb-g shadow-sm-hover' style='min-height:201px;'>"
                          + "<a _target=blank href='" + sURL + "' class='text-center px-3 py-4 d-flex position-relative height-10 border'>"
                     + "   <img style='width:200px;height:200px;' src='" + sBgImg + "'/>"
                     + "   </a> </div></div>\r\n";
                html += sItem;
            }
            html += "</div></div>";
            return html;
        }
        public IActionResult AttachmentList()
        {
            string sParentID = Request.Query["parentid"].ToString() ?? String.Empty;
            ViewBag.Attachments = GetAttachmentGallery(HttpContext.GetCurrentUser(),sParentID);
            return View();
        }
    }
}
