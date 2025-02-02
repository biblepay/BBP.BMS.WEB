using BiblePay.BMS.DSQL;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UIWallet;
using static BMSCommon.Common;
using static BMSCommon.Encryption;
using static BMSCommon.Model.BitcoinSyncModel;

namespace BiblePay.BMS.Controllers
{
	public class BMSController : Controller
    { 

        [Route("BMS/GetBlockCount")]
        public int GetBlockCount()
        {
            int n = 0;// BMSCommon.BitcoinSync.GetBestHeight(IsTestNet(HttpContext));
            return n;
        }

        [Route("BMS/GetPriceQuote")]
        public string GetPriceQuote()
        {
            string sPair = Request.Query["pair"];
            PriceQuote q = new PriceQuote();
            double dPrice = BBPAPI.Interface.Core.GetPriceQuote(sPair);
            q.Price = dPrice.ToString("0." + new string('#', 339));
            q.XML = "<MIDPOINT>" + q.Price + "</MIDPOINT><EOF>";
            String sJson = Newtonsoft.Json.JsonConvert.SerializeObject(q, Newtonsoft.Json.Formatting.Indented);
            return sJson;
        }

        [Route("BMS/GetRoute")]
        public string GetRoute()
        {
			string sDest = Request.Query["destination"];
			string sCallerid = Request.Query["callerid"];
            string sResult = BBPAPI.Interface.Phone.GetRoute(sDest, sCallerid);
            return sResult;
		}

		[Route("BMS/MobileAPI")]
        public string MobileAPI()
        {
            var m = new MobileAPI1
            {
                BTCUSD = BBPAPI.Interface.Core.GetPriceQuote("BTC/USD")
            };
            double nBBPBTC = BBPAPI.Interface.Core.GetPriceQuote("BBP/BTC");
            m.BBPUSD = m.BTCUSD * nBBPBTC;
            m.BBPBTC = nBBPBTC.ToString("0." + new string('#', 339));
            String sJson = Newtonsoft.Json.JsonConvert.SerializeObject(m);
            return sJson;
        }

        [Route("BMS/GetMemPoolCount")]
        public int GetMemPoolCount()
        {
            int n = Global.GetMemPoolCount();

            return n;
        }


        [Route("BMS/Supply")]
        public string Supply()
        {
            // this endpoint shows bbp circulating supply
            SupplyType s = BBPAPI.Interface.WebRPC.GetSupply(false).Result;
            string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(s, Newtonsoft.Json.Formatting.Indented);
            return sJson;
        }

        [Route("BMS/GetVideoList")]
        public string GetVideoList()
        {
            GetBusinessObject bo = new GetBusinessObject();
            bo.TestNet = false;
            List<VideoNew> l = BBPAPI.Interface.Repository.GetVideos(bo);
            string sCmd = Request.Headers["Action"].ToString();
            string sBBPPubKey = Request.Headers["unchained-public-key"].ToString() ?? String.Empty;
            string sBBPSig = Request.Headers["unchained-auth-signature"].ToString() ?? String.Empty;
            var fSuc = BBPAPI.Interface.Repository.VerifySignature(false, sBBPPubKey, "authenticate", sBBPSig);
            BMSCommon.Common.Log("x1cmd " + sCmd + ", pubkey " + sBBPPubKey + ", " + sBBPSig + ", " + fSuc.ToString());
            string sResponse = Newtonsoft.Json.JsonConvert.SerializeObject(l, Newtonsoft.Json.Formatting.Indented);
            string sFullResponse = "<json>" + sResponse + "</json>";
            return sFullResponse;
        }



        [Route("BMS/GetStorageBalance")]
        public async Task<string> GetStorageBalance()
        {
             string sBBPAddress = Request.Headers["Action"].ToString();
             string sAlt = Request.Query["bbp"].ToString() ?? String.Empty;
             if (sAlt != String.Empty)
             {
                 sBBPAddress = sAlt;
             }
             if (sBBPAddress==String.Empty)
             {
                return "Invalid BBP address.";
             }
             string sData = "";
             // TODO::FIX::await StorjIO.GetHistoricalUsage(BBPAPI.Globals._DBUser2,sBBPAddress);
             sData += "\r\n<EOF>\r\n";
             return sData;
        }

        [Route("BMS/GetBillingHistory")]
        public async Task<string> GetBillingHistory()
        {
            string sBBPAddress = Request.Headers["Action"].ToString();
            string sAlt = Request.Query["bbp"].ToString() ?? String.Empty;
            if (sAlt != String.Empty)
            {
                sBBPAddress = sAlt;
            }
            if (sBBPAddress == String.Empty)
            {
                return "Invalid BBP address.";
            }
            string sHistorical = "";
            // TODO::FIX:: await StorjIO.GetHistoricalCharges(BBPAPI.Globals._DBUser2, sBBPAddress);

			string sData = "<payload>" + sHistorical + "</payload>\r\n<EOF>\r\n";
            return sData;
        }

        [Route("BMS/GenerateToken")]
        public async Task<string> GenerateToken()
        {
            // This is a proof of concept endpoint where we generate a storj Custom "macaroon"
            // based on the BBP keypair, which gives access to a specific storj subdirectory.
            // TODO: SUPPLY BBP PRIV KEY, GET ACCESS TOKEN
            var sAction = Request.Headers["Action"].ToString();
			BBPNetAddress a = new BBPNetAddress();
            a.TestNet = false;
            a.Address = sAction;
			bool fValid = await BBPAPI.Interface.WebRPC.ValidateBBPAddress(a);
            if (!fValid)
            {
                string sResult1 = "<error>Invalid biblepay address.</error><EOF>\r\n";
                return sResult1;
            }
            // Generate a Read/Write key based on the subdirectory.
            string sSancName = Guid.NewGuid().ToString();
            BBPKeyPair k = await GetKeyPair3(false, sSancName);
            string sAccess = "";
            string sResult = "<bbppubkey>" + k.PubKey + "</bbppubkey><bbpprivkey>" 
                             + k.PrivKey + "</bbpprivkey><access>" 
                    + sAccess + "</access>\r\n\r\n<EOF>\r\n";
            return sResult;
        }
        

        private async static Task<bool> DownloadFileAsync(HttpClient client, string filename, string url)
        {
            if (System.IO.File.Exists(filename))
                return true;
            try
            {
                CreateDir(filename);
                var contents = await client.GetByteArrayAsync(url);
                System.IO.File.WriteAllBytes(filename, contents);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> GetBody()
        {
            var body = string.Empty;
            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                body = await stream.ReadToEndAsync();
            }

            return body;
        }


		[HttpPost]
		[Route("BMS/Voicemail")]
		public async Task<string> ProcessVoicemail()
		{
            PhoneCallerDestination pcd = new PhoneCallerDestination();
			pcd.CallerID = Request.Query["callerid"].ToString() ?? String.Empty;
            pcd.Destination = Request.Query["destination"].ToString() ?? String.Empty;
			pcd.Body = await GetBody();
            string s1 = BBPAPI.Interface.Phone.ProcessVoiceMailLD(pcd);
            return s1;
		}


		[Route("BMS/dialpad_webevent")]
        public async Task<string> dialpad_webevent()
        {
            var body = await GetBody();
            BBPAPI.Interface.Phone.ProcessWebHookLD(body); 
            return "OK";
        }


        [Route("BMS/Status")]
        public string Status()
        {
            string sBindURL = GetConfigKeyValue("bindurl");
            StatusObject s = new StatusObject();
            double nCalc = GlobalSettings.METRIC_SYNCED_COUNT / (GlobalSettings.METRIC_FILECOUNT + .01);
            s.Synced_Count = GlobalSettings.METRIC_SYNCED_COUNT;
            s.File_Count = GlobalSettings.METRIC_FILECOUNT;
            s.Synced_Percent = nCalc;
            s.BMS_VERSION = GlobalSettings.BMS_VERSION;

            
            s.Memory_Pool_Count = Global.dMemoryPool.Count;
            s.Status = "SUFFICIENT";
            s.URL = sBindURL;
            s.EOF = "<EOF>";
            string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(s, Newtonsoft.Json.Formatting.Indented);
            return sJson;
        }

        [Route("BMS/Scratch/{id}")]
        public string Scratch()
        {
            string id = Request.RouteValues["id"].ToString() ?? "";
            string value = "";
            string sOut = "<scratch>" + value + "</scratch>\r\n<EOF>\r\n";
            return sOut;
        }


        [Route("BMS/POSE")]
        public string POSE()
        {
            var sPOSE = string.Empty;
            sPOSE += "\r\n|v1.0|Status: SUFFICIENT\n|\r\n<EOF>\n";
            return sPOSE;
        }

        
        private static string GetIP(HttpContext h)
        {
            if (h.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIps))
            {
                string sIP = forwardedIps.First();
                return sIP;
            }
            var remoteIpAddress = h.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            return remoteIpAddress;

        }


        [Route("BMS/SAMLLogin")]
        public string SAMLLogin()
        {
			string sKey = IsTestNet(HttpContext) ? "tUser" : "User";
			string s1 = Request.Query["key"].ToString() ?? String.Empty;
			string sPrivKey = BMSCommon.Encryption.FromHexString(s1);
			string sPubKey = BBPAPI.Interface.Repository.GetPublicKeyFromPrivateKey(false,sPrivKey);
            if (String.IsNullOrEmpty(sPubKey)||String.IsNullOrEmpty(s1))
            {
                return "501";
            }
            User u = BBPAPI.Model.UserFunctions.GetAndCacheUser(sPrivKey,sPubKey);
            if (u != null)
            {
                u.LoggedIn = true;
                HttpContext.Session.SetObject(sKey, u);
                Response.Redirect("../gospel/about");
            }
            return "502";
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


        [Route("BMS/WatchVideo")]
        public async Task<ActionResult> Video1()
        {
            string sVideo = Request.Query["Video"];
            string sTP1 = DSQL.UI.ReqPathToFilePath(sVideo);
            FileInfo fi = GetFileInfo(sTP1);
            if (fi != null && false)
            {
                var sr = System.IO.File.OpenRead(sTP1);
                return File(sr, "video/mp4", Path.GetFileName(sTP1), true); //enableRangeProcessing = true
            }
            else
            {
                // Pulling down from StorjIO
                Stream s = await BBPAPI.StorjIOReadOnly.StorjDownloadStream(sVideo);
                return File(s, "video/mp4", Path.GetFileName(sTP1), true);
            }
        }
    }
}

