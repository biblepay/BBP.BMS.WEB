using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using static BMSCommon.Common;
using static BMSCommon.Extensions;


namespace BiblePay.BMS.Controllers
{
    public class ProposalController : Controller
    {

        protected string GetVote(string ID, string sAction)
        {
            if (ID == "")
            {
                string sData = "(N/A)";
                return sData;
            }
            string sNarr = "From the RPC console in biblepaycore, enter the following command:<br><br>gobject vote-many " + ID + " funding " + sAction;
            string sJS = "<a style='cursor:pointer;' onclick=\"var e={}; e.Narr='" + sNarr + "';DoCallback('show_modal',e);\">Vote " + sAction + "</a>";
            return sJS;
        }

        public static string btnSubmitProposal_Click(HttpContext h, string sData)
        {
            Proposal p = new Proposal();
            p.id = Guid.NewGuid().ToString();
            string sError = String.Empty;
            p.Name = GetFormData(sData, "txtName");
            p.BBPAddress = GetFormData(sData, "txtAddress");
            p.Amount = GetDouble(GetFormData(sData, "txtAmount"));
            p.Added = DateTime.Now;
            User u0 = h.GetCurrentUser();
            p.NickName = u0.NickName;
            p.ERC20Address = u0.ERC20Address;

            p.ExpenseType = GetFormData(sData, "ddCharity");
            p.URL = GetFormData(sData, "txtURL");
            p.Chain = IsTestNet(h) ? "test" : "main";

            if (p.Name.Length < 5)
                sError = "Proposal name too short.";
            if (p.NickName.IsNullOrEmpty())
                sError = "Please log in first so that your nickname can be populated on the proposal.";
            if (p.BBPAddress.Length < 24)
                sError = "Address must be valid.";
            if (p.Amount <= 0 || p.Amount > 13000000)
                sError = "Amount must be populated.";

            if (!u0.LoggedIn)
                sError = "You must be logged in.";

            BBPNetAddress ba = new BBPNetAddress();
            ba.TestNet = IsTestNet(h);
            ba.Address = p.BBPAddress;

            bool fValid = BBPAPI.Interface.WebRPC.ValidateBBPAddress(ba).Result;

            if (!fValid)
            {
                sError = "Address is not valid for this chain.";
            }

            if (GetDouble(p.Amount) > 17000000 || GetDouble(p.Amount) < 1)
            {
                sError = "Amount is too high (over superblock limit) or too low.";
            }

            double nMyBal = GetDouble(GetAvatarBalance(h, false));
            if (nMyBal < 2501)
                sError = "Balance too low.";

            if (sError != String.Empty)
            {
                string sJson1 = MsgBoxJson(h, "Error", "Error", sError);
                return sJson1;
            }
            // Submit
            p.TestNet = IsTestNet(h);
            p.User = h.GetCurrentUser();
            BBPAPI.Interface.Repository.GobjectSerialize(p);

            string sJson = MsgBoxJson(h, "Success", "Success", "Thank you.  Your proposal will be submitted in six blocks.");
            return sJson;
        }

        protected string GetProposalsList(HttpContext h)
        {
            string sChain = IsTestNet(h) ? "test" : "main";
            BBPAPI.Interface.WebRPC.SubmitProposals(h.GetCurrentUser());


            List<Proposal> dt = BBPAPI.Interface.Repository.GetDatabaseObjects<Proposal>("proposal");
            dt = dt.Where(s => s.Chain == sChain && DateTime.Now.Subtract(s.Added).TotalSeconds < 86400 * 30).ToList();
            dt = dt.OrderByDescending(s => Convert.ToDateTime(s.Added)).ToList();
            string html = "<table class=saved><tr><th>UserName<th>Expense Type<th>Proposal Name<th>Amount<th>PrepareTXID<th>URL<th>Chain<th>Updated<th>Submit TXID<th>Vote Yes<th>Vote No</tr>\r\n";
            for (int y = 0; y < dt.Count; y++)
            {
                string sURLAnchor = "<a href='" + dt[y].URL + "' target=_blank>View Proposal</a>";
                string sID = dt[y].SubmitTXID;
                string div = "<tr>"
                    + "<td>" + dt[y].NickName
                    + "<td>" + dt[y].ExpenseType
                    + "<td>" + dt[y].Name
                    + "<td>" + dt[y].Amount.ToString();
                string sPrepareTXID = dt[y].PrepareTXID ?? String.Empty;
                div += "<td><small>"
                    + Mid(sPrepareTXID, 0, 10)
                    + "</small>"
                    + "<td>" + sURLAnchor
                    + "<td>" + dt[y].Chain.ToString()
                    + "<td>" + dt[y].Updated.ToMilitaryTime()
                + "<td><font style='font-size:7px;'>" + dt[y].SubmitTXID + "</font>"
                + "<td>" + GetVote(sID, "yes") + "<td>" + GetVote(sID, "no");
                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }

        public IActionResult ProposalAdd()
        {
            List<BMSCommon.Model.DropDownItem> ddCharity = new List<DropDownItem>();
            ddCharity.Add(new DropDownItem{ key0 = "Charity", text0 = "Charity" });
            ddCharity.Add(new DropDownItem{ key0 = "PR", text0 = "PR" });
            ddCharity.Add(new DropDownItem{ key0 = "P2P", text0 = "P2P" });
            ddCharity.Add(new DropDownItem{ key0 = "IT", text0 = "IT" });
            ddCharity.Add(new DropDownItem{ key0 = "XSPORK", text0 = "XSPORK" });
            ViewBag.ddCharity = ListToHTMLSelect(ddCharity, "Charity");
            return View();
        }

        public IActionResult ProposalList()
        {
            ViewBag.ProposalList = GetProposalsList(HttpContext);
            return View();
        }

    }
}
