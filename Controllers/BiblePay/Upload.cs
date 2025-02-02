using BBPAPI;
using BiblePay.BMS.Extensions;
using BMSCommon;
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
    public class Upload : Controller
    {
        
        [HttpGet]
        public ActionResult UploadFile()
        {
            ViewBag.Message = "";

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> UploadFile(List<IFormFile> file, string txtTitle, string txtDescription)
        {

            if (txtTitle.Length < 5 || txtDescription.Length < 5)
            {
                ViewBag.Message = "Title and Description must be signifigant.";
                return View();
            }

            if (file.Count > 0)
            {
                for (int i = 0; i < file.Count; i++)
                {
                    string _FileName = Path.GetFileName(file[i].FileName);
                    bool fOK = DSQL.UI.IsAllowableExtension(_FileName);
                    string sPubKey = HttpContext.GetCurrentUser().BBPAddress;// Encryption.GetPubKeyFromPrivKey(HttpContext.GetCurrentUser().BBPPrivKeyMainNet, false);

                    if (fOK)
                    {
                        FileInfo fi = new FileInfo(_FileName);
                        string sGuid = Guid.NewGuid().ToString() + "" + fi.Extension;
                        string sDestFN = Path.Combine(Path.GetTempPath(), sGuid);
                        string sStorePath = sPubKey + "/video/" + sGuid;

                        string sDestStorageFN = DSQL.UI.ReqPathToFilePath(sStorePath);


                        using (var stream = new FileStream(sDestStorageFN, System.IO.FileMode.Create))
                        {
                                await file[i].CopyToAsync(stream);
                        }

                        string sCoverFolder = DSQL.UI.ReqPathToFilePath(sPubKey + "/video");
                        string sCover =  FFMpegUtils.GetOneFrameFromMp4UsingFFMPEG(sDestStorageFN, sCoverFolder);
                        FileInfo fi1 = new FileInfo(sCover);
                        string sStorePathCover = sPubKey + "/video/" + fi1.Name;
                        Pin p = new Pin();
                        p  = BBPAPI.Utilities.PinLogic.StoreFile(HttpContext.GetCurrentUser(), sDestStorageFN, sStorePath, "");
                        Pin pCover = new Pin();
                        Pin rCover = BBPAPI.Utilities.PinLogic.StoreFile(HttpContext.GetCurrentUser(), sCover, sStorePathCover, "");
                        // Now we go from the Storj file to the Video
                        Video v1 = new Video();
                        v1.Title = txtTitle;
                        v1.Description = txtDescription;
                        v1.Added = DateTime.Now;
                        string sBBPAddress = HttpContext.GetCurrentUser().BBPAddress;
                        v1.BBPAddress = sBBPAddress;
                        v1.Source = sGuid;
                        v1.Cover = sStorePathCover;
                        v1.id = Guid.NewGuid().ToString();
                        BBPAPI.Interface.Repository.SaveVideo(v1);
                        ServerToClient returnVal = new ServerToClient();
                        returnVal.returnbody = "";
                        returnVal.returntype = "uploadsuccess";
                        returnVal.returnurl = p.URL;
                        string o1 = JsonConvert.SerializeObject(returnVal);
                        return RedirectToAction("videolist", "bbp");
                    }
                 }
                 ViewBag.Message = "Sent " + file[0].FileName + " successfully";
                 return View();
            }
            return View();
        }

    }
}
