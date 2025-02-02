using BMSCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Syncfusion.Licensing;
using System.Threading.Tasks;
using static BMSCommon.Common;

namespace BiblePay.BMS
{


	public class Program
    {

        public static async Task<bool> LocalInit()
        {
            // Primary Entry point for BMS:
            // Set the user, start the loop thread, and initialize the charge
            //await BBPAPI.ServiceInit.Init();
            // This area is still in development....
            if (1 == 0)
            {
                /*
                    System.Threading.Thread tPushServer = new System.Threading.Thread(DSQL.PushServer.InitializePushServer);
                    tPushServer.Start();
                    System.Threading.Thread tPushClient = new System.Threading.Thread(DSQL.PushServer.InitializePushClient);
                    tPushClient.Start();
                */
            }
            return true;
        }



        public static async Task Main(string[] args)
        {
            // BIND TO THE RIGHT PORT - Public Web host binds to HTTPS
            // User binds to HTTP
            // Sancs bind to HTTPS
            // Primary Entry Point for background thread (looper)
            BBPAPI.Service.RegisterProControls(SyncfusionLicenseProvider.RegisterLicense);
            string sPath = GetFolder("") + "bms.conf";
            Common.Log("BOOTING UP USING PATH " + sPath + " for conf");
            string sBindURL = GetConfigKeyValue("bindurl");
            /*
            if (sBindURL == string.Empty && false)
            {
                sBindURL = "https://localhost:" + GlobalSettings.DEFAULT_HTTPS_PORT.ToString();
            }
            */
            if (System.Diagnostics.Debugger.IsAttached)
            {
                sBindURL = "https://localhost:" + BiblePay.BMS.GlobalSettings.DEFAULT_HTTPS_PORT.ToString();
            }


            sBindURL += ";http://0.0.0.0:" + BiblePay.BMS.GlobalSettings.DEFAULT_HTTP_PORT.ToString();

            await LocalInit();

            StartupConfig.CreateHostBuilder(args, sBindURL, null).Build().Run();

        }


    }
}
