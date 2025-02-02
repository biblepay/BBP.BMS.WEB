using BiblePay.BMS.DSQL;
using BiblePay.BMS.Extensions;
using BiblePay.BMS.Models;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using static BiblePay.BMS.DSQL.UIWallet;
using static BMSCommon.Extensions;

namespace BiblePay.BMS.Controllers
{
    public class QuantController : Controller
    {

        public VIX _VIX = null;

        [HttpPost]
        public async Task<JsonResult> ProcessDoCallback([FromBody] ClientToServer o)
        {
            ServerToClient returnVal = new ServerToClient();
            User u0 = GetUser(HttpContext);

            if (o.Action == "Profile_SaveQuant")
            {
                double nLiq = GetFormData(o.FormData, "txtNetLiq").ToDouble();
                if (nLiq > 0)
                {
                }
            }
			else if (o.Action == "quant_subscribe")
            {
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sProdID = a.strategyid.Value;
                string sResult = QuantController.SubscribeToProduct(sProdID, this.HttpContext);
                if (sResult == String.Empty)
                {
                    DSQL.UI.MsgBox(HttpContext, "Subscribed", "Subscribed",
                        "Thank you for subscribing to this quant strategy. "
                        + "Your account will automatically be debited on the first of each month for the monthly "
                        + "subscription fee denominated in BBP.  You may cancel at any time by visiting My Subscriptions. <br><br>As long as your account is in good standing, you will receive a weekly "
                        + "Signal e-mail containing the analysis service Signal Output for hypothetical trades that this strategy would execute if "
                        + "the computer were following these rules for a hypothetical process in an investment fund.  <br><br>By using this service you agree that all investment signals are for informational purposes only, "
                        + " and do not constitute trading advice.  It is at your sole discretion to fully evaluate each possible trade and make a SELF DIRECTED DECISION.   "
                        + " By using this service you agree to take responsibility for your own actions, and you hereby hold BiblePay and our Quant division harmless "
                        + " from all harm that may arise by acting on your Self Directed actions in your personal trading account.  <br><br>PAST PERFORMANCE IS NOT A GUARANTEE OF FUTURE RESULTS.\r\n", false);
                }
                else
                {
                    DSQL.UI.MsgBox(HttpContext, "Error", "Error", sResult, false);
                }

                string m = "location.href='bbp/messagepage';";
                returnVal.returnbody = m;
                returnVal.returntype = "javascript";
                string o1 = JsonConvert.SerializeObject(returnVal);
                return Json(o1);
            }
            else
            {
                throw new Exception(o.Action + " not recognized");
            }
            return Json("");
        }




        public string SerializeToString(List<QuantChartItem> theChart)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(theChart.GetType());
            StringWriter m = new StringWriter();
            x.Serialize(m, theChart);
            return m.ToString();
        }

		public static string SubscribeToProduct(string sProductID, HttpContext h)
        {
            string sError = String.Empty;
            if (!h.GetCurrentUser().LoggedIn)
            {
                sError = "Not logged in.";
            }
            double nBalance = BBPAPI.Interface.Repository.QueryAddressBalance(IsTestNet(h), h.GetCurrentUser().BBPAddress);
            if (nBalance < 1000)
            {
                sError = "Your BBP balance is too low; please add funds.";
            }
            string sSig = String.Empty;
            h.Request.Cookies.TryGetValue("erc20signature", out sSig);
            if (sSig == String.Empty)
            {
                sError = "Your metamask signature is null, please log out and log back in first.";
            }

            if (sError == String.Empty)
            {
                DataTable dt = null;
                if (dt.Rows.Count == 0)
                {
                    sError = "Unable to find product.";
                }
                else
                {

                    double nCt = 0;
                    if (nCt > 0)
                    {
                        sError = "Sorry, you are already subscribed to this product.";
                    }
                    else
                    {
                        bool f1 = false;

                        if (!f1)
                        {
                            sError = "Unable to subscribe";
                        }
                        else
                        {
                            return String.Empty;
                        }
                    }
                }
            }
            return sError;
        }

        public ActionResult ProductList()
        {
            DataTable dt = null;
            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='saved'>");
            sb.Append("<tr><th>Product ID<th>Narrative<th>Added<th>ROI %<th>Cost BBP<th>Monthly Cost USD<th>Subscribe</tr>");
            foreach (DataRow dtr in dt.Rows)
            {
                string sID = dtr["id"].ToString().Substring(0, 16);
                string sROI = dtr["ROI"].AsDouble().ToString(); // Percentage();
                double nCost = dtr["cost"].AsDouble();
                double nCostBBP = ConvertUSDToBiblePay(nCost);
                string sSubscribe = GetStandardButton("btnsubscribe", "Subscribe", "quant_subscribe", 
                    "var e={};e.strategyid='" + dtr["id"].ToString() + "';",
                    "Are you sure you would like to subscribe to this algorithm?", "quant/processdocallback");
                string sRow = "<tr><td>" + sID + "<td>" + dtr["Narrative"] + "<td>"
                    + dtr["Added"].ToShortDateString() + "<td>" + sROI + "<td>" + FormatUSD(nCostBBP) + " BBP" 
                    + "<td>" + nCost.ToString() + "<td>" + sSubscribe + "</td></tr>";
                sb.Append(sRow);
            }
            sb.Append("</table>");
            BlackBoxEditModel m = new BlackBoxEditModel();
            m.Report = sb.ToString();
            return View(m);
        }


        public ActionResult Subscriptions()
        {
            string sMyID = HttpContext.GetCurrentUser().ERC20Address;
            DataTable dt = null;
            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='saved'>");
            sb.Append("<tr><th>Product ID<th>Narrative<th>Added<th>Monthly Cost BBP<th>Monthly Cost USD</tr>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sID = dt.Rows[i]["id"].ToString().Substring(0, 16);
                double nCost = dt.Rows[i]["MonthlyCost"].ToString().ToDouble();
                double nCostBBP = ConvertUSDToBiblePay(nCost);
                string sRow = "<tr><td>" + sID + "<td>" + dt.Rows[i]["Description"] + "<td>"
                    + dt.Rows[i]["Added"].ToShortDateString() + "<td>" + FormatUSD(nCostBBP) + " BBP"
                    + "<td>" + nCost.ToString() + "</tr>";
                sb.Append(sRow);
            }
            sb.Append("</table>");
            BlackBoxEditModel m = new BlackBoxEditModel();
            m.Report = sb.ToString();
            return View(m);
        }

        public ActionResult TransactionHistory()
        {
            string sMyID = HttpContext.GetCurrentUser().ERC20Address;
            DataTable dt = null;// DB.Financial.GetTxHistory(sMyID);
            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='saved'>");
            sb.Append("<tr><th>TXID<th>Description<th>Amount<th>Added</tr>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sID = dt.Rows[i]["id"].ToString().Substring(0, 16);
                double nAmt = dt.Rows[i]["Amount"].ToString().ToDouble();
                string sRow = "<tr><td>" + dt.Rows[i]["TXID"].ToString() + "<td>" + dt.Rows[i]["Description"] + "<td>"
                    + FormatUSD(nAmt) + "<td>" + dt.Rows[i]["Added"].ToShortDateString()
                    + "</tr>";
                sb.Append(sRow);
            }
            sb.Append("</table>");
            BlackBoxEditModel m = new BlackBoxEditModel();
            m.Report = sb.ToString();
            return View(m);
        }

        
        public ActionResult BlackBoxList()
        {
            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='saved'>");
            sb.Append("<tr><th>Index<th><th>All<th>Narrative<th>Added<TH>ROI %</tr>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                 string sID = dt.Rows[i]["id"].ToString();
                 string sNarr = dt.Rows[i]["Narrative"].ToString();
                 double nROI = dt.Rows[i]["ROI"].ToString().ToDouble();
                 string sROI = nROI == 0 ? "Calculating" : nROI.ToString() + "%";
                 string sInnerLinkIndex = "quant/StrategyBacktest?id=INDEX-" + sID;
                 string sInnerLinkAll = "quant/StrategyBacktest?id=ALL-" + sID + "&SYMBOL=SPY";
                 string sLinkIndex = "<a href='" + sInnerLinkIndex + "'>Index</a>";
				 string sLinkAll = "<a href='" + sInnerLinkAll + "'>All</a>";
 				 string sRow = "<tr><td>" + sLinkIndex + "<td>" + sLinkAll + "</td><td>"
                        + sNarr + "<td>" 
                        + dt.Rows[i]["Added"].ToString() + "<TD>" + sROI + "</tr>";
                 sb.Append(sRow);
            }
            sb.Append("</table>");
            BlackBoxEditModel m = new BlackBoxEditModel();
            m.Report = sb.ToString();
            return View(m);
        }
        


public ActionResult BlackBoxEdit()
{
    string sID = Request.Query["id"].ToString();
    BlackBoxEditModel m = new BlackBoxEditModel();
    DataTable dt = null;// DB.Financial.GetStrategyByID(sID);
    if (dt.Rows.Count > 0)
    {
        string sCode = dt.Rows[0]["code"].ToString();
        m.Code = sCode;
    }
    return View(m);
}


[HttpPost]
public JsonResult Save([FromBody] ClientToServer o)
{
    ServerToClient2 returnVal = new ServerToClient2();
    returnVal.Body = "alert('saved');";
    returnVal.Type = "javascript";
    string sID = Request.Query["id"].ToString();
    if (sID == String.Empty)
        return Json("err");

    TransformDOM t = new TransformDOM(o.FormData);
    DOMItem t0 = t.GetDOMItem("g0", "mycode");
    string sql = "Update options.Strategy set code=@mycode where id=@id;";
    if (t0.Value.Length > 50)
    {
        NpgsqlCommand m1 = new NpgsqlCommand(sql);
        m1.Parameters.AddWithValue("@id", sID);
        m1.Parameters.AddWithValue("@mycode", t0.Value);
    }
    return Json(returnVal);
}

[HttpPost]
public JsonResult DeleteBackTests([FromBody] ClientToServer o)
{
    BMSCommon.Model.ServerToClient2 returnVal = new ServerToClient2();
    returnVal.Body = "alert('Not Deleted');";
    returnVal.Type = "javascript";
    return Json(returnVal);
}
}
}
