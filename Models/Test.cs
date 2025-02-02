using BBPAPI;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BiblePay.BMS.Models
{

    public class DummyModel
    {
        public List<DummyObject> MyItems = new List<DummyObject>();
        public List<DummyObject> MyCols = new List<DummyObject>();
    }
    public class DummyObject
    {
        public string A;
        public string B;
        public string C;
        public string FieldName;
    }

    public class City
    {
        public int Id { get; set; }
        public string Name1 { get; set; }
        public string State { get; set; }
        public int Zip { get; set; }

        public string myfield = "a";
        public DateTime StartDate { get; set; }
    }

    public class MailInbox
    {
        public List<BBPEmailModel> mailMessages = new List<BBPEmailModel>();
        public BBPEmailModel activeItem = null;
    }

}


