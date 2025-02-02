using BBPAPI.Model;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using static BiblePay.BMS.DSQL.SessionHelper;

namespace BiblePay.BMS.Controllers
{
	public class PaginatorController : Controller
    {

        public class PaginatorObject
        {
            public string HTML { get; set; }
            public int StartRow { get; set; }
            public int EndRow { get; set; }
            public int NumRows { get; set; }
            public int RowsPerPage { get; set; }
            public bool IsRowVisible(int nRowNum)
            {
                bool fVisible = nRowNum >= StartRow && nRowNum <= EndRow;
                return fVisible;
            }
        }

        public static PaginatorObject MakePag(HttpContext h, string sPageName, int nNumRows, int nRowsPerPage)
        {
            string sCurPageNbr = h.Session.GetString("paginator_" + sPageName);
            int nCurPageNbr = sCurPageNbr.AsInt32();

            PaginatorObject p = new PaginatorObject();
            p.NumRows = nNumRows;
            int nNumbersVisible = 5;
            p.RowsPerPage = nRowsPerPage;
            p.StartRow = (int)(nCurPageNbr * nRowsPerPage);
            p.EndRow = (int)(p.StartRow + nRowsPerPage);
            string sPag = String.Empty;
            string sDiv = "<div class=\"pagination\">";
            sPag += sDiv;
            int nTargetRec2 = (nCurPageNbr - 1) * nRowsPerPage;
            if (nTargetRec2 >= (p.NumRows - 1))
            {
                nCurPageNbr = (p.NumRows / nRowsPerPage) - 1;
            }

            for (int i = 0; i <= (nNumbersVisible + 1); i++)
            {
                int nMyPageNbr = nCurPageNbr + i - 2;
                string sPageNbr = (nMyPageNbr + 1).ToString();
                string sActChar = sPageNbr;
                int nTargetRec = nMyPageNbr * nRowsPerPage;
                if (nTargetRec > p.NumRows)
                {
                    break;
                }
                if (i == 0)
                {
                    sActChar = "&laquo;";

                }
                if (i == 6)
                {
                    sActChar = "&raquo;";
                }
                string sActive = (nMyPageNbr == nCurPageNbr) ? "class='active'" : String.Empty;
                string sJS = "var e={}; e.Page='" + nMyPageNbr.ToString()
                   + "'; e.NumRows='" + nNumRows.ToString() + "'; e.PageName='" + sPageName + "'; e.Target='" + sPageName + "';"
                   + "DoCallback('Paginator_Click', e, 'paginator/processdocallback');return true;";
                string sCell = "<a onclick=\"" + sJS + "\" " + sActive + ">" + sActChar + "</a>\r\n";
                if (nMyPageNbr >= 0)
                {
                    sPag += sCell;
                }
            }
            sPag += "</div>";
            p.HTML = sPag;
            return p;
        }


        public class PaginationRecord
        {
            public string Page { get; set; }
            public string PageName { get; set; }
            public string Target { get; set; }
            public int NumRows { get; set; }
            public int RowsPerPage { get; set; }
        }

        [HttpPost]
        public JsonResult ProcessDoCallback([FromBody] ClientToServer o)
        {
            if (o == null)
                return null;

            ServerToClient returnVal = new ServerToClient();
            User u0 = GetUser(HttpContext);
            if (o.Action == "Log_Save")
            {
                dynamic oObj = o.ExtraData;
                string sLog = oObj.ToString();
                return Json(String.Empty);
            }
            else if (o.Action == "Paginator_Click")
            {
                PaginationRecord pr = Newtonsoft.Json.JsonConvert.DeserializeObject<PaginationRecord>(o.ExtraData);

                int nPageNbr = pr.Page.ToString().AsInt32();
                int nRecNbr = nPageNbr *  pr.RowsPerPage;
                if (nRecNbr > pr.NumRows)
                {
                    nPageNbr = (pr.NumRows / pr.RowsPerPage);
                }
                if (nRecNbr < 0)
                {
                    nPageNbr = 0;
                }
                string sKey = "paginator_" + pr.PageName;
                HttpContext.Session.SetString(sKey, pr.Page);
                string m = "location.href='" + pr.Target + "';";

                returnVal.returnbody = m;
                returnVal.returntype = "javascript";
                string o1 = JsonConvert.SerializeObject(returnVal);
                return Json(o1);
            }
            else
            {
                throw new Exception("Unknown method.");
            }
        }
    }
}

