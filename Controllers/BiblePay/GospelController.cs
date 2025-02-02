using BBPAPI;
using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BMSCommon.Common;

namespace BiblePay.BMS.Controllers
{
    public class GospelController : Controller
    {
        protected string GetIllustrationContent(string sURL)
        {
            if (sURL == "xmrinquiry")
            {
                sURL = "https://supportxmr.com";
            }
            // Reserved:  This same routine is supposed to handle the XMR pool payment owed page.
            sURL = sURL.Replace("javascript", "");
            sURL = sURL.Replace("script:", "");
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            string sCleansed = rgx.Replace(sURL.ToLower(), "");
            bool fOK = sURL.Contains("media/JesusChrist") || sURL.StartsWith("https://supportxmr.com") || sURL.StartsWith("https://www.freebibleimages.org/") || sURL.StartsWith("https://wiki.biblepay.org/");
            if (sURL.ToLower().Contains("script") || sURL.ToLower().Contains("javascript") || sURL.ToLower().Contains("(") || sURL.Contains(")"))
                fOK = false;
            if (sURL.ToLower().Contains("javas	cript"))
                fOK = false;
            if (sCleansed.Contains("java") || sCleansed.Contains("confirm"))
                fOK = false;
            if (fOK)
            {
                string sDec = System.Web.HttpUtility.UrlDecode(sURL);
                sDec = System.Web.HttpUtility.HtmlEncode(sDec);
                string sIframe = "<iframe width='95%' style='height: 90vh;' src='" + sDec + "'></iframe>";
                return sIframe;
            }
            else
            {
                return "404";
            }
        }
        public IActionResult GospelVideos()
        {
            return View();
        }
        protected string GetGospelContentFromFile(string sType)
        {
            string sPath = System.IO.Path.Combine(Global.msContentRootPath, "wwwroot/media/JesusChrist/" + sType + ".htm");

            if (sType.ToLower().Contains("anr"))
            {
                return "404";
            }
            if (!System.IO.File.Exists(sPath))
            {
                return "404";
            }

            string sData = System.IO.File.ReadAllText(sPath, Encoding.Default);
            sData = sData.Replace("”", "\"");
            char z = sData[167];
            int n1 = (int)z;
            char apostrophe = (char)"'"[0];
            sData = sData.Replace((char)0xfffd, apostrophe);
            return sData;
        }

        protected string GetStudies(bool fTestNet)
        {
            List<Article> lA = BBPAPI.Interface.Repository.GetDatabaseObjects<Article>("article");
            lA = lA.OrderBy(s => s.name).ThenBy(s => s.description).ToList();
            string html = String.Empty;
            for (int i = 0; i < lA.Count; i++)
            {
                Article a = lA[i];
                string sArticle = a.name;
                bool fHidden = false;
                if (sArticle == "ThreeDays" || sArticle == "NonChristianReligions")
                    fHidden = true;
                if (!fHidden)
                {
                    string row = "<a href=gospel/viewer?type=" + sArticle + ">" + a.description + "</a><br>";
                    html += row;
                }
            }
            return html;
        }
        protected string GetOrphanCollage(bool fTestNet)
        {
            List<SponsoredOrphan> lSPO = BBPAPI.Interface.Repository.GetDatabaseObjects<SponsoredOrphan>("sponsoredorphan");
            lSPO = lSPO.Where(s => s.Active == 1).ToList();
            lSPO = lSPO.Where(s => s.ChildID != "Genevieve Umba").ToList();
            lSPO = lSPO.Where(s => s.Charity.ToLower() != "sai").ToList();
            lSPO = lSPO.OrderBy(s => s.Charity).ThenBy(s=>s.Name).ToList();
            string sHTML = "<table><tr>";
            int iTD = 0;
            string sErr = String.Empty;
            for (int i = 0; i < lSPO.Count; i++)
            {
                SponsoredOrphan spo = lSPO[i];
                // Each Orphan should be a div with their picture in it
                string sMyBIO = spo.BioURL;
                string sName = spo.ChildID + " - " + spo.Charity;
                string sBioImg = spo.BioPicture;
                if (sBioImg != String.Empty)
                {
                    string sMyOrphan = "<td style='padding:7px;border:1px solid lightgrey' cellpadding=7 cellspacing=7><a href='" + sMyBIO + "'>" + sName
                        + "<br><img style='width:300px;height:250px' src='" + sBioImg + "'></a><br></td>";
                    sHTML += sMyOrphan;
                    iTD++;
                    if (iTD > 2)
                    {
                        iTD = 0;
                        sHTML += "<td width=30%>&nbsp;</td></tr><tr>";
                    }
                }
                else
                {
                    sErr += "<a href='" + sMyBIO + "'>Missing</a>";
                }
            }
            sHTML += "</TR></TABLE>";
            return sHTML;
        }

        protected string GetIllustrations()
        {
            string prefix = String.Empty;
            List<Illustration> dt = BBPAPI.Interface.Repository.GetChainBusinessObjects<Illustration>(HttpContext.GetCurrentUser());
            string html = String.Empty;

            for (int i = 0; i < dt.Count; i++)
            {
                Illustration i0 = dt[i];
                string myURL = dt[i].URL.ToString() ?? String.Empty;
                string sURL = System.Web.HttpUtility.UrlEncode(myURL);
                prefix = "<h3>All credit for these Illustrations goes to "
                        + "<a href = 'https://www.freebibleimages.org/illustrations/'> Free Bible Images </ a> !</h3>"
                        + "<small><font>NOTE:  Choose then click \"VIEW SLIDESHOW\" for the presentation.<br/></small>"
                        + "</font></h3><br>&nbsp;<p><p>";
                string URL2 = "<a href='gospel/viewer?target=" + sURL + "'>" + i0.Description + "</a>";
                bool fDeleted = false;
                if (sURL == "https://wiki.biblepay.org/End_Times_Scriptural_Basis")
                    fDeleted = true;
                if (i0.Description == "Parable of the Unforgiving Servant" || i0.Description == "Final Judgement of the World") fDeleted = true;
                if (!fDeleted)
                {
                    string row = URL2 + "<br>";
                    html += row;
                }
            }
            string output = prefix + html;
            return output;
        }
        protected string GetArticles(bool fTestNet,string type)
        {
            string prefix = String.Empty;
            List<Article> dt = BBPAPI.Interface.Repository.GetArticles(type);
            string html = String.Empty;
            for (int i = 0; i < dt.Count; i++)
            {
                string sArticle = dt[i].name.ToString();
                string sDesc = dt[i].description.ToString();
                string sNarr = sArticle + " - " + sDesc;
                string sURL = "";
                string URL2 = "<a href='gospel/viewer?target=" + sURL + "'>" + sNarr + "</a>";
                bool fDeleted = false;
                if (sURL == "https://wiki.biblepay.org/End_Times_Scriptural_Basis" || sDesc=="Grace and Mercy")
                    fDeleted = true;
                if (sDesc.Contains("Three Days of Darkness"))
                {
                    fDeleted = true;
                }

                if (!fDeleted)
                {
                    string row = URL2 + "<br>";
                    html += row;
                }
            }
            string output = prefix + html;
            return output;
        }



        protected string GetWikis(bool fTestNet, string type)
        {
            string prefix = String.Empty;
            List<Wiki> dt = BBPAPI.Interface.Repository.GetChainBusinessObjects<Wiki>(HttpContext.GetCurrentUser());
            string html = String.Empty;
            for (int i = 0; i < dt.Count; i++)
            {
                string sURL = dt[i].Name;
                string sDesc = dt[i].Description;

                if (type == "wiki")
                {
                    prefix = "<h3>Wiki Theological Articles</h3><br>&nbsp;<p><p><p><p>";
                }
                string URL2 = "<a href='gospel/viewer?target=" + sURL + "'>" + sDesc + "</a>";
                bool fDeleted = false;
                if (sURL == "https://wiki.biblepay.org/End_Times_Scriptural_Basis" || sDesc == "Grace and Mercy")
                    fDeleted = true;
                
                if (!fDeleted)
                {
                    string row = URL2 + "<br>";
                    html += row;
                }
            }
            string output = prefix + html;
            return output;
        }


        protected string GetAccountabilityPDFList()
        {
            string html = "<table>";
            for (int year = 2017; year <= DateTime.Now.Year; year++)
            {
                string row = "<tr><td><a href=/gospel/accountability?year=" + year.ToString() + ">Accounting Year " + year.ToString() + "</a></td></tr>\r\n";
                html += row;
            }
            html += "<tr><td><a href=/gospel/accountability?year=total>Grand Total (All Time)</a></td></tr>\r\n";
            html += "</table>";
            html += "<br><table><tr><td><a href=/gospel/accountability?type=cameroon-one>Cameroon-One Report</a></td></tr>\r\n";
            html += "<tr><td><a href=gospel/accountability?type=kairos>Kairos Report</a></td></tr>\r\n";
            html += "</table>";
            return html;
        }


        protected IActionResult ConvertHtmlToPDF(string HTML, string PDFFileName)
        {
            byte[] b = ConvertHtmlToBytes(HTML, PDFFileName);
            string sGuid = Guid.NewGuid().ToString() + ".html";
            return File(b, "application/html", sGuid);
        }
        protected IActionResult GenerateAccountingReport(int nYear)
        {
            List<BMSCommon.Model.Expense> lExpenses = BBPAPI.Interface.Repository.GetDatabaseObjects<Expense>("expense");
            List<Revenue> lRevenue = BBPAPI.Interface.Repository.GetDatabaseObjects<Revenue>("revenue");
            if (nYear != 0)
            {
                lExpenses = lExpenses.Where(s => Convert.ToDateTime(s.Added).Year == nYear).ToList();
                lRevenue = lRevenue.Where(s => Convert.ToDateTime(s.Added).Year == nYear).ToList();
            }
            else
            {
                lExpenses = lExpenses.Where(s => s.Added != null).ToList();
                lRevenue = lRevenue.Where(s => s.Added != null).ToList();

            }

            List<CharityReport> l = new List<CharityReport>();
            for (int i  = 0; i < lExpenses.Count; i++)
            {
                CharityReport cr = new CharityReport();
                cr.Added = Convert.ToDateTime(lExpenses[i].Added);
                cr.Type = "DR";
                cr.Notes = lExpenses[i].Charity;
                cr.Amount = lExpenses[i].Amount;
                l.Add(cr);
            }
            for (int i = 0; i < lRevenue.Count; i++)
            {
                CharityReport cr = new CharityReport();
                cr.Added = Convert.ToDateTime(lRevenue[i].Added);
                cr.Type = "CR";
                cr.Notes = lRevenue[i].Charity;
                cr.Amount = lRevenue[i].Amount;
                l.Add(cr);
            }

            var grouped = (from p in l
                           group p by new { month = p.Added.Month, year = p.Added.Year } into d
                           select new { dt = string.Format("{0}/{1}", d.Key.month, d.Key.year), count = d.Count() }).OrderBy(g => g.dt);
            List<CharityReport> lNew = JsonSerializer.Deserialize<List<CharityReport>>(JsonSerializer.Serialize(grouped));
            l = l.OrderBy(s => Convert.ToDateTime(s.Added)).ToList();
            string html = Report.GetTableHTML("Accounting Report", l, true);

            string accName = "BiblePay Accounting Year " + nYear.ToString() + ".pdf";
            return ConvertHtmlToPDF(html,accName);
        }

        public IActionResult GenerateCharityReport(string sCharity)
        {
            // pull in the OrphanExpense Object first
            List<OrphanExpense> l = BBPAPI.Interface.Repository.GetDatabaseObjects<OrphanExpense>("orphanexpense");
            List<OrphanExpense> lFiltered = l.Where(s => s.Charity.ToLower() == sCharity.ToLower()).ToList();
            lFiltered = lFiltered.OrderBy(s => Convert.ToDateTime(s.Added)).ToList();
            List<SponsoredOrphan> lSPO = BBPAPI.Interface.Repository.GetDatabaseObjects<SponsoredOrphan>("sponsoredorphan");
            string html = Report.GetCharityTableHTML(lFiltered, lSPO, 0);
            string accName = "Charity Report - " + sCharity + ".pdf";
            return ConvertHtmlToPDF(html,accName);
        }
        public IActionResult CoreGospel()
        {
            return View();
        }

        public IActionResult Wells()
        {
            return View();
        }

        public IActionResult Illustrations()
        {
            ViewBag.Illustrations = GetIllustrations();
            return View();
        }
        
        public IActionResult Collage()
        {
            ViewBag.Collage = GetOrphanCollage(IsTestNet(HttpContext));
            return View();
        }

        public IActionResult Accountability()
        {

            string typ = HttpContext.Request.Query["type"].ToString();
            string y = HttpContext.Request.Query["year"].ToString();
            if (typ != String.Empty)
            {
                return GenerateCharityReport(typ);
            }
            else if (y != String.Empty)
            {
                if (y == "total")
                {
                    return GenerateAccountingReport(0);
                }
                else
                {
                    return GenerateAccountingReport ((int)GetDouble(y));
                }
            }
            else
            {
                ViewBag.AccountabilityReports = GetAccountabilityPDFList();
                return View();
            }
        }

        public IActionResult WikiTheology()
        {
            ViewBag.Wiki = GetWikis(IsTestNet(HttpContext), "wiki");
            return View();
        }

        public IActionResult TheologicalStudy()
        {
            ViewBag.TheologicalStudy = GetStudies(IsTestNet(HttpContext));
            return View();
        }
        public IActionResult About()
        {
            // if they are not logged in, and they just came on, send them to the login
            if (HttpContext.Session.GetString("latent_login") != "1")
            {
                HttpContext.Session.SetString("latent_login", "1");
                User u = HttpContext.GetCurrentUser();
                if (!u.LoggedIn)
                {
                    Response.Redirect("../profile/login");
                }
            }
            return View();
        }
        public IActionResult Viewer()
        {
            string typ = HttpContext.Request.Query["type"].ToString();
            string t = HttpContext.Request.Query["target"].ToString();
            if (typ == "xmrinquiry")
            {
                ViewBag.GospelData = GetIllustrationContent(typ);
            }
            else if (typ != String.Empty)
            {
                ViewBag.GospelData = GetGospelContentFromFile(typ);
            }
            else if (t != String.Empty)
            {
                ViewBag.GospelData = GetIllustrationContent(t);
            }
            return View();
        }
    }
}

