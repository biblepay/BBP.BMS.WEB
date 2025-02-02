using BiblePay.BMS.Models;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace BiblePay.BMS.Controllers.Quant
{
    public class QuantFunctions
    {

        public List<QuantChartItem> GetVIXOverTime()
        {
            List<QuantChartItem> l = new List<QuantChartItem>();
            DateTime dtStart = Convert.ToDateTime("5/1/2006");
            for (int i = 0; i < 20 * 365; i += 14)
            {
                DateTime dt = dtStart.AddDays(i);
                double nVIX = 0;
                if (nVIX != 0)
                {
                    QuantChartItem c = new QuantChartItem();
                    c.date = dt;
                    c.value = nVIX;
                    l.Add(c);
                }
            }
            return l;
        }

        public List<QuantChartItem> GetSPYOverTime()
        {
            List<QuantChartItem> l = new List<QuantChartItem>();
            DateTime dtStart = Convert.ToDateTime("5/1/2006");
            for (int i = 0; i < 20 * 365; i += 14)
            {
                DateTime dt = dtStart.AddDays(i);
                double nVIX = 0;// _VIX.GetSpyULPrice(dt);
                if (nVIX != 0)
                {
                    QuantChartItem c = new QuantChartItem();
                    c.date = dt;
                    c.value = nVIX;
                    l.Add(c);
                }
            }
            return l;
        }


        public List<QuantChartItem> GetCachedRFR()
        {
            List<QuantChartItem> t = GetQuantChart("rfr");
            if (t != null)
                return t;
            t = GetRFROverTime();
            return t;
        }

        public List<QuantChartItem> GetCachedVIX()
        {
            List<QuantChartItem> t = GetQuantChart("vix");
            if (t != null)
                return t;
            t = GetVIXOverTime();
            return t;
        }
        public List<QuantChartItem> GetCachedSPY()
        {
            List<QuantChartItem> t = GetQuantChart("spy");
            if (t != null && t.Count > 0)
                return t;
            t = GetSPYOverTime();
            return t;
        }


        public List<QuantChartItem> GetCachedMACD(string sType)
        {
            List<QuantChartItem> t = GetQuantChart(sType);
            if (t != null)
                return t;
            t = GetMACDOverTime(sType);
            return t;
        }

        public static List<QuantChartItem> GetQuantChart(string id)
        {
            DatabaseQuery dq = new DatabaseQuery();
            dq.Key = id;
            dq.TableName = "BackTest";
            string sData = BBPAPI.Interface.Repository.DeserializeOptionsObject(dq);
            List<QuantChartItem> t1 = DeserializeFromString(sData);
            return t1;
        }

        public List<QuantChartItem> GetMACDOverTime(string sType)
        {
            List<QuantChartItem> l = new List<QuantChartItem>();
            DateTime dtStart = Convert.ToDateTime("5/1/2006");
            for (int i = 0; i < 20 * 365; i += 14)
            {
                DateTime dt = dtStart.AddDays(i);
                MACD m = new MACD();
                if (m != null)
                {
                    QuantChartItem c = new QuantChartItem();
                    c.date = dt;
                    if (sType == "MACD")
                    {
                        c.value = m.DMA12 - m.DMA26;
                    }
                    else if (sType == "BollingerUpper")
                    {
                        c.value = m.BollingerUpperBand;
                    }
                    else if (sType == "BollingerLower")
                    {
                        c.value = m.BollingerLowerBand;
                    }
                    else if (sType == "KeltnerUpper")
                    {
                        c.value = m.KeltnerUpperBand;
                    }
                    else if (sType == "KeltnerLower")
                    {
                        c.value = m.KeltnerLowerBand;
                    }
                    else if (sType == "TTMSqueeze")
                    {
                        c.value = m.TTMSqueeze;
                    }
                    l.Add(c);
                }
            }
            return l;
        }

        public List<QuantChartItem> GetRFROverTime()
        {
            List<QuantChartItem> l = new List<QuantChartItem>();
            DateTime dtStart = Convert.ToDateTime("5/1/2006");
            for (int i = 0; i < 20 * 365; i += 14)
            {
                DateTime dt = dtStart.AddDays(i);
                double r = 0;// _VIX.GetRFR(dt);
                if (r != 0)
                {
                    QuantChartItem c = new QuantChartItem();
                    c.date = dt;
                    c.value = r;
                    l.Add(c);
                }
            }
            return l;

        }



        public List<QuantChartItem> ConvertToQuantChart(dynamic t, string sType)
        {
            List<QuantChartItem> l = new List<QuantChartItem>();
            for (int i = 0; i < t.Analysis.Count; i++)
            {
                var d = t.Analysis[i];
                QuantChartItem c = new QuantChartItem();
                c.date = d.OpenDate;
                if (sType == "netliq")
                {
                    c.value = d.NetLiquidationValue;
                }
                else
                    if (sType == "roi")
                {
                    c.value = d.ROI * 100;
                }
                else if (sType == "st")
                {
                    c.value = d.WinsShortTermPercentage * 100;
                }
                else if (sType == "averagecost")
                {
                    c.value = -1 * (d.AverageCost * 100);
                }
                else if (sType == "averageloss")
                {
                    c.value = -1 * (d.AverageLoss * 100);
                }
                else if (sType == "averagewin")
                {
                    c.value = d.AverageWin * 100;
                }
                l.Add(c);
            }
            return l;
        }


        public static List<QuantChartItem> DeserializeFromString(string sData)
        {
            List<QuantChartItem> tDummy = new List<QuantChartItem>();
            if (sData == String.Empty)
            {
                return tDummy;
            }
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(tDummy.GetType());
            try
            {
                StringReader srIn = new StringReader(sData);
                List<QuantChartItem> j = (List<QuantChartItem>)x.Deserialize(srIn);
                return j;
            }
            catch (Exception)
            {
            }
            return null;
        }




        public String ToFS(object d)
        {
            double n = BMSCommon.Common.GetDouble(d);
            n = Math.Round(n, 2);
            return n.ToString();
        }



        private string GetChartFrame(string sBoxId, string sType)
        {
            string sChart1 = "<iframe style='width:800px;height:450px;border:none;' src='chart/chartview?id="
                + sBoxId + "&type=" + sType + "'/></iframe>\r\n";
            return sChart1;
        }


        /*
        public ActionResult StrategyBacktest()
        {
            DataTable dtVix = new DataTable();// BBPAPI.Interface.Repository.GetStockOverviewPrices("");
            _VIX = null;// new VIX(dtVix);
            BlackBoxEditModel m = new BlackBoxEditModel();
            string sID = Request.Query["id"].ToString();
            //OptionsShared.ProfitLoss.LONGSHORT L0 = LS == 1 ? OptionsShared.ProfitLoss.LONGSHORT.LONG : OptionsShared.ProfitLoss.LONGSHORT.SHORT;
            // var t = null;// BBPAPI.Interface.Repository.GetTradeAnalysisReport(sID);
            dynamic t = null;
            StringBuilder sHTML = new StringBuilder();

            // Graphs here
            // Bank over Time
            // Short Term Win % over time
            // ROI over time
            sHTML.Append(GetChartFrame(sID, "netliq"));
            sHTML.Append(GetChartFrame(sID, "roi"));
            sHTML.Append(GetChartFrame(sID, "stwin"));
            sHTML.Append(GetChartFrame(sID, "costs"));
            sHTML.Append(GetChartFrame(sID, "squeeze"));
            sHTML.Append(GetChartFrame(sID, "spy"));

            sHTML.Append("<h3>" + t.Name + " - " + t.Strategy);

            if (t.Analysis.Count < 1)
            {
                return View(m);
            }
            string sFooter = "<br><h4>Summary:</h4><table class=saved><tr><td>ROI<td>"
                + t.Analysis[t.Analysis.Count - 1].ROI.ToPercentage() + "</tr><tr><td>Alpha<td>"
            + t.Alpha.ToString() + "</tr><tr><td>Beta<td>" + t.Beta.ToString() + "</tr><tr><td>Sharpe Ratio<td>"
            + t.SharpeRatio.ToString() + "</tr><tr><td>Average Duration<td>" + t.AverageDuration.ToString() + "</tr><tr><td>Start Date<td>"
            + t.StartDate.ToShortDateString() + "</tr><tr><td>End Date<td>" + t.EndDate.ToShortDateString()
            + "</tr><tr><td>Leverage<td>" + t.Leverage.ToString();
            sFooter += "</td></tr><tr><td>Name<td>" + t.Name + "</tr><tr><td>Strategy<td>" + t.Strategy + "<tr><td>Long/Short<td>"
                + t.LongShort + "<tr><td>Risk Free Rate<td>" + t.RFR.ToString() + "<tr><td>Tone<td>" + t.Tone.ToString() + "</tr>";

            sFooter += "<tr><td>Monte Carlo Positive Sum<td>" + t.MonteCarloPositive
                + "<tr><td>Monte Carlo Negative Sum<td>" + t.MonteCarloNegative + "<td>Monte Carlo Steps<td>" + t.MonteCarloSteps + "</tr></table>";

            sHTML.Append(sFooter);

            sHTML.Append("<table class=saved>");

            string sHeader = "<tr style='background-color:yellow;'><td>Trade No<td>Symbol"
                        + "<td>Qty<td>Open Date<td>Avg Win<td>Avg Loss<td>UL Open<td>UL Close<td>ST Win %<td>LT Win %<td>Wins"
                        + "<td>P&L<td>ROI %<td>Net Liq.<td>Bank</td><td>Max Drawdown<td>Low Bank</tr>";

            for (int i = 0; i < t.Analysis.Count; i++)
            {
                var t1 = t.Analysis[i];
                string sRow = "<tr><td>" + t1.TradeNumber.ToString() + "<td>" + t1.Symbol.ToString() + "<td>"
                    + t1.Quantity.ToString() + "<td>" + t1.OpenDate.ToShortDateString() + "<td>" + ToFS(t1.AverageWin * 100)
                    + "<td>" + ToFS(t1.AverageLoss * 100)
                    + "<td>" + t1.UnderlyingOpenPrice.ToString()
                    + "<td>" + t1.UnderlyingClosePrice.ToString()
                    + "<td>" + t1.WinsShortTermPercentage.ToPercentage()
                    + "<td>" + t1.WinsLongTermPercentage.ToPercentage() + "<td>" + t1.Wins.ToString()
                    + "<td>" + ToFS(t1.PL) + "<td>" + t1.ROI.ToPercentage()
                    + "<td>" + t1.NetLiquidationValue.ToCurrency() + "</td>"
                    + "<td>" + t1.Bank.ToCurrency() + "</td><td>"

                    + t1.MaxDrawdown.ToCurrency()
                    + "<td>" + t1.LowBank.ToCurrency() + "</tr>";
                if (i % 20 == 0)
                {
                    sHTML.Append(sHeader);
                }
                if (t1.Narrative != null)
                {
                    string sRow2 = "<tr><td colspan=10>" + t1.Narrative.ToString() + "</td></tr>";
                    sHTML.Append(sRow2);
                }
                sHTML.Append(sRow + "\r\n");

                if (i > 7000)
                    break;
            }
            sHTML.Append("</table>");
            m.Report = sHTML.ToString();
            return View(m);
        }
        */

    }
}
