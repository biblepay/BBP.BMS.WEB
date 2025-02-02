using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BiblePay.BMS.Models
{
    public interface IBBPSvc
    {
        string FooRequest(string sData);
        string BarRequest(string sBar);
    }

    public class BBPSvc : IBBPSvc
    {
        public string FooRequest(string sData)
        {
            return "1";
        }

        public string BarRequest(string sBar)
        {
            return "2";
        }
    }
}
