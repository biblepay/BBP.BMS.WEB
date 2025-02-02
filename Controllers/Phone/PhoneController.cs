using BBPAPI;
using BiblePay.BMS.DSQL;
using BiblePay.BMS.Extensions;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using static BMSCommon.Common;
using static BMSCommon.Extensions;


namespace BiblePay.BMS.Controllers
{
    public class PhoneController : Controller
    {
        public static string States = "Alabama, Alaska, Arizona, Arkansas, California, Colorado, Connecticut, Delaware, Florida, Georgia, Hawaii, Idaho, Illinois, Indiana, Iowa, Kansas, Kentucky, Louisiana, Maine, Maryland, Massachusetts, Michigan, Minnesota, Mississippi, Missouri, Montana, Nebraska, Nevada, New Hampshire, New Jersey, New Mexico, New York, North Carolina, North Dakota, Ohio, Oklahoma, Oregon, Pennsylvania, Rhode Island, South Carolina, South Dakota, Tennessee, Texas, Utah, Vermont, Virginia, Washington, West Virginia, Wisconsin, Wyoming";
        public static string StateAbbrev = "AL, AK, AZ, AR, CA, CO, CT, DE, FL, GA, HI, ID, IL, IN, IA, KS, KY, LA, ME, MD, MA, MI, MN, MS, MO, MT, NE, NV, NH, NJ, NM, NY, NC, ND, OH, OK, OR, PA, RI, SC, SD, TN, TX, UT, VT, VA, WA, WV, WI, WY";

        private JsonResult Navigate(string sURL)
        {
            var returnVal = new ServerToClient();
            var m = "location.href='" + sURL + "';";
            returnVal.returnbody = m;
            returnVal.returntype = "javascript";
            var o1 = JsonConvert.SerializeObject(returnVal);
            return Json(o1);
        }

        public static string GetSelect(List<DropDownItem> l, string sSelected, string sElementName, string sRowID)
        {
            string HTML = "<select data-parentid='" + sRowID + "' name='" + sElementName + "' id='" + sElementName + "'>";
            HTML += "<option value=''>CHOOSE</option>";
            for (int i = 0; i < l.Count; i++)
            {
                string sSel1 = l[i].key0 == sSelected ? "SELECTED" : "";
                string sOption = "<option value='" + l[i].key0 + "' " + sSel1 + ">" + l[i].text0 + "</option>\r\n";
                HTML += sOption;    
            }
            HTML += "</select>";
            return HTML;
        }

        public IActionResult PhoneMapMaintenance()
        {
            User u = HttpContext.GetCurrentUser();
            DataTable dt = new DataTable();
            List<DropDownItem> l = new List<DropDownItem>();
            string sHTML = "<table class='saved'><tr><th>Added<th>SIP UserName<th>Phone Number</tr>";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                // Add dropdown for Phone Number
                string sSIP = dr["username"].ToStr();
                sSIP = sSIP.Replace("@biblepay.robandrews.n2.voximplant.com", "");
                string sTR = "<TR><TD>" + dr["added"].ToStr() + "</TD><TD>" 
                    + sSIP + "</TD><TD>" + GetSelect(l,  dr["phonenumber"].ToStr(), "ddPhoneNumber", dr["id"].ToStr()) + "</TD></TR>";
                sHTML += sTR + "\r\n";
            }
            sHTML += "</table>";
            ViewBag.PhoneMapReport = sHTML;
            return View();
        }

        public IActionResult _SMSTab()
        {
            string data = GetTemplate("sms.htm");
            string ci = String.Empty;
            // Set up the chat header
            string sUID = HttpContext.Session.GetString("SMS_WITH");
            ViewBag.SMSWith = sUID == null ? "No One" : sUID;
            User dtUser = BBPAPI.Model.UserFunctions.GetCachedUser(IsTestNet(HttpContext), sUID);
            data = data.Replace("@FriendsName", ViewBag.SMSWith);
            DataTable dt = new DataTable();// BBPAPI.Interface.Repository.GetChats(IsTestNet(HttpContext));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sUserID = dt.Rows[i]["erc20address"].ToString();
                string contactitem = GetTemplate("contactlistitem.htm");
                contactitem = contactitem.Replace("@myname", dt.Rows[i]["nickname"].ToString());
                contactitem = contactitem.Replace("@uid", sUserID);
                bool fActive = BBPAPI.Model.UserFunctions.IsUserActive(false, sUserID);
                string sUserStatus = fActive ? "status-success" : "status-danger";
                string sUserStatusHR = fActive ? "Active" : "Off";
                contactitem = contactitem.Replace("@messengerstatus", sUserStatus); // status-success = active, status-danger=red, status=green, status-warning=yellow
                contactitem = contactitem.Replace("@status", sUserStatusHR);
                string sNickName = dt.Rows[i]["nickname"].ToString();
                contactitem = contactitem.Replace("@datafiltertag", sNickName.ToLower());

                string sAvatarURL = dt.Rows[i]["BioURL"].ToString();
                if (sAvatarURL == "")
                    sAvatarURL = "/img/demo/avatars/emptyavatar.png";

                contactitem = contactitem.Replace("@avatar", sAvatarURL);
                ci += contactitem + "\r\n";
            }
            data = data.Replace("@contactlistitems", ci);
            string sMsgs = BMS.DSQL.Chat.GetChatMessages(HttpContext, "SMS_WITH");
            data = data.Replace("@chatmessages", sMsgs);
            // SMS poll
            string js = "<script>setTimeout(`DoCallback('sms_poll','','../../phone/processdocallback');`, 5000);</script>";
            data += js;
            ViewBag.SMSInnerFrame = data;
            return View();
        }
        private JsonResult Javascript(string sJS)
        {
            var returnVal = new ServerToClient
            {
                returnbody = sJS,
                returntype = "javascript"
            };
            var o1 = JsonConvert.SerializeObject(returnVal);
            return Json(o1);
        }
        
        public string CallHistoryReport()
        {
            var pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
            var dt1 = BBPAPI.Interface.Phone.GetCallHistoryReport((long)GetDouble(pu.id));
            var sHTML = "<table width=100%><tr><th>Added<th>From Number<th>To Number<th>Duration<th>USD Chg<th>BBP Charge<th>TXID<th>Bill</tr>";
            for (var i = 0; i < dt1.Count; i++)
            {
                var dr = dt1[i];
                var sRow = "<tr><td>" + dr.added.ToMilitaryTime()
                                      + "<td>" + dr.fromnumber.ToString() 
                                      + "<td>" + dr.tonumber.ToString()
                                      + "<td>" + dr.duration.ToString()
                                      + "<td>" + dr.charge.ToCurrencyDollars()
                                      + "<td>" + dr.amountbbp.ToCurrencyBBP() 
                                      + "<td style='font-size:4px;'>" + dr.txid.ToString() + "<td>" 
                                      + dr.amountbilled.ToCurrencyBBP() 
                                      + "</tr>\r\n";
                sHTML += sRow;
            }
            sHTML += "</table>";
            return sHTML;
        }

        public IActionResult CallHistory()
        {
            ViewBag.CallHistoryReport = CallHistoryReport();
            return View();
        }

        public IActionResult VoicemailReview()
        {
            PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
            ViewBag.txtGreeting = pu.Greeting;
            ViewBag.txtDuration = pu.AnswerDuration;
            DataTable dt = new DataTable();// BBPAPI.Interface.Phone.GetVoiceMailReport(pu.UserId);
            string sHeader = "<table style='border-spacing: 10px;'><tr><th width=25%>Added<th>Caller ID<th>Duration<th width=50%>Play<th>Action</tr>";
            string HTML = String.Empty;
            HTML += sHeader;
            PaginatorController.PaginatorObject p = PaginatorController.MakePag(HttpContext, "phone/rates", dt.Rows.Count, 1999);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sCallerID = "N/A"; //todo, pull in callerid
                string sDuration = "N/A"; //todo pull duration
                string sPlayLink = dt.Rows[i]["URL"].ToStr();

                sPlayLink = sPlayLink.Replace("storage-gw-us-01.voximplant.com", "voicemail.biblepay.org");
                string sPlayControl = "<audio controls><source src=\"" + sPlayLink + "\"> </audio>";

                //string sPlayURL = "<a href='" + sPlayLink + "' target=_blank>Play</a>";
                string sDeleteButton = "<button class='btn xbtn-info xshadow-0 ml-auto btn-default' id='btnDV' "
                    + "onclick=\"var e={};e.id='" + dt.Rows[i]["id"].ToStr() + "';DoCallback('phone_deletevoicemail', "
                    + "e, '../../phone/processdocallback');\"><i class=\"fal fa-trash-alt\"></i></button>";

                string sTD = "<tr><td> " + dt.Rows[i]["Added"].ToString()
                    + "<td>" + sCallerID + "<td>" + sDuration + "<td>" + sPlayControl + "<td>" + sDeleteButton + "</td></tr>";
                
                if (p.IsRowVisible(i))
                {
                    HTML += sTD;
                }
            }
            HTML += "</table><br>";
            HTML += p.HTML;
            ViewBag.VoicemailReport = HTML;
            return View();
        }

        public IActionResult Rates()
        {
            double nBBPAmount = PricingService.ConvertUSDToBiblePayWithCache(.01);
            List<PhoneRate> dt = BBPAPI.Interface.Phone.GetRatesReport(nBBPAmount);
            ViewBag.RateSource = dt;
            ViewBag.ddDataSource = new string[] { "Top", "Bottom" };
            return View();
        }

        public IActionResult RateOverride()
        {
            PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
            DataTable dt = new DataTable();// BBPAPI.Interface.Phone.GetRateOverrideReport(pu.UserId);
            string sHeader = "<table><tr><th>Added<th width=20%>Country</tr>";
            string HTML = String.Empty;
            HTML += sHeader;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sTD = "<tr><td> " + dt.Rows[i]["Added"].ToString() + "<td>" + dt.Rows[i]["Country"].ToStr();

                HTML += sTD;
            }
            HTML += "</table><br>";
            ViewBag.RateOverrideReport = HTML;
            return View();
        }


        public IActionResult HardwareSettings()
        {
            PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
            if (pu == null)
            {
                ViewBag.SIPUserID = "Provision Softphone First";
                ViewBag.BBPAddress = "Provision Softphone First";
            }
            else
            {
                string sSuff = "@biblepay.robandrews.n2.voximplant.com";
                if (pu.WalletBalance > 10000 && false)
                {
                    ViewBag.SIPUserID = "" + pu.bbpaddress + sSuff + "";
                }
                else
                {
                    ViewBag.SIPUserID = pu.bbpaddress + "@provisionsuffix.provisiondomain.biblepay.org";
                }

                ViewBag.BBPAddress = pu.bbpaddress; 
            }


           
            return View();
        }

        public IActionResult PhoneService()
        {
            PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
            ViewBag.PhoneUser = pu;
            if (pu == null)
            {
                DSQL.UI.MsgBox(HttpContext, "Unauthorized", "Unauthorized", "Sorry, this node does not provide phone service", true);
                HttpContext.Response.Redirect("gospel/about");
                return null;
            }
            else
            {
                return View();
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public IActionResult Phone()
        {
            // TODO: Track the PID so we can kill, restart, minimize, unminimize, Ring, etc.
            string sHTML = "<html><body><script>window.moveTo(580,240);window.resizeTo(540,720);"
                           + "window.location='/phone/phoneservice"
                           + "';</script></body></html>";
            string sAppArgs = "--app=data:text/html," + sHTML;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ProcessDoCallback([FromBody] ClientToServer o)
        {
            User u0 = GetUser(HttpContext);
            ServerToClient returnVal = new ServerToClient();

            if (o.Action == "Phone_Ring")
            {
                //string sLogArea = GetFormData(o.FormData, "logarea");
                dynamic oNum = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sIncoming = oNum.number.Value;
                Log("CallerID::Inbound::" + sIncoming);
                return null;
            }
            else if (o.Action == "phone_addcountryoverride")
            {
                PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
                if (pu.WalletBalance < 1000)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Phone balance too low.  You can top up your balance by sending BBP to your Phone Address."));
                }
                string sCountry = GetFormData(o.FormData, "txtCountry");

                bool fValidCountry = true;
                if (!fValidCountry)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Rate Override Authorization",
                            "Sorry, you must select a valid Country."));

                }
                PhoneUserCountry pu1 = new PhoneUserCountry();
                pu1.UserId = pu.id;
                pu1.NewCountry = sCountry;
                bool fIns = BBPAPI.Interface.Phone.InsertCountryOverride(pu1);

                if (fIns)
                {
                    return Json(DSQL.UI.MsgBoxJson(HttpContext, "Success", "Success", "Added new country code"));
                }
                else
                {
                    return Json(DSQL.UI.MsgBoxJson(HttpContext, "Fail", "Fail", "Failed to add new country code"));
                }
            }
            else if (o.Action == "phone_addnumber")
            {
                // Make a new Phone User Here
                PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
                if (pu.WalletBalance < 1000)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Phone balance too low.  You can top up your balance by sending BBP to your Phone Address."));
                }

                if (GetDouble(pu.id) > 0 && pu.WalletBalance < 10000)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Phone balance too low to add an additional line.  You can top up your balance by sending BBP to your Phone Address.  Ensure balance is > 10K."));
                }
                string sPhoneNumber = GetFormData(o.FormData, "ddPhoneNum");
                int nRegionID = 0;
                nRegionID = (int)GetDouble(GetFormData(o.FormData, "ddRegion"));
                if (nRegionID == 0)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision",
                        "Sorry, you must select a valid phone region first."));
                }

                // Verify this User-BBP address is not in use already....
                long nNewUserID = 0;
                string sNewAddress = BBPAPI.Interface.Phone.GetPhoneUserNameBasedOnRecordCount(HttpContext.GetCurrentUser());
                try
                {
                    BBPAddressKey bpk = new BBPAddressKey();
                    bpk.Address = sNewAddress;
                    bpk.PrivateKey = pu.bbppk;
                    nNewUserID = BBPAPI.Interface.Phone.AddNewPhoneUser(bpk);
                    Log("AddNewPhoneUserRemoteEP::" + nNewUserID.ToString());
                }
                catch(Exception ex1)
                {
                    Log("ProvisionNewUser::" + ex1.Message);
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Failed during selection of username"));
                }
                // At this point save the phone number;
                NewPhoneUser npu = new NewPhoneUser();
                npu.NewUserID = nNewUserID;
                npu.Address = pu.bbpaddress;
                npu.Address = sNewAddress;
                npu.BBPPrivateKey = pu.bbppk;
                bool fSuccess = BBPAPI.Interface.Phone.InsertPhoneUser(npu);
                if (!fSuccess)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Failed during attachment of user ID."));
                }

                string sCountryState = GetFormData(o.FormData, "ddState");
                PhoneRegionCountryAddress npo = new PhoneRegionCountryAddress();
                npo.RegionID = nRegionID;
                npo.CountryState = sCountryState;
                npo.Address = pu.bbpaddress;
                sPhoneNumber = BBPAPI.Interface.Phone.BuyAndGetNewPhoneNumber(npo);
                if (sPhoneNumber == "FAIL" || sPhoneNumber == null || sPhoneNumber.Length < 10)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Failed during attachment of new phone number"));
                }

                // This one is OK
                NewPhoneUser npu1 = new NewPhoneUser();
                npu1.Address = pu.bbpaddress;
                npu1.NewUserID = nNewUserID;
                npu1.PhoneNumber = sPhoneNumber;

                bool f1000 = BBPAPI.Interface.Phone.SetPhoneUserPhoneNumber(npu1);
                if (!f1000)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Failed during creation of new phone number"));
                }
                // This one is OK
                bool f1002 = BBPAPI.Interface.Phone.SetPhoneRulesCreated(nNewUserID);
                if (!f1002)
                {
                    return Json(DSQL.UI.GetModalDialogJson("Provision", "Failed during setting of rules creation."));
                }
                string sResult = "Your BBP Phone has been created!  Your phone number is " + sPhoneNumber 
                    + ".  To use your phone simply click the Phone tab.  You can also launch the phone application from your BBP wallet (click Phone from the left menu).  Thank you for using BIBLEPAY and God Bless you!";
                string modal = DSQL.UI.GetModalDialogJson("Provision Phone Service", sResult, "location.href='phone/phonemapmaintenance';");
                return Json(modal);
            }
            else if (o.Action == "phone_changestate")
            {
                HttpContext.Session.SetString("ddState", GetFormData(o.FormData, "ddState"));
                HttpContext.Session.SetString("ddRegion", "");
                string js = "$('#partial-load-provision').load('/Phone/_Provision');";
                return Javascript(js);
            }
            else if (o.Action == "phone_changeregion")
            {
                HttpContext.Session.SetString("ddRegion", GetFormData(o.FormData, "ddRegion"));
                var js = "$('#partial-load-provision').load('/Phone/_Provision');";
                return Javascript(js);
            }
            else if (o.Action == "phone_deletevoicemail")
            {
                // Delete the voicemail
                var pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
                dynamic oVM = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sVoiceMailGuid = oVM.id;
                BBPAPI.Interface.Phone.DeleteVoiceMail(sVoiceMailGuid);
                string js = "window.location.href='phone/voicemailreview';";
                return Javascript(js);
            }
            else if (o.Action == "phone_update_greeting")
            {
                var pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
                string sNewGreeting = GetFormData(o.FormData, "txtGreeting");
                string sNewDuration = GetFormData(o.FormData, "txtDuration");
                VoiceGreeting vg = new VoiceGreeting();
                vg.UserID = pu.id;
                vg.Greeting = sNewGreeting;
                vg.Duration = sNewDuration.AsInt32();
                BBPAPI.Interface.Phone.SetVoicemailGreeting(vg);
                string js = "window.location.href='phone/voicemailreview';";
                return Javascript(js);  
            }
            else if (o.Action == "phone_saveextensionmappings")
            {
                TransformDOM t = new TransformDOM(o.FormData);

                foreach (string sParent in t.lParents)
                {
                    if (!String.IsNullOrEmpty(sParent))
                    {
                        string sPhoneNumber = t.GetDOMItem(sParent, "ddPhoneNumber").Value.ToStr();

                        if (!String.IsNullOrEmpty(sPhoneNumber))
                        {
                            PhoneUserMappingUpdate m = new PhoneUserMappingUpdate();
                            m.User = HttpContext.GetCurrentUser();
                            m.PhoneUser = sParent.AsInt32();
                            m.PhoneNumber = sPhoneNumber;
                            bool f11 = BBPAPI.Interface.Phone.UpdatePhoneUserMapping(m);
                        }

                        bool f1 = false;
                    }

                }
                string js = "window.location.href='phone/phonemapmaintenance';";
                return Javascript(js);
            }
            else
            {
                throw new Exception("Unknown Option");
            }
        }

        public async Task<ActionResult> AddPhoneNumber()
        {
            return View();
        }
        public async Task<ActionResult> Provision()
        {

            PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
            ViewBag.PhoneUser = pu;
            var ddRegions = new List<DropDownItem>();
            var ddPhoneNums = new List<DropDownItem>();
            var vStates = States.Split(",");
            var vStateAbbrev = StateAbbrev.Split(",");
            var sRegionID = HttpContext.Session.GetString("ddRegion") ?? String.Empty;
            var sState = HttpContext.Session.GetString("ddState") ?? String.Empty;
            if (String.IsNullOrEmpty(sState))
            {
                sState = "AL";
            }

            return View();
        }
    }
}

