using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BiblePay.BMS.Models
{
    public class BlackBoxEditModel
    {
        public string Code  { get; set; }
        public string Report { get; set; }
        public BlackBoxEditModel()
        {
            Code = String.Empty;
            Report = String.Empty;
        }
    }

}
