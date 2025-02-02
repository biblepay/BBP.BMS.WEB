using BBPAPI.Model;
using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.Chat;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using static BMSCommon.Common;

namespace BiblePay.BMS
{
    public class BWS
    {
        public class ViewBagActionFilter : ActionFilterAttribute
        {
            // GLOBAL HTTPCONTEXT ACCESSOR
            public override void OnResultExecuting(ResultExecutingContext context)
            {
                // for razor pages
                if (context.Controller is PageModel)
                {
                    var controller = context.Controller as PageModel;
                    controller.ViewData.Add("b1", $"~/avatar/empty.png");
                }
                // for Razor Views
                if (context.Controller is Controller)
                {
                    var controller = context.Controller as Controller;
                    controller.ViewBag.HttpContext = controller.HttpContext;
                    controller.ViewBag.Chain = GetChain(controller.HttpContext);
                    controller.ViewBag.ChainColor = GetChainColor(controller.HttpContext);
                    controller.ViewBag.LoginStatus = GetLogInStatus(controller.HttpContext);
                    controller.ViewBag.BioURL = GetBioURL(controller.HttpContext);
                    controller.ViewBag.Balance = GetAvatarBalance(controller.HttpContext, false);
                    controller.ViewBag.NotificationCountHR = GetNotificationCountHR(controller.HttpContext);
                    controller.ViewBag.NotificationCount = GetNotificationCount(controller.HttpContext);
                    controller.ViewBag.LoginAction = GetLogInAction(controller.HttpContext);
                    controller.ViewBag.NickName = GetNickName(controller.HttpContext);
                    User u = controller.HttpContext.GetCurrentUser();
                    if (u != null)
                    {
                        controller.ViewBag.Notifications = GetNotifications0(controller.HttpContext, u.ERC20Address);
                        controller.ViewBag.LoggedInColor = "background-color:Lime";
                    }
                    if (u.BBPAddress == null)
                    {
                        controller.ViewBag.LoggedInColor = "";
                    }
                    // You have access to the httpcontext & route in controller.HttpContext & controller.RouteData
                }
                base.OnResultExecuting(context);
            }
        }



        public class HalfordFileProvider
        {
            private readonly RequestDelegate _next;
            private readonly IFileProvider fileProvider;

            public HalfordFileProvider(RequestDelegate next, IFileProvider fileProvider)
            {
                this._next = next;
                this.fileProvider = fileProvider;
            }

            public static string StripLeading(string Data, int iLeadingChars)
            {
                if (iLeadingChars > Data.Length)
                    return Data;
                string sOut = Data.Substring(iLeadingChars - 1, Data.Length - iLeadingChars + 1);
                return sOut;
            }

            public static string ReqPathToFilePath(string ContextRequestPath)
            {
                string sOrigReqPath = ContextRequestPath;
                string sReqPath = StripLeading(sOrigReqPath, 2);
                if (IsWindows())
                {
                    sReqPath = sReqPath.Replace("/", "\\");
                }
                string Sourcepath = Path.Combine(GetFolder(""), sReqPath);

                System.IO.FileInfo fi = new FileInfo(Sourcepath);
                if (!System.IO.Directory.Exists(fi.Directory.FullName))
                {
                    System.IO.Directory.CreateDirectory(fi.Directory.FullName);
                }
                return Sourcepath;
            }

            public FileInfo GetFileInfo(string ContextRequestPath)
            {
                System.IO.FileInfo fi = new FileInfo(ContextRequestPath);
                if (!fi.Exists)
                {
                    // This is BiblePay DSQL 404 page:
                    Log("Cant find file " + ContextRequestPath + " from orig req path");
                    return null;
                }
                else
                {
                    // This is static content
                    return fi;
                }
            }

            // BiblePay Decentralized Web Server
            public async Task Invoke(HttpContext context)
            {
                try
                {
                    // Reserved: When we resolve CDN nickname to URI:
                    string sourcepath = context.Request.Path;
                    if (sourcepath.Contains("/BMS/StaticVideoPlayer"))
                    {
                        string sSource = context.Request.Query["id"];
                        string sPage = "https://zglobalcdn.biblepay.org:8443/video/staticplayer/bbp/staticplayer.htm";
                        string html = ExecuteMVCCommand(sPage);
                        string sBindURL = String.Empty;
                        string sURL = sBindURL + "/video/" + sSource + "/1.m3u8";
                        html = html.Replace("{thesource}", sURL);
                        await context.Response.WriteAsync(html);
                        return;
                    }
                    else if (sourcepath.Contains("/BMS/GetDirectoryContents"))
                    {
                        try
                        {
                            string sKey = context.Request.Query["key"].ToString();
                            string sNickName = String.Empty;
                            string sUID = "0"; // .Service.ValidateKey(sKey, out sNN);
                            if (sUID == "0")
                            {
                                throw new Exception("API Key invalid.  To obtain a key, go to unchained.biblepay.org | Wallet.");
                            }
                            List<string> s = new List<string>();
                            string sJson3 = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                            await context.Response.WriteAsync(sJson3);
                        }
                        catch (Exception ex)
                        {
                            Log("gdi" + ex.Message);
                            await context.Response.WriteAsync("ERROR");
                        }
                    }
                    else if (sourcepath.Contains("/BMS/ValidateKey"))
                    {
                        try
                        {
                            string sKey = context.Request.Query["key"].ToString();
                            string sNickName = String.Empty;
                            string sUID = "0";// BBPess.Service.ValidateKey(sKey, out sNN);
                            if (sUID == "0")
                            {
                                throw new Exception("API Key invalid.  To obtain a key, go to unchained.biblepay.org | Wallet.");
                            }
                            var u = new UnchainedReply();
                            u.UserID = sUID;
                            string sJson3 = Newtonsoft.Json.JsonConvert.SerializeObject(u);
                            await context.Response.WriteAsync(sJson3);
                        }
                        catch (Exception ex)
                        {
                            Log("ValidateKey" + ex.Message);
                            await context.Response.WriteAsync("ERROR");
                        }

                    }
                    else if (sourcepath.Contains("/BMS/PlayVideos"))
                    {
                        string sFolder = GetFolder("video");

                        DirectoryInfo d = new DirectoryInfo(sFolder);
                        DirectoryInfo[] dis = d.GetDirectories();
                        string sBindURL = "";
                        string sHTML = "<html><h3>Sanctuary Videos</h3><br><br>";
                        foreach (DirectoryInfo di in dis)
                        {
                            string sSubFolder = di.Name;
                            if (sSubFolder.Length > 8)
                            {
                                string sMainFile = di.FullName + "/1.m3u8";
                                if (System.IO.File.Exists(sMainFile))
                                {
                                    string sID = sSubFolder;
                                    string sURL = sBindURL + "/BMS/StaticVideoPlayer?id=" + sID;
                                    string sRow = "<a href='" + sURL + "'>Play " + sID + "</a><br>\r\n";
                                    sHTML += sRow;
                                }
                            }
                        }
                        sHTML += "</html>";
                        await context.Response.WriteAsync(sHTML);
                        return;
                    }
                    else if (sourcepath.Contains("/video") || sourcepath.Contains("/upload/") || sourcepath.Contains("/wwwroot"))
                    {
                        // Case 1: A web resource
                        string sTP1 = ReqPathToFilePath(sourcepath);
                        FileInfo fi = GetFileInfo(sTP1);

                        if (fi == null)
                        {
                            double nConfigType = 1;
                            if (nConfigType == 1)
                            {
                                // Use storj instead
                                sourcepath = System.Web.HttpUtility.UrlDecode(sourcepath);
                                string sSourceKey = sourcepath;
                                string sTP = ReqPathToFilePath(sourcepath);
                                bool fSucc = await BBPAPI.StorjIOReadOnly.StorjDownloadLg(sSourceKey, sTP);
                                fi = GetFileInfo(sTP);
                            }

                            else if (nConfigType == 2)
                            {
                                sourcepath = System.Web.HttpUtility.UrlDecode(sourcepath);
                                string sSourceKey = sourcepath;// "BB2bDCqCqNsfc7FgWFJn4sRgnUt4tsM" + sourcepath;
                                string sTP = ReqPathToFilePath(sourcepath);
                                Stream s = await BBPAPI.StorjIOReadOnly.StorjDownloadStream(sSourceKey);

                                context.Response.ContentType = "video/mp4";
                                await s.CopyToAsync(context.Response.Body);
                                return;

                            }

                            if (fi != null)
                            {
                                await context.Response.SendFileAsync(fi.FullName);
                            }
                            string sTargetPath = ReqPathToFilePath(sourcepath);
                            return;
                        }
                        else
                        {
                            // File actually exists locally:
                            await context.Response.SendFileAsync(fi.FullName);
                        }
                    }
                    else if (sourcepath == "/")
                    {
                        // The web root
                        context.Response.Redirect("/gospel/about");
                    }
                    else if (sourcepath.Contains("/favicon.ico"))
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }
                    else
                    {
                        // Reserved for Reverse DNS lookups:
                        // string webPath = GetWebPathByReverseDNS(sourcepath);
                        // bool fExists = System.IO.File.Exists(webPath);
                        // if (webPath == String.Empty || !fExists)
                        // webPath = GetFolder(DEFAULT_PORT, "images1", "404.jpg");
                        // await context.Response.SendFileAsync(webPath);
                    }
                }
                catch (Exception ex)
                {
                    // cancelled task?
                    Log("BWS::WebServer::" + ex.Message);
                }
            }
        }
    }

}