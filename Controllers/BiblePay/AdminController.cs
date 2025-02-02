using BiblePay.BMS.Extensions;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.ControllerExtensions;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BiblePay.BMS.DSQL.UI;
using static BMSCommon.Common;

namespace BiblePay.BMS.Controllers
{

    public class AdminController : Controller
    {

        public IActionResult MarketingDashboard()
        {
            ViewBag.DynamicTable = DSQL.UI.GetBasicTable("t", "the new dynamic table");
            ViewBag.Accordian = DSQL.UI.GetAccordian("a1", "[Accordian] Collapse", "<br>A dynamic table inside a dynamic accordian.");
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ProcessDoCallback([FromBody] ClientToServer o)
        {
            ServerToClient returnVal = new ServerToClient();
            User u0 = GetUser(HttpContext);

            if (o.Action == "timeline_post")
            {
                string sData = GetFormData(o.FormData, "txtBody");
                string sPaste = GetFormData(o.FormData, "divPaste");
                dynamic oExtra = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sParentID = oExtra.parentid.Value;
                string sError = String.Empty;
                if (String.IsNullOrEmpty(sParentID))
                    sError = "Parent ID invalid.";

                Timeline t = new Timeline();
                t.Body = sData;
                t.dataPaste = sPaste;
                t.ERC20Address = u0.ERC20Address;
                t.BBPAddress = u0.BBPAddress;
                t.Added = DateTime.Now;
                t.ParentID = sParentID;
                t.id = Guid.NewGuid().ToString();
                
                if (sError != String.Empty)
                {
                    string s1 = MsgBoxJson(HttpContext, "Timeline Post", "Error", sError);
                    return Json(s1);
                }
                BBPAPI.Interface.Repository.SaveTimeLine(t);

                // Redirect user to the Timeline to show the post
                return this.ReturnJS("location.reload();");
            }
            else if (o.Action == "persist_theme")
            {
				dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sTheme = a.Extra.themeOptions;
				//mod-skin-dark
				//mod-skin-????
				HttpContext.Session.SetString("theme", sTheme);
            }
            else if (o.Action == "video_search")
            {
                string sSearch = GetFormData(o.FormData, "txtSearcher");
                return this.ReturnURL("/bbp/videos?search=" + sSearch);
            }
            else if (o.Action == "scrapy_paste")
            {
                string sBody = GetFormData(o.FormData, "txtBody");
                string sParsed = String.Empty;
                for (int i = 0; i < 3; i++)
                {
                    sParsed = await WebPageScraper(sBody);
                    if (sParsed != String.Empty)
                        break;
                }

                string m = "var p = document.getElementById('divPaste');p.innerHTML=\"" + sParsed + "\";";
                return this.ReturnJS(m);
            }
            else if (o.Action == "admin_addexpense")
            {
                // Add revenue record, add expense record, and add OrphanExpense distribution
                List<SponsoredOrphan> dt = BBPAPI.Interface.Repository.GetDatabaseObjects<SponsoredOrphan>("sponsoredorphan");
                string sCharity = GetFormData(o.FormData, "txtCharityName").ToUpper();
                dt = dt.Where(s => s.Charity.ToLower() == sCharity.ToLower()).ToList();
                string sNotes = GetFormData(o.FormData, "txtNotes");
                string sAdded = GetFormData(o.FormData, "txtAdded");
                sAdded = Convert.ToDateTime(sAdded).ToString();
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sOpType= a.OpType.Value;
                string sError = String.Empty;
                if (sOpType != "PAYMENT" && sOpType != "CHARGE")
                    sError = "Invalid Operation Type";
                if (sCharity != "CAMEROON-ONE" && sCharity != "SAI" && sCharity != "KAIROS")
                    sError = "Invalid Charity Name";
                if (sAdded == String.Empty)
                    sError = "Added date must be populated.";
                double nExpenseTotal = GetDouble(GetFormData(o.FormData, "txtAmount"));
                if (nExpenseTotal < 1)
                    sError = "Invalid expense amount.  It should be above zero.";
                if (sNotes == String.Empty)
                {
                    sError = "Sorry, notes must be populated.";
                }

                if (sError != String.Empty)
                {
                    string s3 = MsgBoxJson(HttpContext, "Error", "Error", sError);
                    return Json(s3);
                }
                if (dt.Count == 0)
                    sError = "No rows.";

                if (sError != String.Empty)
                {
                    string s3 = MsgBoxJson(HttpContext, "Error", "Error", sError);
                    return Json(s3);
                }

                int nModifier = 0;
                if (sOpType == "PAYMENT")
                {
                    nModifier = -1;
                }
                else
                {
                    nModifier = 1;
                }
                double nAmount = nModifier * (nExpenseTotal / dt.Count);
                
                for (int i = 0; i < dt.Count; i++)
                {
                    OrphanExpense oe = new OrphanExpense();
                    oe.Added = Convert.ToDateTime(sAdded);
                    oe.Amount = nAmount;
                    oe.Charity = sCharity;
                    oe.URL = String.Empty;
                    oe.HandledBy = "bible_pay";
                    oe.ChildID = dt[i].ChildID;
                    double nOldBalance = BMS.Report.GetChildBalance(HttpContext.GetCurrentUser(),oe.ChildID);
                    oe.Balance = nOldBalance += nAmount;
                    oe.Notes = sNotes;
                    oe.id = Guid.NewGuid().ToString();
                    bool f2 = BBPAPI.Interface.Repository.StoreData<OrphanExpense>("options.orphanexpense", oe, oe.id);
                }
                if (sOpType == "PAYMENT")
                {
                    // Add the orphan expense record
                    Expense x = new Expense();
                    x.Added = sAdded;
                    x.Amount = nExpenseTotal;
                    x.URL = "";
                    x.Charity = sCharity;
                    x.HandledBy = "bible_pay";
                    x.Notes = sNotes;
                    x.id = Guid.NewGuid().ToString();
                    bool f3 = BBPAPI.Interface.Repository.StoreData<Expense>("options.expense", x, x.id);
                    // Add the Revenue Record
                    Revenue r = new Revenue();
                    r.Added = sAdded.ToDateTime();
                    r.BBPAmount = 0;
                    r.BTCRaised = 0;
                    r.Amount = nExpenseTotal;
                    r.Notes = "[Donation for ] " + sNotes;
                    r.Charity = sCharity;
                    r.HandledBy = "bible_pay";
                    r.id = Guid.NewGuid().ToString();
                    bool f4 = BBPAPI.Interface.Repository.StoreData<Revenue>("options.revenue", r, r.id);
                }
                string s4 = MsgBoxJson(HttpContext, "Success", "Success", "Success");
                return Json(s4);
            }
            else if (o.Action == "video_delete")
            {
                dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string sID = a.id.Value;
                if (false && sID.Length > 1)
                {
                    // TODO: Figure out how to honor the permissions on user record and remove the video (IE DCMA takedown request):
                    return Json("Failed");
                }
            }
            else if (o.Action == "scratch_submit")
            {
                string sScratchID = GetFormData(o.FormData, "txtScratchID");
                string sScratchValue = GetFormData(o.FormData, "txtScratchValue");
                string sError = String.Empty;
                if (sScratchID == String.Empty || sScratchValue == String.Empty)
                {
                    sError = "Values must be populated";
                }
                else
                {
                    //BBPAPI.ServiceInit.SetScratch(sScratchID, sScratchValue);
                }
                string sNarr = sError != String.Empty ? sError : "Successfully saved.  Your data will be saved for 10 minutes and then erased.  "
                    + "NOTE: Once you access the data, we will rewrite the value with 'accessed' so that it cannot be accessed again (it can only be accessed once).";
                string modal = DSQL.UI.GetModalDialog("Results", sNarr);
                returnVal.returnbody = modal;
                returnVal.returntype = "modal";
                string outdata = JsonConvert.SerializeObject(returnVal);
                return Json(outdata);
            }
            else if (o.Action == "proposal_add")
            {
                string sRedir = ProposalController.btnSubmitProposal_Click(HttpContext, o.FormData);
                return Json(sRedir);
            }
            else if (o.Action == "show_modal")
            {
                //Extra Data
                dynamic msg = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
                string modal = DSQL.UI.GetModalDialog("Information", msg.Narr.Value);
                returnVal.returnbody = modal;
                returnVal.returntype = "modal";
                string outdata = JsonConvert.SerializeObject(returnVal);
                return Json(outdata);
            }
            else if (o.Action == "test_1")
            {
                // MessagePage
                DSQL.UI.MsgBox(HttpContext, "BBP Long Process", "Error occurred", "object not set to an object.", false);
                string m = "location.href='bbp/messagepage';";
                returnVal.returnbody = m;
                returnVal.returntype = "javascript";
                string o1 = JsonConvert.SerializeObject(returnVal);
                return Json(o1);
            }
            else
            {
                Log("Unknown Method" + o.Action);

                throw new Exception("Unknown method " + o.Action);
            }
            return Json(String.Empty);
        }

        public IActionResult AnalyticsDashboard() => View();
    }
}
