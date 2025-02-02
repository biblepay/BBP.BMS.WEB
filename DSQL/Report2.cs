using BBPAPI;
using BBPAPI.Model;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.UIWallet;
using static BMSCommon.Common;
using static BMSCommon.Encryption;

namespace BiblePay.BMS
{
    public static class Report
    {
        public static string DoFormat(double myNumber, double nDiscPct = 0)
        {
            if (nDiscPct > 0)
            {
                double nDiscAmt = myNumber * (nDiscPct / 100);
                double nNewAmt = myNumber - nDiscAmt;
                myNumber = nNewAmt;
            }
            var s = string.Format("{0:0.00}", myNumber);
            return s;
        }

        public static string GetTableBeginning(string sTableName)
        {
            string css = "<style> html {    font-size: 1em;    color: black;    font-family: verdana }  .r1 { font-family: verdana; font-size: 10; }</style>";
            string logo = "https://www.biblepay.org/wp-content/uploads/2018/04/Biblepay70x282_96px_color_trans_bkgnd.png";
            string sLogoInsert = "<img width=300 height=100 src='" + logo + "'>";
            string HTML = "<HTML>" + css + "<BODY><div><div style='margin-left:12px'><TABLE class=r1><TR><TD width=70%>" + sLogoInsert
                + "<td width=25% align=center>" + sTableName + "</td><td width=5%>" + DateTime.Now.ToShortDateString() + "</td></tr>";
            HTML += "<TR><TD><td></tr>" + "<TR><TD><td></tr>" + "<TR><TD><td></tr>";
            HTML += "</table>";
            return HTML;
        }
        public static string GetCharityTableHTML(List<OrphanExpense> c, List<SponsoredOrphan> lSPO, int iMaxRows)
        {
            string css = "<style> html {    font-size: 4px;    color: black;    font-family: verdana }  .r1 { font-family: verdana; font-size: 4px; }</style>";
            string logo = "https://www.biblepay.org/wp-content/uploads/2018/04/Biblepay70x282_96px_color_trans_bkgnd.png";
            string sLogoInsert = "<img width=300 height=100 src='" + logo + "'>";
            string HTML = "<HTML>" + css + "<BODY><div><div style='margin-left:12px'><TABLE class=r1><TR><TD width=95%>" + sLogoInsert
                + "<td width=5% align=right>Accountability</td><td>" + DateTime.Now.ToShortDateString() + "</td></tr>";
            HTML += "<TR><TD><td></tr>" + "<TR><TD><td></tr>" + "<TR><TD><td></tr>";
            HTML += "</table>";
            string header = "<TR><Th>Date<Th>Amount<th>Charity<th>Child Name<th>Child ID<th>Balance<Th width=30%>Notes</tr>";
            HTML += "<table width=100%>" + header + "<tr><td colspan=7 width=100%><hr></tr>";
            double nDR = 0;
            double nCR = 0;
            double nTotal = 0;
            string sOldDate = String.Empty;
            double nBalance = 0;
            int iStart = 0;
            for (int i = 0; i < c.Count; i++)
            {
                OrphanExpense oRow = c[i];
                double dAmt = GetDouble(oRow.Amount);
                string sType = dAmt >= 0 ? "DR" : "CR";
                nTotal += GetDouble(oRow.Balance);
                string dt1 = Convert.ToDateTime(oRow.Added).ToShortDateString();
                SponsoredOrphan mySPO = lSPO.Where(s => s.ChildID == oRow.ChildID).FirstOrDefault();
                string sChildName = mySPO == null ? oRow.ChildID : mySPO.Name; 
                string row = "<tr><td align=right>" + dt1 + "<td align=right>" 
                    + DoFormat(oRow.Amount) 
                    + "<td align=right>" + oRow.Charity
                    + "<td align=right>" + sChildName
                    + "<td align=right>" + oRow.ChildID 
                    + "<td align=right>" + DoFormat(oRow.Balance)
                    + "<td align=right><small>" + oRow.Notes + "</small></tr>";
                // Add the totals
                if (dAmt > 0)
                {
                    nDR += dAmt;
                }
                else
                {
                    nCR += dAmt;
                }
                nBalance += dAmt;

                if (sOldDate != dt1 && i > 1 && (i >= iStart || iMaxRows == 0))
                {
                    HTML += "<tr><td colspan=10><hr></td></tr>";
                }
                sOldDate = dt1;

                if (i >= iStart || iMaxRows == 0)
                {
                    HTML += row;
                }
            }
            HTML += "<tr><td>&nbsp;</td></tr>";
            HTML += "<tr><td>BALANCE:<td><td>" + DoFormat(nBalance) + "</tr>";
            HTML += "</body></html>";
            return HTML;
        }
        public static string GetTableHTML(string sReportName, List<CharityReport> dt, bool fShowCreditAndDebit = false)
        {
            //Added, Amount, Type, Charity or Notes
            StringBuilder HTML = new StringBuilder();
            try
            {
                HTML.Append(GetTableBeginning(sReportName));
                string sCols = "Type;Added;Amount;Notes";
                string[] vCols = sCols.Split(new string[] { ";" }, StringSplitOptions.None);
                string sHeader = "<tr>";
                for (int i = 0; i < vCols.Length; i++)
                {
                    sHeader += "<th>" + vCols[i] + "</th>";
                }
                sHeader += "</tr>";
                HTML.Append("<table width=100%>" + sHeader + "<tr><td colspan=5 width=100%><hr></tr>");
                double nTotal = 0;
                double nTotalDr = 0;
                double nTotalCr = 0;
                for (int i = 0; i < dt.Count; i++)
                {
                    CharityReport c1 = dt[i];
                    string sRow = "<tr>";
                    sRow += "<td align=right>" + dt[i].Type.ToString() + "</td>";
                    sRow += "<td align=right>" + dt[i].Added.ToString() + "</td>";
                    sRow += "<td align=right>" + dt[i].Amount.ToString() + "</td>";
                    sRow += "<td align=right>" + dt[i].Notes.ToString() + "</td>";
                    sRow += "</tr>";
                    double nAmt = c1.Amount;
                    if (c1.Type == "CR")
                    {
                        nAmt = nAmt * -1;
                    }
                        nTotal += nAmt;
                        if (nAmt > 0)
                        {
                            nTotalDr += nAmt;
                        }
                        else
                        {
                            nTotalCr += nAmt;
                        }
                    HTML.Append(sRow);
                }

                HTML.Append("<tr><td>&nbsp;</td></tr>");

                if (fShowCreditAndDebit)
                {
                    HTML.Append("<tr><td>Total Expenses: <td><td>" + nTotalDr.ToString() + "</tr>");
                    HTML.Append("<tr><td>Total Revenue: <td><td>" + nTotalCr.ToString() + "</tr>");
                }

                if (!fShowCreditAndDebit)
                {
                    HTML.Append("<tr><td>TOTAL:<td><td>" + nTotal.ToString() + "</tr>");
                }

                HTML.Append("</body></html>");
                return HTML.ToString();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            return HTML.ToString();
        }

        public static double GetChildBalance(User u,string sChildID)
        {
            List<OrphanExpense> lOOE = BBPAPI.Interface.Repository.GetDatabaseObjects<OrphanExpense>("orphanexpense");
            lOOE = lOOE.Where(s => s.ChildID.ToLower() == sChildID.ToLower()).ToList();
            lOOE = lOOE.OrderByDescending(s => Convert.ToDateTime(s.Added)).ToList();
            double nBal = lOOE[0].Balance;
            return nBal;
        }
    }
}
