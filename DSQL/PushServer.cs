
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static BMSCommon.Common;

namespace BiblePay.BMS.DSQL
{
	public static class PushServer
    {
        private static int iPushThreadID = 0;
        private static int iPushThreadCount = 0;
        private static int nPushServerPort = 3005;

        private static void CloseSocket(Socket c)
        {
            try
            {
                c.Close();
            }
            catch (Exception)
            {

            }
        }


        private static bool SendPacketToClient(Socket oClient, byte[] oData, int iSize)
        {
            try
            {
                oClient.Send(oData, iSize, SocketFlags.None);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("was aborted"))
                {

                }
                else
                {
                    bool fPrint = !(ex.Message.Contains("A connection attempt failed because the connected party did not properly respond after a period of time")
                        || ex.Message.Contains("An existing connection was forcibly closed"));
                    if (fPrint)
                        Log("SEND " + ex.Message);
                }
                return false;
            }
        }



        private static void PushClientThread(Socket client, string socketid)
        {
            string sData = String.Empty;
            int nLastReceived = UnixTimestamp();
            double nTrace = 0;
            // Client to Push Server
            try
            {
                client.ReceiveTimeout = 65000;
                client.SendTimeout = 65000;

                while (true)
                {
                    int size = 0;
                    int nElapsed = UnixTimestamp() - nLastReceived;

                    if (nElapsed > (60 * 60 * 10))
                    {
                        client.Close();
                        iPushThreadCount--;
                        return;
                    }
                    if (!client.Connected)
                    {
                        iPushThreadCount--;
                        return;
                    }

                    //WorkerInfo w1 = GetWorker(socketid);
                    bool fBanned = false;

                    if (!fBanned && client.Available > 0)
                    {
                        byte[] data = new byte[client.Available];
                        nLastReceived = UnixTimestamp();

                        try
                        {
                            size = client.Receive(data);
                        }
                        catch (ThreadAbortException)
                        {
                            return;
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("An existing connection was forcibly closed"))
                            {
                                Console.WriteLine("ConnectionClosed");
                                return;
                            }
                            else if (ex.Message.Contains("was aborted"))
                            {
                                return;
                            }
                            Console.WriteLine("Error occurred while receiving data " + ex.Message);
                        }
                        if (size > 0)
                        {
                            sData = Encoding.UTF8.GetString(data, 0, data.Length);
                            sData = sData.Replace("\0", "");
                            // From Client to PushServer
                            // INBOUND DATA
                            // Send a pong back for this data.
                            System.Diagnostics.Debug.WriteLine(sData);

                            sData += "MY REPLY";
                            byte[] dOut = Encoding.ASCII.GetBytes(sData);
                            SendPacketToClient(client, dOut, dOut.Length);

                            
                            bool fSent = true;

                        }
                        else
                        {
                            if (true)
                            {
                                // Keepalive (prevents the pool from hanging up on the miner)
                                var json = "{ \"id\": 0, \"method\": \"keepalived\", \"arg\": \"na\" }\r\n";
                                data = Encoding.ASCII.GetBytes(json);
                            }
                        }
                    }

                    // ****************************************** In from XMR Pool -> Miner *******************************************************
                    // Back to CLIENT here:
                    if (false)
                    {
                        var json = "{ \"id\": 0, \"method\": \"keepalived\", \"arg\": \"na\" }\r\n";
                        byte[] byteOut = Encoding.ASCII.GetBytes(json);
                        // This goes back to the client
                        SendPacketToClient(client, byteOut, byteOut.Length);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Log("minerXMRThread is going down...", true);
                return;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("did not properly respond"))
                {
                    Log("PushServerThread[0]: Trace=" + nTrace.ToString() + ":" + ex.Message);
                    //Ban(socketid, 1, ex.Message.Substring(0, 12));
                }
                if (ex.Message.Contains("was aborted"))
                {
                    // Noop
                }
                else if (ex.Message.Contains("forcibly closed"))
                {

                }
                else if (!ex.Message.Contains("being aborted"))
                {
                    // This is where we see Unexpected end of content while loading JObject. Path 'params.job_id', line 1, position 72.
                    // and Unterminated string. Expected delimiter: ". Path 'params.result', line 1, position 144.
                    // Invalid character after parsing property name. Expected ':' but got:
                    // and Unterminated string. Expected delimiter: ". Path 'params.id', line 1, position 72.
                    Log("minerXMRThread2 v2.1: " + ex.Message + " [sdata=" + sData + "], Trace=" 
                        + nTrace.ToString() + ", PARSEDATA     \r\n");
                }
            }
            iPushThreadCount--;
        }

        private static int SOCKET_TIMEOUT = 5000;

        public static void InitializePushClient()
        {
            int nTrace = 0;
            while (true)
            {
                try
                {
                    TcpClient tcp = new TcpClient();
                    string sLocation = "localhost";
                    tcp.Connect(sLocation, nPushServerPort);
                    for (int i = 0; i < 30; i++)
                    {
                        var json = "{ \"id\": 0, \"method\": \"keepalived\", \"arg\": \"na\" }\r\n";
                        byte[] outData = Encoding.ASCII.GetBytes(json);
                        Stream stmOut = tcp.GetStream();
                        stmOut.Write(outData, 0, json.Length);

                        System.Threading.Thread.Sleep(10000);

                        NetworkStream stmIn = tcp.GetStream();
                        int bytesIn = 0;

                        try
                        {
                            tcp.ReceiveTimeout = SOCKET_TIMEOUT;
                            tcp.SendTimeout = SOCKET_TIMEOUT;
                            if (stmIn.DataAvailable)
                            {
                                byte[] bIn = new byte[65536];
                                bytesIn = stmIn.Read(bIn, 0, 65536);
                                if (bytesIn > 0)
                                {
                                    string sData = Encoding.UTF8.GetString(bIn, 0, bytesIn);
                                    sData = sData.Replace("\0", "");
                                    string[] vData = sData.Split("\n");
                                    System.Diagnostics.Debug.WriteLine(" INB " + sData);

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.Contains("did not properly respond"))
                            {
                                Log("PushClientThread[0]: Trace=" + nTrace.ToString() + ":" + ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    BMSCommon.Common.Log("PushClient::" + ex.Message);
                }

                System.Threading.Thread.Sleep(60 * 60 * 1000);
            }

        }



        public static void InitializePushServer()
        {

            TcpListener listener = null;

            retry:

            if (listener != null)
            {
                listener.Stop();
                System.Threading.Thread.Sleep(5000);
            }

            try
            {
                bool fDebugMode = true;
                bool fIsPrimary = BBPAPI.Service.IsPrimary();
                listener = new TcpListener(IPAddress.Any, nPushServerPort);
                listener.Start();
                string sNarr = "BBP Push Server starting up on port " + nPushServerPort.ToString();
                BMSCommon.Common.Log(sNarr);
            }
            catch (Exception ex1)
            {
                BMSCommon.Common.Log("Problem starting PushServer:" + ex1.Message);
                System.Threading.Thread.Sleep(60000);
                goto retry;
            }

            while (true)
            {
                //  The inbound socket service thread
                try
                {
                    Thread.Sleep(10);
                    if (listener.Pending())
                    {
                        Socket client = listener.AcceptSocket();
                        try
                        {
                            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        }
                        catch (Exception ex)
                        {
                            BMSCommon.Common.Log("NonCrit reuseaddr " + ex.Message);
                        }
                        try
                        {
                            if (BMSCommon.Common.IsWindows())
                                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                        }
                        catch (Exception ex)
                        {
                            BMSCommon.Common.Log("Logging non-critical dontlinger " + ex.Message);
                        }

                        int nSockTrace = 0;
                        string socketid = client.RemoteEndPoint.ToString();
                        try
                        {
                            nSockTrace = 1;

                            if (true)
                            {
                                iPushThreadID++;
                                ThreadStart starter = delegate { PushClientThread(client, socketid); };
                                var childSocketThread = new Thread(starter);
                                iPushThreadCount++;
                                childSocketThread.Start();

                            }
                            else
                            {
                                // They are already banned
                                CloseSocket(client);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("We have a big issue answering sockets " + ex.Message + ", sock trace=" + nSockTrace.ToString());
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Log("PushServer is going down...");
                    return;
                }
                catch (Exception ex)
                {
                    Log("InitializeXMRPool v1.3: " + ex.Message);
                    Thread.Sleep(5000);
                    goto retry;
                }
            }
        }
    }
}

