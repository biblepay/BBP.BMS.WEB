using BBPAPI;
using BBPAPI.Model;
using BiblePay.BMS.Extensions;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.Utility;
using static BMSCommon.Common;

namespace BiblePay.BMS.DSQL
{
    public static class ControllerExtensions
    {
        
        public static string GetFormValue(this Controller controller, string key)
        {
            string sData = String.Empty;
            ClientToServer cts = controller.HttpContext.Session.GetObject<ClientToServer>("formdata");
            if (cts != null)
            {
                sData = DOMItem.GetFormData(cts.FormData, key);
            }
            return sData;
        }

        public static RedirectResult MsgBox(this Controller controller, string sTitle, string sHeading, string sBody)
        {
            controller.HttpContext.Session.SetString("msgbox_title", sTitle);
            controller.HttpContext.Session.SetString("msgbox_heading", sHeading);
            controller.HttpContext.Session.SetString("msgbox_body", sBody);
            return controller.Redirect("../bbp/messagepage");
        }

        public static JsonResult RenderDivToClient(this Controller controller, ClientToServer o, string sDiv, string sMyData)
        {
            ServerToClient returnVal = new ServerToClient();
            returnVal.returnbody = "$('#" + sDiv + "').html(`" + sMyData + "`);";
            returnVal.returntype = "javascript";
            string o1 = JsonConvert.SerializeObject(returnVal);
            return controller.Json(o1);
        }

		public static JsonResult RedirectToSection(this Controller controller, ClientToServer o, string sDiv, string sHandler)
        {
            controller.HttpContext.Session.SetObject("formdata", o);
            ServerToClient returnVal = new ServerToClient();
            returnVal.returnbody = "$('#" + sDiv + "').load('" + sHandler + "');";
            returnVal.returntype = "javascript";
            string o1 = JsonConvert.SerializeObject(returnVal);
            return controller.Json(o1);
        }

        public static string RedirectToSectionJS(this Controller controller, string sDiv, string sHandler)
        {
            string sData = "$('#" + sDiv + "').load('" + sHandler + "');";
            return sData;
        }
        
        public static JsonResult ShowModalDialog(this Controller controller, ClientToServer o, string sTitle, string sValue, string sOptJS)
        {
            controller.HttpContext.Session.SetObject("formdata", o);
            ServerToClient returnVal = new ServerToClient();
            string modal = DSQL.UI.GetModalDialog(sTitle, sValue, sOptJS);
            returnVal.returnbody = modal;
            returnVal.returntype = "modal";
            string outdata = JsonConvert.SerializeObject(returnVal);
            return controller.Json(outdata);
        }
        
        public static JsonResult ReturnJS(this Controller controller, string sScript)
        {
            ServerToClient returnVal = new ServerToClient();
            returnVal.returnbody = sScript;
            returnVal.returntype = "javascript";
            string o1 = JsonConvert.SerializeObject(returnVal);
            return controller.Json(o1);
            //return returnVal;
        }
        public static JsonResult ReturnURL(this Controller controller, string sURL)
        {
            string sJava = "location.href='" + sURL + "';";
            return ReturnJS(controller, sJava);
        }

    }

    public static class UI
    {

        public static async Task<string> WebPageScraper(string url)
        {
            try
            {
                // This is used for Timeline posts to build a URL preview of a pasted link
                url = HttpUtility.UrlDecode(url);
                ScrapingBrowser browser = new ScrapingBrowser();
                WebPage page;
                bool fShady = IsQuestionableNetworkAddress(url);
                bool fHTTProtocols = false;
                if (url.Contains("https://") || url.Contains("http://"))
                {
                    fHTTProtocols = true;
                }
                if (url.Contains("127.0.0.1") || url.Contains("localhost") || url.Contains("//127") || url.Contains("local"))
                {
                    url = "";
                }
                if (fShady || !fHTTProtocols)
                {
                    url = "";
                }
                string webUrl = url;
                page = await browser.NavigateToPageAsync(new Uri(webUrl));

                var title = page.Html.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", string.Empty);
                if (string.IsNullOrEmpty(title))
                    title = page.Html.SelectSingleNode("//title")?.InnerText;

                var description = page.Html.SelectSingleNode("//meta[@property='og:description']")?.GetAttributeValue("content", string.Empty);
                if (string.IsNullOrEmpty(description))
                    description = page.Html.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", string.Empty);

                var image = page.Html.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", string.Empty);
                if (string.IsNullOrEmpty(image))
                    image = page.Html.SelectNodes("//img").FirstOrDefault().GetAttributeValue("src", string.Empty);

                string sDiv = "<div>Title: " + title + "<br>Description: " + description + "<br><img src='" + image + "' /></div>";
                return sDiv;
            }
            catch(Exception)
            {
                return String.Empty;
            }
        }
        
        public static void MsgBox(HttpContext h, string sTitle, string sHeading, string sBody, bool fRedirect)
        {
            h.Session.SetString("msgbox_title", sTitle);
            h.Session.SetString("msgbox_heading", sHeading);
            h.Session.SetString("msgbox_body", sBody);
            if (fRedirect)
                h.Response.Redirect("../bbp/messagepage");
        }
        
        public static string FormatUSD(double myNumber)
        {
            var s = string.Format("{0:0.00}", Math.Round(myNumber,2));
            return s;
        }

        public static string FormatCurrency(double nMyNumber)
        {
            var s = string.Format("{0:0.0000000000}", nMyNumber);
            return s;
        }

        public static string GetBioURL(HttpContext h)
        {
            User u = h.GetCurrentUser();
            
            string sBIO = GetBioURL(u==null ? String.Empty : u.BioURL);
            return sBIO;
        }
        public static string GetBioURL(string URL)
        {
            string empty = "/img/demo/avatars/emptyavatar.png";
            if (URL == null || URL == "")
                return empty;
            return URL;
        }
        
        public static bool IsAllowableExtension(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            if (ext.Length < 1) return false;
            ext = ext.Substring(1, ext.Length - 1).ToLower();
            string allowed = "jpg;jpeg;gif;bmp;png;mp4";
            string[] vallowed = allowed.Split(";");
            for (int i = 0; i < vallowed.Length; i++)
            {
                if (vallowed[i] == ext)
                    return true;
            }
            return false;
        }
        public static string ListToHTMLSelect(List<DropDownItem> s, string sSelected)
        {
            string html = String.Empty;
            for (int i = 0; i < s.Count; i++)
            {
                DropDownItem di = s[i];
                string narr = String.Empty;
                if (s[i].key0.ToLower() == sSelected.ToLower())
                    narr = " SELECTED";
                string row = "<option " + narr + " value='" + s[i].key0 + "'>" + s[i].text0 + "</option>";
                html += row + "\r\n";
            }
            return html;
        }

        public static string GetStandardButton(string sID, string sCaption, string sEvent, string sArgs, string sConfirmNarrative, string sDestURL)
        {
            string sConfirmNarr = String.Empty;
            if (sConfirmNarrative != String.Empty)
            {
                sConfirmNarr = "var bConfirm=confirm('" + sConfirmNarrative + "');if (!bConfirm) return false;";
            }
            string sButton = "<button class='btn-default xbtn xbtn-info xbtn-block' id='" + sID 
                + "' onclick=\"" + sConfirmNarr + sArgs + "DoCallback('" + sEvent + "',e,'" + sDestURL + "');\" >" + sCaption + "</button>";
            return sButton;
        }

        public static string GetAvatarBalance(HttpContext h, bool fEraseCache)
        {

            User u = h.GetCurrentUser();
            double nBal = BBPAPI.Interface.Repository.QueryAddressBalance(false, u.BBPAddress);
            return FormatUSD(nBal);
        }

        public static string GetAvatarPictureWithName(bool fTestNet, string sUserID)
        {
            User u1 = UserFunctions.GetCachedUser(fTestNet, sUserID);
            string sBioURL = String.Empty;
            string sNickName = String.Empty;
            if (u1 == null)
            {
                sBioURL = "/img/demo/avatars/emptyavatar.png";
                sNickName = "Guest";
            }
            else
            {
                sBioURL = u1.BioURL;
                sNickName = u1.NickName ?? String.Empty;
            }
            string sAvatar = "<span class='profile-image-md rounded-circle d-block' style=\"background-image:url('" + sBioURL + "'); "
                + "background-size: cover;\"></span><span>" + sNickName + "</span> ";
            return sAvatar;
        }

        public static string GetAvatarPicture(bool fTestNet, string sUserID)
        {
            User u1 = UserFunctions.GetCachedUser(fTestNet, sUserID);
            string sBioURL = String.Empty;
            if (u1 == null || String.IsNullOrEmpty(u1.BioURL))
            {
                sBioURL = "/img/demo/avatars/emptyavatar.png";
            }
            else
            {
                sBioURL = u1.BioURL;
            }
            string sAvatar = "<span class='profile-image-md rounded-circle d-block' style=\"background-image:url('" + sBioURL + "'); "
                + "background-size: cover;\"></span> ";
            return sAvatar;
        }
        public static double GetAvatarBalanceNumeric(HttpContext h, bool fEraseCache)
        {
            User u = h.GetCurrentUser();
            if (u == null)
            {
                return 0;
            }
            long nElapsed = (long)(UnixTimestamp() - GetSessionDouble(h, "lastbalancecheck"));
            string sChain = GetChain(h);

            UserFunctions.SetLastUserActivity(IsTestNet(h), u.ERC20Address ?? String.Empty);
            if (nElapsed < 60*1 && !fEraseCache)
            {
               double nNewBal = GetDouble(h.Session.GetString(sChain + "_balance"));
               BMSCommon.Common.Log("(1)AVATAR_BALANCE::" + nElapsed.ToString() + "," + nNewBal.ToString());
               return nNewBal;
            }
            double nBal = BBPAPI.Interface.Repository.QueryAddressBalance(IsTestNet(h), u.BBPAddress);
            if (nBal == 0)
                nBal = -1;
            SetSessionDouble(h, "lastbalancecheck", UnixTimestamp());
            h.Session.SetString(sChain + "_balance", nBal.ToString());
            return nBal;
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
            string sReqPath = sOrigReqPath;
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
            if (IsWindows())
            {
                Sourcepath = Sourcepath.Replace("\\\\", "\\");
            }
            return Sourcepath;
        }



        public static string GetTemplate(string sName)
        {
            string sLoc = Path.Combine(Global.msContentRootPath, "wwwroot/templates/" + sName);
            string data = System.IO.File.ReadAllText(sLoc);
            return data;
        }

        public static string GetModalDialog(string title, string body, string optjs="")
        {
            string data = GetTemplate("modal.htm");
            data = data.Replace("@title", title);
            data = data.Replace("@body", body);
            data = data.Replace("@modalid", "modalid1");
            data = data.Replace("@optjs", optjs);
            return data;
        }

        public static string GetTimelinePostDiv(HttpContext h, string sParentID)
        {
            string data = GetTemplate("timeline.htm");
            // This is the reply to dialog, hence we replace with the active user:
            data = data.Replace("@BioURL",GetBioURL(h));
            data = data.Replace("@parentid", sParentID);
            // Append the posts, one by one from all who posted on this thread.
            GetBusinessObject bo = new GetBusinessObject();
            bo.TestNet = IsTestNet(h);
            bo.ParentID = sParentID;
            List<Timeline> l = BBPAPI.Interface.Repository.GetTimeLine(bo);
            for (int i = 0; i < l.Count; i++)
            {
                User uRow = UserFunctions.GetCachedUser(IsTestNet(h), l[i].ERC20Address);
                if (uRow != null)
                {
                    string entry = GetTemplate("timelinepost.htm");
                    entry = entry.Replace("@BioURL", uRow.BioURL);
                    entry = entry.Replace("@VALUE", l[i].Body);
                    entry = entry.Replace("@divPaste", l[i].dataPaste);
                    data += "\r\n" + entry;
                }
            }
            return data;
        }

        public static string GetModalDialogJson(string title, string body, string optjs="")
        {
            ServerToClient returnVal = new ServerToClient();
            string modal = DSQL.UI.GetModalDialog(title, body, optjs);
            returnVal.returnbody = modal;
            returnVal.returntype = "modal";
            string outdata = JsonConvert.SerializeObject(returnVal);
            return outdata;
        }

        public static string MsgBoxJson(HttpContext h, string sTitle, string sHeading, string sBody)
        {
            MsgBox(h, sTitle, sHeading, sBody, false);
            ServerToClient returnVal = new ServerToClient();
            string m = "location.href='/bbp/messagepage';";
            returnVal.returnbody = m;
            returnVal.returntype = "javascript";
            string o1 = JsonConvert.SerializeObject(returnVal);
            return o1;
        }
        public static string GetAccordian(string id, string title, string body)
        {
            string data = GetTemplate("accordian.htm");
            data = data.Replace("@title", title);
            data = data.Replace("@body", body);
            data = data.Replace("@id", id);
            return data;
        }

        public static string GetBasicTable(string tableid, string title)
        {
            string data = GetTemplate("basictable.htm");
            data = data.Replace("@tableid", tableid);
            data = data.Replace("@tablename", title);
            string th = "<th>Col1</th><th>Col2</th><th>Col3</th>";
            string tr = String.Empty;
            for (int i = 0; i < 50; i++)
            {
                string row = "<tr><td>Val " + i.ToString() + "</td><td>col2 " + i.ToString() + "</td><td>value 3</td></tr>";
                tr += row + "\r\n";
            }
            data = data.Replace("@tableheaders", th);
            data = data.Replace("@tablerows", tr);
            return data;
        }

        public static string GetChartOfGenericDataTradingView(List<QuantChart> l, string sChartType)
        {
            DateTime dtOldTime = new DateTime();
            string[] sDataSet = new string[10];
            for (int ch = 0; ch < l.Count; ch++)
            {
                for (int i = 0; i < l[ch].Chart.Count; i++)
                {
                     QuantChartItem dp = l[ch].Chart[i];
                     DateTime dtchart = dp.date.AddDays(0);
                     string sRow = "{ time: '" + dtchart.ToString("yyyy-MM-dd") + "', value: " + (dp.value).ToString() + " },";
                     string sStick = "{ time: '" + dtchart.ToString("yyyy-MM-dd") + "', open: " + dp.value.ToString() + ", high: " + dp.value.ToString() + ", low: " + dp.value.ToString() + ", close: " + (dp.value+1).ToString() + "},";
                     string sActive = sChartType == "candlestick" ? sStick : sRow;
                     if (dtOldTime == dtchart)
                     {
                     }
                        else
                     {
                           sDataSet[ch] += sActive;
                     }
                     dtOldTime = dtchart;
                }
            }
            for (int i = 0; i < l.Count; i++)
            {
                sDataSet[i] = sDataSet[i].Substring(0, sDataSet[i].Length - 1);
            }
            string sGuid = Guid.NewGuid().ToString();
            string sJSName = (sChartType == "candlestick") ? "chart_tradingview_candle.htm" : "chart_tradingview_area.htm";
            string sTemplate = GetTemplate(sJSName);

            sTemplate = sTemplate.Replace("seriesdata0", sDataSet[0]);
            sTemplate = sTemplate.Replace("@chartid0", "chart" + sGuid);
            for (int i = 1; i < l.Count; i++)
            {
                string sSeriesNew = "chart.addLineSeries({color:'rgba(4,111,232,1)',lineWidth:2,}).setData([" + sDataSet[i] + "]);";
                sTemplate = sTemplate.Replace("seriesdata" + i.ToString(), sSeriesNew);
            }
            return sTemplate;
        }
    }
}
