using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BiblePay.BMS.DSQL
{


    public class MyWebClient : System.Net.WebClient
    {
        private int DEFAULT_TIMEOUT = 30000;
        public void SetTimeout(int iTimeout)
        {
            DEFAULT_TIMEOUT = iTimeout * 1000;
        }
        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            System.Net.WebRequest w = base.GetWebRequest(uri);
            w.Timeout = DEFAULT_TIMEOUT;
            return w;
        }
    }


    public static class Utility
    {

        public static bool ContainsReservedWord(string sData)
        {
            string data = sData.ToLower();
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            data = rgx.Replace(data, "");
            if (data.Contains("javascript") || data.Contains("script") || data.Contains("javas"))
            {
                return true;
            }
            return false;
        }

        public static string CleanseXSSAdvanced(string sData, bool fNeutralizeJS = false)
        {
            // Here is an evil one:
            // https://pay.org/PrayerBlog?entity=townhall111%22%20onpointermove=alert%28document.cookie%29%3e
            // Note how the attacker does not use the word javascript.. And he encodes the left and right parentheses; and the semicolon.
            // So we catch the word script in other places, but we dont catch this.  Lets remove the %01-%99

            // Note: Microsofts XSS method, htmlencode, does NOT protect against this!           
            sData = sData.Replace("document.", "");
            sData = sData.Replace(".domain", "");
            sData = sData.Replace(".cookie", "");
            sData = sData.Replace("javascript", "");
            sData = sData.Replace("%22", "");
            sData = sData.Replace("%20", "");
            sData = sData.Replace("%2528", "");
            sData = sData.Replace("%2529", "");
            sData = sData.Replace("alert", "");

            sData = sData.Replace("<", "");
            sData = sData.Replace(">", "");
            sData = sData.Replace("(", "{");
            sData = sData.Replace(")", "}");
            if (fNeutralizeJS)
            {
                sData = sData.Replace("$(", "");
                sData = sData.Replace("${", "");
                sData = sData.Replace("\"", "");
                sData = sData.Replace("'", "");
                sData = sData.Replace("`", "");
                sData = sData.Replace("{", "");
                sData = sData.Replace("}", "");
                sData = sData.Replace("onpointer", "");
            }
            sData = sData.Replace("script:", "");
            sData = sData.Replace("\\", "");
            sData = sData.Replace("%25", "");
            sData = sData.Replace("%28", "");
            sData = sData.Replace("%29", "");
            sData = sData.Replace("%3e", "");
            sData = sData.Replace("onmouse", "");
            sData = sData.Replace("onpointer", "");
            bool fCRW = ContainsReservedWord(sData);
            if (fCRW)
                return "blocked";
            return sData;
        }


        public static bool IsQuestionableNetworkAddress(string url)
        {
            try
            {
                Uri myUri = new Uri(url);
                string host = myUri.Host;
                bool fContainsNumbers = host.All(char.IsDigit);
                if (fContainsNumbers)
                {
                    return true;
                }

                IPHostEntry hostEntry;
                hostEntry = Dns.GetHostEntry(host);
                IPAddress[] ipv4Addresses = Array.FindAll(
                        Dns.GetHostEntry(host).AddressList,
                            a => a.AddressFamily == AddressFamily.InterNetwork);

                IPAddress[] ipv4MyAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                //DNS supports more than one record
                for (int i = 0; i < hostEntry.AddressList.Length; i++)
                {
                    bool fIsLoopback = IPAddress.IsLoopback(hostEntry.AddressList[i]);
                    if (fIsLoopback)
                    {
                        return true;
                    }

                }
                for (int i = 0; i < ipv4Addresses.Length; i++)
                {
                    bool fIsLoopback = IPAddress.IsLoopback(ipv4Addresses[i]);
                    if (fIsLoopback)
                    {
                        return true;
                    }
                    for (int j = 0; j < ipv4MyAddresses.Length; j++)
                    {
                        if (ipv4Addresses[i].ToString() == ipv4MyAddresses[j].ToString())
                            return true;
                    }

                }

            }
            catch (Exception)
            {
                return true;
            }
            return false;
        }



    }

    public static class TimeUtility
    {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;
        public static string GetRelativeTime(DateTime yourDate)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - yourDate.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
    }
}
