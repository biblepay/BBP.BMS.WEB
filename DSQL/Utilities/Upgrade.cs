using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using static BMSCommon.Common;


namespace BiblePay.BMS
{
    // R ANDREWS - June 28th, 2019
    public class Upgrade
    {

        public static bool TerminateProcess(string sName)
        {
            if (IsWindows())
                return false;

            // Used to Terminate a dotnetcore process by name (cross platform)
            try
            {
                Process[] proc = Process.GetProcessesByName(sName);
                Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

                if (proc.Length > 0)
                {
                    for (int i = 0; i < proc.Length; i++)
                    {

                        if (currentProcess.Id != proc[i].Id)
                        {
                            Log("killing " + proc[i].Id.ToString());
                            proc[i].Kill();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log("BiblePayUpgrade (BUG)::TerminateProcess::Unable to terminate " + sName + " because " + ex.Message);
            }
            return false;
        }

        public static void StartNewThread()
        {
            string sPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            ProcessStartInfo pi = new ProcessStartInfo("dotnet", "BiblePay.BMSD.dll");
            pi.UseShellExecute = true;
            pi.WorkingDirectory = sPath;
            pi.CreateNoWindow = false;
            pi.WindowStyle = ProcessWindowStyle.Normal;
            Process procchild = Process.Start(pi);
        }
    }
}
