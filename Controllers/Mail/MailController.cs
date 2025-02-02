using BBPAPI;
using BiblePay.BMS.DSQL;
using BiblePay.BMS.Extensions;
using BiblePay.BMS.Models;
using BMSCommon.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static BiblePay.BMS.DSQL.ControllerExtensions;

namespace BiblePay.BMS.Controllers
{
    public class MailController : Controller
    {

		public async Task<JsonResult> SendMail([FromBody] BMSCommon.Model.ClientToServer o)
		{
			MailInbox m = new MailInbox();
			TransformDOM t = new TransformDOM(o.FormData);
			string sSubject = t.GetDOMItem("", "txtSubject").Value;
			string sCC = t.GetDOMItem("", "txtCC").Value;
			string sTo = t.GetDOMItem("", "txtTo").Value;
			string sBody = t.GetDOMItem("", "txtEmailBody").Value;
			BBPOutboundEmail mm = new BBPOutboundEmail();
			mm.To.Add(sTo);
			mm.BCC.Add("rob@biblepay.org");
			mm.Subject = sSubject;
			mm.Body = sBody;
			mm.IsBodyHTML = false;
			DACResult r = BBPAPI.Interface.Core.SendEmail(mm).Result;
			if (r.Error != string.Empty)
			{ 
				string s2 = DSQL.UI.GetModalDialogJson("Error Sending Email", r.Error);
				return Json(s2);
			}
			else
			{
				string sMyJS = "closeModalByDataTarget('panel-compose');";
				return this.ReturnJS(sMyJS);
			}

		}

		public async Task<JsonResult> RetrieveMailItem([FromBody] BMSCommon.Model.ClientToServer o)
		{
			MailInbox m = new MailInbox();
			dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(o.ExtraData);
			string sFN = a.messagefilename.Value;
			BBPEmailModel r1 = await BBPAPI.Interface.EMail.GetMailItem(HttpContext.GetCurrentUser(),sFN);
			m.activeItem = r1;
			return await this.RenderDivToClient<MailInbox>("Mail/_MailPreview", m, "divMailPreview", true);
		}

		public async Task<IActionResult> InboxGeneral()
		{
			MailInbox m = new MailInbox();
			m.activeItem = new BBPEmailModel();
			m.activeItem.Subject = "Hello";
			m.activeItem.From = "Your Name";
			m.activeItem.To = new System.Collections.Generic.List<string>();
			m.activeItem.To.Add("Your Name");
			return View(m);
		}

		public IActionResult InboxRead() => View();
		public IActionResult InboxWrite() => View();

	}
}
