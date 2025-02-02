using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BiblePay.BMS.Controllers
{
	public class TimelineController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> UploadFileTimeline(List<IFormFile> file)
        {
            try
            {
                if (file.Count > 0)
                {
                    for (int i = 0; i < file.Count;)
                    {
                        string _FileName = Path.GetFileName(file[i].FileName);
                        bool fOK = DSQL.UI.IsAllowableExtension(_FileName);
                        if (fOK)
                        {
                            FileInfo fi = new FileInfo(_FileName);
                            string sGuid = Guid.NewGuid().ToString() + "" + fi.Extension;
                            string sDestFN = Path.Combine(Path.GetTempPath(), sGuid);
                            using (var stream = new FileStream(sDestFN, System.IO.FileMode.Create))
                            {
                                await file[i].CopyToAsync(stream);
                            }

                            string sStorePath = "upload/photos/" + sGuid;
                            Pin p = new Pin();
                            p = BBPAPI.Utilities.PinLogic.StoreFile(HttpContext.GetCurrentUser(), sDestFN, sStorePath, "");
                            ServerToClient returnVal = new ServerToClient();
                            returnVal.returnbody = "";
                            returnVal.returntype = "uploadsuccess";
                            returnVal.returnurl = p.URL;
                            string o1 = JsonConvert.SerializeObject(returnVal);

                            return Json(o1);
                        }
                        else
                        {
                            string modal = DSQL.UI.GetModalDialog("Save Timeline Image", "Extension not allowed");
                            BMSCommon.Model.ServerToClient returnVal = new ServerToClient();
                            returnVal.returntype = "modal";
                            returnVal.returnbody = modal;
                            string o1 = JsonConvert.SerializeObject(returnVal);
                            return Json(o1);
                        }
                    }
                }
                throw new Exception("no file");
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                throw;
            }
        }

        public IActionResult Timeline()
        {
            ViewBag.Timeline = DSQL.UI.GetTimelinePostDiv(HttpContext, "main");
            return View();
        }

    }
}
