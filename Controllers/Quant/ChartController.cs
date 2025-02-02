using BMSCommon;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using BMSCommon.Model;
using System.Data;
using BiblePay.BMS.Controllers.Quant;

namespace BiblePay.BMS.Controllers
{
    public class ChartController : Controller
    {

        public static string[] GetChartOfGenericDataTradingView2(List<QuantChart> l, string sChartType)
        {
            DateTime dtOldTime = new DateTime();
            string[] sDataSet = new string[10];

            for (int ch = 0; ch < l.Count; ch++)
            {
                for (int i = 0; i < l[ch].Chart.Count; i++)
                {
                    QuantChartItem dp = l[ch].Chart[i];
                    DateTime dtchart = dp.date.AddDays(0);

                    string sRow = "{ time: '" + dtchart.ToString("yyyy-MM-dd") + "', value: " + (dp.value).ToString() + " },";
                    string sStick = "{ time: '" + dtchart.ToString("yyyy-MM-dd") + "', open: " + dp.value.ToString() + ", high: " + dp.value.ToString() + ", low: " + dp.value.ToString() + ", close: " + (dp.value + 1).ToString() + "},";
                    string sActive = sChartType == "candlestick" ? sStick : sRow;
                    if (dtOldTime == dtchart)
                    {
                        //bool f999 = false;
                    }
                    else
                    {
                        sDataSet[ch] += sActive;
                    }
                    dtOldTime = dtchart;
                }
            }
            for (int i = 0; i < l.Count; i++)
            {
                if (sDataSet[i] != null)
                {
                    sDataSet[i] = sDataSet[i].Substring(0, sDataSet[i].Length - 1);
                }
            }
          
            return sDataSet;
        }

        public IActionResult ChartView()
        {
            DataTable dtVix = null;// BBPAPI.Interface.Repository.GetStockOverviewPrices("");
			QuantFunctions q = new QuantFunctions();

            //q._VIX = null;// new VIX(dtVix);


			string sID = Request.Query["id"].ToString();
            string sType = Request.Query["type"].ToString();

            dynamic t = null;// BBPAPI.Interface.Repository.GetTradeAnalysisReport(sID);
            StringBuilder sHTML = new StringBuilder();
            List<QuantChart> lc = new List<QuantChart>();
            if (sType == "netliq")
            {
                List<QuantChartItem> lNetLiq = q.ConvertToQuantChart(t, "netliq");
                QuantChart c1a = new QuantChart("Net Liquidity - Time", "green", "lime", lNetLiq);
                lc.Add(c1a);
            }
            else if (sType == "stwin")
            {
                List<QuantChartItem> lst = q.ConvertToQuantChart(t, "st");
                List<QuantChartItem> lMACD = q.GetCachedMACD("MACD");
                List<QuantChartItem> lVIX = q.GetCachedVIX();
                List<QuantChartItem> lRFR = q.GetCachedRFR();
                QuantChart c3a = new QuantChart("ST Wins", "green", "lime", lst);
                QuantChart c3b = new QuantChart("MACD", "blue", "black", lMACD);
                QuantChart c3c = new QuantChart("VIX", "red", "pink", lVIX);
                QuantChart c2b = new QuantChart("RFR", "blue", "black", lRFR);
                lc.Add(c3a);
                lc.Add(c3b);
                lc.Add(c3c);
                lc.Add(c2b);
            }
            else if (sType == "spy")
            {
                List<QuantChartItem> lspy = q.GetCachedSPY();
                QuantChart c7a = new QuantChart("SPY", "yellow", "brown", lspy);
                List<QuantChartItem> lroi = q.ConvertToQuantChart(t, "roi");
                QuantChart c7b = new QuantChart("ROI", "lime", "green", lroi);

                lc.Add(c7a);
                lc.Add(c7b);

            }
            else if (sType == "roi")
            {
                List<QuantChartItem> lroi = q.ConvertToQuantChart(t, "roi");
                QuantChart c2a = new QuantChart("ROI Over Time", "lime", "green", lroi);
                List<QuantChart> lc2b = new List<QuantChart>();
                lc.Add(c2a);
            }
            else if (sType == "costs")
            {
                List<QuantChartItem> lAvgCost = q.ConvertToQuantChart(t, "averagecost");
                List<QuantChartItem> lAvgWin = q.ConvertToQuantChart(t, "averagewin");
                List<QuantChartItem> lAvgLoss = q.ConvertToQuantChart(t, "averageloss");
                QuantChart c4a = new QuantChart("Cost over Time", "pink", "black", lAvgCost);
                QuantChart c4b = new QuantChart("Win over Time", "green", "lime", lAvgWin);
                QuantChart c4c = new QuantChart("Loss over Time", "red", "pink", lAvgLoss);
                lc.Add(c4a);
                lc.Add(c4b);
                lc.Add(c4c);
           }
            else if (sType=="squeeze")
            {
                List<QuantChartItem> lBU = q.GetCachedMACD("BollingerUpper");
                List<QuantChartItem> lBL = q.GetCachedMACD("BollingerLower");
                List<QuantChartItem> lKU = q.GetCachedMACD("KeltnerUpper");
                List<QuantChartItem> lKL = q.GetCachedMACD("KeltnerLower");
                List<QuantChartItem> lSqueeze = q.GetCachedMACD("TTMSqueeze");
                QuantChart c5a = new QuantChart("TTM Squeeze", "green", "lime", lSqueeze);
                QuantChart c5b = new QuantChart("Bollinger upper", "blue", "blue", lBU);
                QuantChart c5c = new QuantChart("Bollinger lower", "blue", "blue", lBL);
                QuantChart c5d = new QuantChart("Keltner Upper", "brown", "yellow", lKU);
                QuantChart c5e = new QuantChart("Keltner Lower", "brown", "yellow", lKL);
                lc.Add(c5a);
                lc.Add(c5b);
                lc.Add(c5c);
                lc.Add(c5d);
                lc.Add(c5e);
            }

            string[] s = GetChartOfGenericDataTradingView2(lc, "area");
            string sJS = "var lineseries = new Array();\r\n";
            sJS += "var legendnames = new Array();\r\n";
            
            for (int i = 0; i < lc.Count; i++)
            {
                if (s[i] != null)
                {
                    int nWidth = (i == 0) ? 3 : 2;
                    string sArea = "{topColor: '" + lc[i].BorderColor + "', bottomColor: 'rgba(0, 120, 255, 0.0)',"
                        + "lineColor: '" + lc[i].BackColor + "',color: '" + lc[i].BackColor + "',lineWidth: " + nWidth.ToString() + "}";

                    string sType1 = (i == 0) ? "addAreaSeries" : "addLineSeries";
                    string sSeriesNew = "lineseries[" + i.ToString() + "] = chart." + sType1 + "(" + sArea + ");"
                        +"\r\nlineseries["+i.ToString()+"].setData([" + s[i] + "]);\r\n";
                    sSeriesNew += "legendnames[" + i.ToString() + "] = new Object();legendnames[" + i.ToString() + "].Name='" 
                        + lc[i].Name + "';legendnames[" + i.ToString() + "].Color='" + lc[i].BackColor + "';\r\n";


                    sJS += sSeriesNew + "\r\n";
                }
            }
            ViewBag.ChartJS = sJS;
            return View();
        }

    }
}
