using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;

namespace BiblePay.BMS.Controllers
{
	public class SMSController : Controller
    {
        private string GetChatData(User u,bool bSendingOne)
        {
            ServerToClient returnVal = new ServerToClient();
            PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(u);
            string sToUID = HttpContext.Session.GetString("SMS_WITH");
            string sMsgs = BiblePay.BMS.DSQL.Chat.GetSMSMessages(pu.PhoneNumber, sToUID);
            string sSnip = "var b=document.getElementById('msgr_input');b.value='';";
            string m = "";
            if (bSendingOne) m += sSnip;
            m += "var p = document.getElementById('chat_container');"
                + "if (p.innerHTML != `" + sMsgs + "`) { p.innerHTML=`" + sMsgs + "`;p.scrollTop = p.scrollHeight; } setTimeout(`DoCallback('sms_poll','','sms/processdocallback')`,5000);";
            returnVal.returnbody = m;
            returnVal.returntype = "javascript";
            string o1 = JsonConvert.SerializeObject(returnVal);
            return o1;
        }
        [HttpPost]
        public async Task<JsonResult> ProcessDoCallback([FromBody] ClientToServer o)
        {
            ServerToClient returnVal = new ServerToClient();
            User u0 = GetUser(HttpContext);
            if (o.Action == "sms_select")
            {
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sUID = a.id.Value;
                if (sUID == u0.ERC20Address)
                {
                    string s1 = MsgBoxJson(HttpContext, "Chat Error", "Error", "You cannot chat with yourself.");
                    return Json(s1);
                }
                HttpContext.Session.SetString("SMS_WITH", sUID);
                string m = "location.href='/sms/sms';";
                returnVal.returnbody = m;
                returnVal.returntype = "javascript";
                string o1 = JsonConvert.SerializeObject(returnVal);
                return Json(o1);
            }
            else if (o.Action == "sms_poll")
            {
                string o1 = GetChatData(HttpContext.GetCurrentUser(), false);
                return Json(o1);
            }
            else if (o.Action == "sms_send")
            {
                if (!u0.LoggedIn)
                {
                    string s1 = MsgBoxJson(HttpContext, "Chat Error", "Error", "You must be logged in to chat.");
                    return Json(s1);
                }

                PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
                string sToUID = HttpContext.Session.GetString("SMS_WITH");

                string sSend = GetFormData(o.FormData, "msgr_input");
                try
                {
                    if (sSend != String.Empty)
                    {
                        if (sToUID != null)
                        {
                            ChatItem ci = new ChatItem();
                            ci.From = pu.PhoneNumber;
                            ci.To = sToUID;
                            ci.body = sSend;
                            ci.time = DateTime.UtcNow;
                            BBPAPI.Interface.Repository.PersistChatItem(ci);
                            SMSMessage msg1 = new SMSMessage();
                            msg1.From = pu.PhoneNumber;
                            msg1.To = sToUID;
                            msg1.Message = sSend;   
                            long nSent = BBPAPI.Interface.Phone.SendSMS(msg1);
                        }
                        else
                        {
                            string s1 = MsgBoxJson(HttpContext, "Chat Error", "Error", "You must choose someone to chat with first.");
                            return Json(s1);
                        }
                    }

                    string o1 = GetChatData(HttpContext.GetCurrentUser(),true);
                    return Json(o1);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message);
                }

            }
            else
            {
                throw new Exception("Unknown method.");
            }
        }

        public IActionResult SMS()
        {
            string data = GetTemplate("sms.htm");
            string ci = String.Empty;
            // Set up the chat header
            string sUID = HttpContext.Session.GetString("SMS_WITH");
            User dtUser = BBPAPI.Model.UserFunctions.GetCachedUser(IsTestNet(HttpContext), sUID);
            if (sUID != null)
            {
                data = data.Replace("@FriendsName", sUID);
                string sFriendsAvatar = String.Empty;
                if (sFriendsAvatar == String.Empty)
                    sFriendsAvatar = "/img/demo/avatars/emptyavatar.png";
                data = data.Replace("@FriendsAvatar", sFriendsAvatar);
            }
            else
            {
                data = data.Replace("@FriendsName", "Choose someone to SMS from the right menu");
                data = data.Replace("FriendsAvatar", "/img/demo/avatars/emptyavatar.png");
            }
            DataTable dt = new DataTable();// BBPAPI.Interface.Phone.GetDistinctSMSUserPhoneNumbers(HttpContext.GetCurrentUser());

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sUserID = dt.Rows[i]["tonumber"].ToString();

                string contactitem = GetTemplate("smscontactlistitem.htm");
            
                string sNickName = dt.Rows[i]["tonumber"].ToString() ?? String.Empty;

                contactitem = contactitem.Replace("@myname", sNickName);
                contactitem = contactitem.Replace("@uid", sNickName);

                bool fActive = BBPAPI.Model.UserFunctions.IsUserActive(false, sUserID);
                string sUserStatus = fActive ? "status-success" : "status-danger";
                string sUserStatusHR = fActive ? "Active" : "Off";
                // Presence
                contactitem = contactitem.Replace("@messengerstatus", sUserStatus); // status-success = active, status-danger=red, status=green, status-warning=yellow
                contactitem = contactitem.Replace("@status", sUserStatusHR);
                contactitem = contactitem.Replace("@datafiltertag", sNickName.ToLower());

                string sAvatarURL = dt.Rows[i]["tonumber"].ToString();
                if (sAvatarURL == "")
                    sAvatarURL = "/img/demo/avatars/emptyavatar.png";

                contactitem = contactitem.Replace("@avatar", sAvatarURL);
                ci += contactitem + "\r\n";
            }
            data = data.Replace("@contactlistitems", ci);
            PhoneUser pu = BBPAPI.Interface.Phone.GetPhoneUser(HttpContext.GetCurrentUser());
            string sMsgs = BMS.DSQL.Chat.GetSMSMessages(pu.PhoneNumber, sUID);
            data = data.Replace("@chatmessages", sMsgs);
            // Chat poll
            string js = "<script>setTimeout(`DoCallback('sms_poll','','sms/processdocallback');`, 5000);</script>";
            data += js;
            ViewBag.SMSInnerFrame = data;
            return View();
        }
    }
}

