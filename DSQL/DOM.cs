using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblePay.BMS.DSQL
{
    public class DOMItem
    {

        public static string GetFormData(string sFormData, string sFieldName)
        {
            string[] vFormData = sFormData.Split("<row>");
            for (int i = 0; i < vFormData.Length; i++)
            {
                string[] cFormData = vFormData[i].Split("<col>");
                if (cFormData.Length > 2)
                {
                    string sID = cFormData[1];
                    string sValue = cFormData[2];
                    if (sID.ToLower() == sFieldName.ToLower())
                    {
                        return sValue;
                    }
                }
            }
            return String.Empty;
        }
		public static string GetFormData(string sFormData, string sFieldName, string sParentID)
		{
			string[] vFormData = sFormData.Split("<row>");
			for (int i = 0; i < vFormData.Length; i++)
			{
				string[] cFormData = vFormData[i].Split("<col>");
				if (cFormData.Length > 2)
				{
					string sID = cFormData[1];
					string sValue = cFormData[2];
                    string sFormParentID = cFormData[0];
					if (sID.ToLower() == sFieldName.ToLower() && sFormParentID.ToLower()==sParentID)
					{
						return sValue;
					}
				}
			}
			return String.Empty;
		}

		public string ID { get; set; }
        public string Value { get; set; }
        public double AsDouble
        {
            get
            {
                if (Value == null || Value == String.Empty)
                    return 0;
                return Convert.ToDouble(Value);
            }
        }


        public Int32 AsInt32
        {
            get
            {
                if (Value == null || Value == String.Empty)
                    return 0;
                return Convert.ToInt32(Value);
            }
        }


        public string ParentID { get; set; }
        public string GUID { get; set; }
        public DOMItem()
        {
            ID = String.Empty;
            Value = String.Empty;
            ParentID = String.Empty;
            GUID = Guid.NewGuid().ToString();
        }
    }

    public class TransformDOM
    {
        public string FormData { get; set; }
        public Dictionary<string, DOMItem> dictForm = new Dictionary<string, DOMItem>();
        public List<string> lParents = new List<string>();

        private void TransformFormData()
        {
            if (FormData == String.Empty)
                return;
            dictForm.Clear();
            lParents.Clear();
            string[] vRows = FormData.Split("<row>");
            for (int i = 0; i < vRows.Count(); i++)
            {
                string[] vCols = vRows[i].Split("<col>");
                if (vCols.Length > 2)
                {
                    DOMItem d = new DOMItem();
                    d.ParentID = vCols[0];
                    d.ID = vCols[1];
                    d.Value = vCols[2];
                    d.GUID = Guid.NewGuid().ToString();
                    dictForm[d.GUID] = d;
                    if (!lParents.Contains(d.ParentID) && d.ParentID != null)
                        lParents.Add(d.ParentID);
                }
            }
        }

        public DOMItem GetDOMItem(string sParentID, string sElementID)
        {
            DOMItem d = new DOMItem();

            foreach (KeyValuePair<string, DOMItem> kvp in dictForm)
            {
                if ((kvp.Value.ParentID == sParentID || sParentID == String.Empty)&& kvp.Value.ID == sElementID)
                {
                    return kvp.Value;
                }
            }
            return d;
        }
        public TransformDOM(string _FormData)
        {
            FormData = _FormData;
            TransformFormData();
        }
    }

}
