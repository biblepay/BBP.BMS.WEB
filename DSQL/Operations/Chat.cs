using BBPAPI;
using BBPAPI.Model;
using BMSShared;
using Microsoft.AspNetCore.Http;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using BMSCommon.Model;
using BiblePay.BMS.Extensions;
using Org.BouncyCastle.Security;

namespace BiblePay.BMS.DSQL
{
    public static class Chat
    {

        public static string GetChatMessages(HttpContext h, string sUIDKey)
        {
            // chat messages
            string sMsgs = String.Empty;
            User u0 = GetUser(h);
            string sMyId = u0.ERC20Address;
            // UIDKey = CHATTING_WITH or SMS_WITH
            string sChattingWithUID = h.Session.GetString(sUIDKey);
            if (sMyId == null || sChattingWithUID == null)
            {
                return String.Empty;
            }
            if (!chat_depersisted)
            {
                DepersistChatItems(h.GetCurrentUser(), IsTestNet(h));
            }
            if (dictChats.ContainsKey(sMyId))
            {
                for (int i = 0; i < dictChats[sMyId].chats.Count; i++)
                {
                    ChatItem cs = dictChats[sMyId].chats[i];
                    if (cs.From == sMyId && sChattingWithUID == cs.To)
                    {
                        string chatsent = GetTemplate("chatsent.htm");
                        chatsent = chatsent.Replace("@body", cs.body);
                        chatsent = chatsent.Replace("@time", cs.time.ToString());
                        sMsgs += chatsent + "\r\n";
                    }
                    else if (cs.To == sMyId && sChattingWithUID == cs.From)
                    {
                        string chatreply = GetTemplate("chatreply.htm");
                        chatreply = chatreply.Replace("@body", cs.body);
                        chatreply = chatreply.Replace("@time", cs.time.ToString());
                        sMsgs += chatreply + "\r\n";
                    }
                }
            }
            return sMsgs;
        }


        public static string GetSMSMessages(string sMe, string sChattingWith)
        {
            // chat messages
            string sMsgs = String.Empty;
            if (sChattingWith == null)
            {
                return "";
            }
            SMSMessage s1 = new SMSMessage();
            s1.From = sMe;
            s1.To = sChattingWith;

            DataTable dtChats = new DataTable();// BBPAPI.Interface.Phone.GetSMSMessages(s1);
            for (int i = dtChats.Rows.Count-1; i >= 0; i--)
            {
                //                    ChatItem cs = dictChats[sMyId].chats[i];
                DataRow dr = dtChats.Rows[i];
                string sTheRecip = dr["recipient"].ToString().Trim();
                string sTheSender = dr["sender"].ToString().Trim();

                if (dr["sender"].ToString() == sMe && dr["recipient"].ToString() == sChattingWith)
                {
                      string chatsent = GetTemplate("chatsent.htm");
                      chatsent = chatsent.Replace("@body", dr["body"].ToString());
                      chatsent = chatsent.Replace("@time", dr["Added"].ToString());
                      sMsgs += chatsent + "\r\n";
                }
                else if (dr["recipient"].ToString() == sMe && sChattingWith == dr["sender"].ToString())
                {
                        string chatreply = GetTemplate("chatreply.htm");
                        chatreply = chatreply.Replace("@body", dr["body"].ToString());
                        chatreply = chatreply.Replace("@time", dr["Added"].ToString());
                        sMsgs += chatreply + "\r\n";
                }
            }
            sMsgs = sMsgs.Replace("`", "");
            return sMsgs;
        }


        public class ChatSession
        {
            public List<ChatItem> chats = new List<BMSCommon.Model.ChatItem>();
        }

        public static Dictionary<string, ChatSession> dictChats = new Dictionary<string, ChatSession>();


        public static void AddChatItem(bool fTestNet, BMSCommon.Model.ChatItem ci, bool fPersist)
        {
            if (!dictChats.ContainsKey(ci.From))
            {
                ChatSession cs = new ChatSession();
                dictChats.Add(ci.From, cs);
            }
            if (!dictChats.ContainsKey(ci.To))
            {
                ChatSession cs1 = new ChatSession();
                dictChats.Add(ci.To, cs1);
            }
            dictChats[ci.From].chats.Add(ci);
            dictChats[ci.To].chats.Add(ci);
            string sURL = "/bbp/chat";
            if (fPersist)
            {
                ci.TestNet = fTestNet;
                ci.body = "You have received a chat message";
                ci.Type = "chat";
                ci.URL = sURL;
                BBPAPI.Interface.Repository.PersistChatItem(ci);
            }
        }


        public static void DepersistChatItems(User u, bool fTestNet)
        {
            DataTable dt = new DataTable();// BBPAPI.Interface.Repository.GetChats(fTestNet);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ChatItem ci = new ChatItem();
                ci.body = dt.Rows[i]["body"].ToString();
                ci.From = dt.Rows[i]["sender"].ToString();
                ci.To = dt.Rows[i]["recipient"].ToString();
                ci.time = Convert.ToDateTime(dt.Rows[i]["Added"]);
                AddChatItem(fTestNet, ci, false);
            }
            chat_depersisted = true;
        }

        public static bool chat_depersisted = false;
       

        public static string GetNotificationItem(string id, string avatarURL, string message, string fullname, DateTime dtTime, string status)
        {
            string data = GetTemplate("notificationrecord.htm");
            data = data.Replace("@id", id);
            data = data.Replace("@avatarURL", avatarURL);
            data = data.Replace("@message", message);
            data = data.Replace("@fullname", fullname);
            data = data.Replace("@status", status);
            string sRelativeTime = TimeUtility.GetRelativeTime(dtTime);
            data = data.Replace("@timerelative", sRelativeTime);
            return data;
        }


        public static string LevelToNotificationStatus(int i)
        {
            string status = "status-danger";
            if (i == 0)
                status = "status-warning";
            if (i == 1)
                status = "status-danger";
            if (i == 2)
                status = "statusinfo";
            if (i == 3)
                status = "status";
            return status;
        }

        public static string GetNotifications0(HttpContext h, string sUserID)
        {
            GetBusinessObject go = new GetBusinessObject();
            go.TestNet = IsTestNet(h);
            go.ParentID = sUserID;
            DataTable dt = new DataTable();// BBPAPI.Interface.Repository.GetNotifications(go);
            string html = String.Empty;
            if (dt.Rows.Count==0)
            {
                return String.Empty;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                User uRow = UserFunctions.GetCachedUser(IsTestNet(h), dt.Rows[i]["FromUser"].ToString());
                DateTime dtTime = Convert.ToDateTime(dt.Rows[i]["added"]);
                bool fActive = UserFunctions.IsUserActive(false, uRow.ERC20Address);
                string sStatus = fActive ? "status-success" : "status-danger";
                string sAnchor = "<a href='" + dt.Rows[i]["URL"].ToString() + "'>";
                string sFullMessage = sAnchor + dt.Rows[i]["body"].ToString() + "</a>";
                string row = GetNotificationItem(i.ToString(), uRow.BioURL,
                    sFullMessage, uRow.NickName, dtTime, sStatus);
                html += row + "\r\n";
            }
            double nCount = dt.Rows.Count;
            SetSessionDouble(h, "notificationcount", nCount);

            return html;
        }
        public static string GetNotificationCountHR(HttpContext h)
        {
            //You got 151 notifications
            double nCount = GetSessionDouble(h, "notificationcount");
            string sData = "You've got " + nCount.ToString() + " notification(s).";
            return sData;
        }

        public static int GetNotificationCount(HttpContext h)
        {
            int n1 = (int)GetSessionDouble(h, "notificationcount");
            return n1;
        }

    }
}
