using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiblePay.BMS.Models
{
    public class OptionsTradeModel
    {

        public OptionsTrade ActiveTrade { get; set; }
        public List<OptionsTrade> OptionsTradeList { get; set; }
        public List<DropDownValue> DropDownAction { get; set; }
        public List<DropDownValue> DropDownType { get; set; }

        public class DropDownValue
        {
            public string Caption { get; set; }
            public string Value { get; set; }
            public bool Selected { get; set; }
            public DropDownValue()
            {
                Caption = String.Empty;
                Value = String.Empty;
            }
        }
        public string GetDropDown(string sPropertyName, List<DropDownValue> l, string sSelectedValue, string sParentGuid)
        {
            string sHTML = "<select id='" + sPropertyName + "' data-parentid='" + sParentGuid + "'>\r\n";
            for (int i = 0; i < l.Count; i++)
            {
                DropDownValue dd = l[i];
                dd.Selected = sSelectedValue.ToLower() == dd.Caption.ToLower();
                string sRow = "<option value='" + dd.Value + "' {SELECTED}>" + dd.Caption + "</option>\r\n";
                string sNarr = dd.Selected ? "SELECTED" : "";
                sRow = sRow.Replace("{SELECTED}", sNarr);
                sHTML += sRow;
            }
            sHTML += "</select>";
            return sHTML;
        }

        public OptionsTradeModel()
        {
            OptionsTradeList  = new List<OptionsTrade>();
            ActiveTrade = new OptionsTrade();
            InitializeDropDowns();
		}

        private void InitializeDropDowns()
        {
			// drop downs
			DropDownAction = new List<DropDownValue>();
			DropDownAction.Add(new DropDownValue { Caption = "SELL", Value = "SELL", Selected = true });
			DropDownAction.Add(new DropDownValue { Caption = "BUY", Value = "BUY", Selected = false });
            DropDownType = new List<DropDownValue>();
			DropDownType.Add(new DropDownValue { Caption = "PUT", Value = "PUT", Selected = true });
			DropDownType.Add(new DropDownValue { Caption = "CALL", Value = "CALL", Selected = false });
		}
		public void SetActiveTrade(OptionsTrade ots)
        {
            ActiveTrade = ots;
		}
	}
    public class OptionsTrade
    {
        public string id { get; set; }
        public List<OptionsPosition> Positions { get; set; }
        public OptionsTrade()
        {
            id = String.Empty;
            Positions = new List<OptionsPosition>();    
        }
    }
    public class OptionsPosition
    {
        public bool Enabled { get; set; }
        public string Strategy { get; set; }
        public string Action { get; set; }
        public int Quantity { get; set; }
        public string Symbol { get; set; }
        public string ParentID { get; set; }
        public string Description 
        {
            get
            {
                string sNarr = Expiration.ToString();
                return sNarr;
            }

            set
            {

            }
        
        }
        public double Strike { get; set; }
        public string Type { get; set; }
        public double Price { get; set; }
        public DateTime Expiration { get; set; }
        public OptionsPosition()
        {
            ParentID = String.Empty;
            Strategy = String.Empty;

        }
    }
}
