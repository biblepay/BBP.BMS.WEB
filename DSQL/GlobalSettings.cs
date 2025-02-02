using BiblePay.BMS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static BMSCommon.Common;

namespace BiblePay.BMS
{
    public static class GlobalSettings
    {
        public static int BMS_VERSION = 1072;
        public static int MIN_BMS_VERSION = 1001;
        public static int DEFAULT_HTTPS_PORT = 8443;
        public static int DEFAULT_HTTP_PORT = 8080;
        public static bool fDebug = false;
        public static string BX_API = "https://chainz.cryptoid.info/bbp/api.dws?";
        public static string FoundationPublicKey = "BB2BwSbDCqCqNsfc7FgWFJn4sRgnUt4tsM";
        public static long METRIC_FILECOUNT = 0;
        public static long METRIC_SYNCED_COUNT = 0;
    }

}

// This namespace is used for the SSL certificate
namespace FluffySpoon
{
    public class X509
    {
        public static X509Certificate2 GetSSL()
        {
            try
            {
                string sPath = GetFolder("database", "fluffy.bytes");
                string sURL = "https://bbpipfs.s3.filebase.com/database/fluffy.bytes";
                MyWebClient wc = new MyWebClient();
                wc.DownloadFile(sURL, sPath);
                byte[] b = System.IO.File.ReadAllBytes(sPath);
                var cert = new X509Certificate2(b);
                return cert;
            }
            catch (Exception ex)
            {
                Log("GetSSL1::" + ex.Message);
                return null;
            }
        }
    }
}
