using BBPAPI.Model;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using static BiblePay.BMS.DSQL.Chat;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;

namespace BiblePay.BMS.Controllers
{
    public class ChatController : Controller
    {

        [HttpPost]
        public JsonResult ProcessDoCallback([FromBody] ClientToServer o)
        {
            ServerToClient returnVal = new ServerToClient();
            User u0 = GetUser(HttpContext);

            if (o.Action == "chat_select")
            {
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sUID = a.id.Value;
                if (sUID == u0.ERC20Address)
                {
                    string s1 = MsgBoxJson(HttpContext, "Chat Error", "Error", "You cannot chat with yourself.");
                    return Json(s1);
                }
                HttpContext.Session.SetString("CHATTING_WITH", sUID);
                string m = "location.href='/chat/chat';";
                returnVal.returnbody = m;
                returnVal.returntype = "javascript";
                string o1 = JsonConvert.SerializeObject(returnVal);
                return Json(o1);
            }
            else if (o.Action == "Chat_Poll")
            {
                string sMsgs = BiblePay.BMS.DSQL.Chat.GetChatMessages(HttpContext, "CHATTING_WITH");
                string m = "var p = document.getElementById('chat_container');"
                    + "if (p.innerHTML != `" + sMsgs + "`) { p.innerHTML=`" + sMsgs + "`;p.scrollTop = p.scrollHeight; } setTimeout(`DoCallback('Chat_Poll','','chat/processdocallback')`,5000);";
                returnVal.returnbody = m;
                returnVal.returntype = "javascript";
                string o1 = JsonConvert.SerializeObject(returnVal);
                return Json(o1);
            }
            else if (o.Action == "Chat_Send")
            {
                if (!u0.LoggedIn)
                {
                    string s1 = MsgBoxJson(HttpContext, "Chat Error", "Error", "You must be logged in to chat.");
                    return Json(s1);
                }

                string sSend = GetFormData(o.FormData, "msgr_input");
                try
                {
                    if (sSend != String.Empty)
                    {
                        ChatItem ci = new ChatItem();
                        ci.From = u0.ERC20Address;
                        string sToUID = HttpContext.Session.GetString("CHATTING_WITH");
                        if (sToUID != null)
                        {
                            ci.To = sToUID;
                            ci.body = sSend;
                            ci.time = DateTime.Now;
                            AddChatItem(IsTestNet(HttpContext), ci, true);
                        }
                        else
                        {
                            string s1 = MsgBoxJson(HttpContext, "Chat Error", "Error", "You must choose someone to chat with first.");
                            return Json(s1);
                        }
                    }
                    string sMsgs = DSQL.Chat.GetChatMessages(HttpContext, "CHATTING_WITH");
                    string m = "var b=document.getElementById('msgr_input');b.value='';var p = document.getElementById('chat_container');"
                        + "p.innerHTML=`" + sMsgs + "`;p.scrollTop = p.scrollHeight;";
                    returnVal.returnbody = m;
                    returnVal.returntype = "javascript";
                    string o1 = JsonConvert.SerializeObject(returnVal);
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

        public IActionResult Chat()
        {
            string data = GetTemplate("chat.htm");
            string ci = String.Empty;
            // Set up the chat header
            string sUID = HttpContext.Session.GetString("CHATTING_WITH");
            User dtUser = UserFunctions.GetCachedUser(IsTestNet(HttpContext), sUID);
            if (dtUser != null)
            {
                data = data.Replace("@FriendsName", dtUser.NickName);
                string sFriendsAvatar = dtUser.BioURL;
                if (sFriendsAvatar == String.Empty)
                    sFriendsAvatar = "/img/demo/avatars/emptyavatar.png";
                data = data.Replace("@FriendsAvatar", sFriendsAvatar);
            }
            else
            {
                data = data.Replace("@FriendsName", "Choose someone to chat with from the right menu");
                data = data.Replace("FriendsAvatar", "/img/demo/avatars/emptyavatar.png");
            }
            DataTable dt = new DataTable();// BBPAPI.Interface.Repository.GetChats(IsTestNet(HttpContext));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sUserID = dt.Rows[i]["erc20address"].ToString();

                string contactitem = GetTemplate("contactlistitem.htm");
                contactitem = contactitem.Replace("@myname", dt.Rows[i]["nickname"].ToString());
                contactitem = contactitem.Replace("@uid", sUserID);
                bool fActive = UserFunctions.IsUserActive(false, sUserID);
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
            string sMsgs = BMS.DSQL.Chat.GetChatMessages(HttpContext, "CHATTING_WITH");
            data = data.Replace("@chatmessages", sMsgs);
            // Chat poll
            string js = "<script>setTimeout(`DoCallback('Chat_Poll','','chat/processdocallback');`, 5000);</script>";
            data += js;
            ViewBag.Chat = data;
            return View();
        }

    }
}
