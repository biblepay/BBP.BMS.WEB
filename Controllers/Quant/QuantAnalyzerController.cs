using BiblePay.BMS.DSQL;
using BiblePay.BMS.Extensions;
using BiblePay.BMS.Models;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BMSCommon.Extensions;
using static OptionsShared.Model.Schwab;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.UI;
using static BiblePay.BMS.DSQL.UIWallet;
using BBPModelShared;
using System.Collections.Generic;
using BBPAPI.Interface;

namespace BiblePay.BMS.Controllers
{
    public class QuantAnalyzerController : Controller
    {
        [HttpPost]
        public async Task<JsonResult> ProcessDoCallback([FromBody] ClientToServer o)
        {
            ServerToClient returnVal = new ServerToClient();
            User u0 = GetUser(HttpContext);
            if (o.Action == "Quant_Analyzer_Search")
            {
                string sSymbol = GetFormData(o.FormData, "txtSymbol");
                HttpContext.Session.SetObject("quant_symbol", sSymbol);
                ViewBag.CurSymbol = sSymbol;
                return this.ReturnURL("quantanalyzer/analyzer");
            }
            else if (o.Action == "Quant_Analyzer_AddTrade")
            {
                SchwabSpread s = new SchwabSpread();
                s.id = Guid.NewGuid().ToString();
                s.Strategy = "CALL";
                string sCurSymbol = HttpContext.Session.GetObject<string>("quant_symbol");
                if (String.IsNullOrEmpty(sCurSymbol))
                {
                    return this.ShowModalDialog(o, "Error", "Unable to Add to a null symbol.", String.Empty);
                }

                SchwabPrice p = new SchwabPrice();
                p.Symbol = sCurSymbol;
                p.Strike = 1;
                p.MidPrice = -1;
                s.UserId = HttpContext.GetCurrentUser().id;

                s.Positions.Add(p);
                BBPResult b = await BBPAPI.Interface.Finance.SaveAnalysisSpread(s);
                if (!b.Success)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to save.", String.Empty);
                }
                else
                {
                    return this.ReturnURL("quantanalyzer/analyzer");
                }

            }
            else if (o.Action == "Quant_Analyzer_Add")
            {
                // Add a leg to the current spread

                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sSpreadID = a.spreadid.Value;
                SchwabSpread u = HttpContext.Session.GetObject<SchwabSpread>(sSpreadID) ?? null;
                u.UserId = HttpContext.GetCurrentUser().id;

                if (u == null)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to retrieve spread.", String.Empty);
                }
                if (u.Positions.Count < 1)
                {
                    return this.ShowModalDialog(o, "Error", "Sorry, but this trade has no positions.", String.Empty);

                }
                SchwabPrice p = new SchwabPrice();
                p.Symbol = u.Positions[0].Symbol;
                p.Quantity = 1;
                p.Type = "CALL";
                p.Strike = 1;
                p.MidPrice = -1;
                p.Expiration = u.Positions[0].Expiration;

                u.Positions.Add(p);
                BBPResult b = await BBPAPI.Interface.Finance.SaveAnalysisSpread(u);
                if (!b.Success)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to save.", String.Empty);
                }
                else
                {
                    return this.ReturnURL("quantanalyzer/analyzer");
                }

            }
            else if (o.Action == "Quant_Analyzer_Delete")
            {
                // deletes the entire position
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sSpreadID = a.spreadid.Value;
                SchwabSpread u = HttpContext.Session.GetObject<SchwabSpread>(sSpreadID) ?? null;
                u.UserId = HttpContext.GetCurrentUser().id;
                if (u == null)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to retrieve spread.", String.Empty);
                }
                for (int i = 0; i < u.Positions.Count; i++)
                {
                    SchwabPrice p = u.Positions[i];
                    p.Symbol = "DELETED";
                }
                BBPResult b = await BBPAPI.Interface.Finance.SaveAnalysisSpread(u);
                if (!b.Success)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to Remove.", String.Empty);
                }
                else
                {
                    return this.ReturnURL("quantanalyzer/analyzer");
                }

            }
            else if (o.Action == "Quant_Analyzer_Detail_Delete")
            {
                // delete one position row only (not the entire position)
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sSpreadID = a.spreadid.Value;
                string sRowID = a.rowid.Value;
                SchwabSpread u = HttpContext.Session.GetObject<SchwabSpread>(sSpreadID) ?? null;
                u.UserId = HttpContext.GetCurrentUser().id;
                if (u == null)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to retrieve spread.", String.Empty);
                }
                // Find the original row to delete
                for (int i = 0; i < u.Positions.Count; i++)
                {
                    SchwabPrice p = u.Positions[i];
                    string sID = u.id + "." + i.ToString();
                    if (sID == sRowID)
                    {
                        u.Positions.RemoveAt(i);
                        break;
                    }
                }
                BBPResult b = await BBPAPI.Interface.Finance.SaveAnalysisSpread(u);
                if (!b.Success)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to Remove.", String.Empty);
                }
                else
                {
                    return this.ReturnURL("quantanalyzer/analyzer");
                }


            }
            else if (o.Action == "Quant_Analyzer_Save")
            {
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sSpreadID = a.spreadid.Value;
                SchwabSpread u = HttpContext.Session.GetObject<SchwabSpread>(sSpreadID) ?? null;
                u.UserId = HttpContext.GetCurrentUser().id;

                if (u == null)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to retrieve spread.", String.Empty);
                }
                // Update the spread here with the consituent Position values.
                for (int i = 0; i < u.Positions.Count; i++)
                {
                    string sParentID = u.id + "." + i.ToString();
                    string sExpiration = GetFormData(o.FormData, "txtExpiration", sParentID);
                    string sEnabled = GetFormData(o.FormData, "chkEnabled", sParentID);
                    string sAction = GetFormData(o.FormData, "txtAction", sParentID);
                    double nStrike = GetFormData(o.FormData, "txtStrike", sParentID).ToDouble();
                    double nQuantity = GetFormData(o.FormData, "txtQuantity", sParentID).ToDouble();
                    double nPrice = GetFormData(o.FormData, "txtPrice", sParentID).ToDouble();
                    string sType = GetFormData(o.FormData, "txtDropDownType", sParentID).ToStr();

                    if (sExpiration != "" && nStrike > 0)
                    {
                        SchwabPrice p = u.Positions[i];
                        p.Expiration = Convert.ToDateTime(sExpiration);
                        p.Enabled = Convert.ToBoolean(sEnabled);
                        p.Quantity = (int)nQuantity;
                        p.MidPrice = nPrice;
                        p.Type = sType;
                        p.Strike = nStrike;
                    }
                }

                BBPResult b = await BBPAPI.Interface.Finance.SaveAnalysisSpread(u);
                if (!b.Success)
                {
                    return this.ShowModalDialog(o, "Error", "Unable to save.", String.Empty);
                }
                else
                {
                    return this.ReturnURL("quantanalyzer/analyzer");
                }

            }
            else
            {
                throw new Exception(o.Action + " not recognized");
            }
        }

        private async Task<bool> LoadSpreadsIntoOptionsModel(OptionsTradeModel otm)
        {
            List<SchwabSpread> l = (List<SchwabSpread>)ViewBag.Spreads;

            for (int i = 0; i < l.Count; i++)
            {
                SchwabSpread s = l[i];
                OptionsTrade trade = new OptionsTrade();
                trade.id = s.id;
        
                for (int j = 0; j < s.Positions.Count; j++)
                {
                    SchwabPrice p = s.Positions[j];

                    OptionsPosition o = new OptionsPosition();
                    o.Action = (p.Quantity > 0) ? "BUY" : "SELL";

                    o.Symbol = p.Symbol;
                    o.Description = p.OPRASymbol;
                    o.Price = Math.Round(p.MidPrice, 2);
                    o.Type = p.Type;
                    o.Expiration = p.Expiration;
                    o.Enabled = p.Enabled;
                    o.Strategy = p.Type; //todo - have it detect the spread type here and save it.

                    o.Quantity = p.Quantity;
                    o.ParentID = trade.id.ToStr() + "." + j.ToStr();
                    o.Strike = p.Strike;
                    trade.Positions.Add(o);
                }
                otm.OptionsTradeList.Add(trade);

            }
            return true;
        }

        private async Task<BBPChart> LoadAnalyzerPositions(string sSymbol)
        {
            SymbolRequest sr = new SymbolRequest();
            sr.Symbol = sSymbol;
            sr.UserId = HttpContext.GetCurrentUser().id;
            List<SchwabSpread> _spreads = await BBPAPI.Interface.Finance.GetAnalysisSpreads(sr);
            for (int i = 0; i < _spreads.Count; i++)
            {
                SchwabSpread spread = _spreads[i];
                HttpContext.Session.SetObject(spread.id, spread);

            }

            BMSWeb.BusinessLogic.Finance.UpdateSpreadsWithMarketData(sSymbol, _spreads);

            ViewBag.Spreads = _spreads;
            BBPChart b0 = await DrawChartData(_spreads);
            return b0;
        }
        //// Add,Save,Delete,Clone,OppositeClone,CreateOrder


        private async Task<BBPChart> DrawChartData(List<SchwabSpread> _spreads)
        {
            BBPChart b0 = new BBPChart();

            // Get the position first
            if (_spreads.Count < 1)
            {
                return b0;
            }
            // Stress test the position
            b0.ChartName = "Option Analyzer - " + _spreads[0].Positions[0].Symbol;
            BMSWeb.BusinessLogic.Finance.ConvertSpreadToChartData(_spreads, b0);
            b0.ChartData[0].SeriesName = "PL-EXP";
            b0.ChartData[1].SeriesName = "PL-NOW";
            b0.ChartData[0].SeriesColor = "Green";
            b0.ChartData[0].SeriesColor = "Purple";
            b0.ChartData[0].BorderWidth = 2;
            b0.ChartData[1].BorderWidth = 2;
            b0 = await BBPAPI.Interface.Finance.GenerateChart2(b0);
            return b0;
        }

        public async Task<IActionResult> Analyzer()
        {
            ViewBag.CurSymbol = HttpContext.Session.GetObject<string>("quant_symbol");
            BBPChart b0 = await LoadAnalyzerPositions(ViewBag.CurSymbol);
            ViewBag.ChartData0 = b0.Base64Image;
            OptionsTradeModel otm = new OptionsTradeModel();
            await LoadSpreadsIntoOptionsModel(otm);
            return View(otm);
        }

        public async Task<IActionResult> AuthPanel()
        {
            return View();
        }
    }
}
