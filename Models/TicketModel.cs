using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiblePay.BMS.Models
{
    public class TicketModel
    {
        public List<SelectListItem> DispositionList
        {
           get; 
           set;
        }

        public List<SelectListItem> AssignedToList
        {
            get; set;
        }

        [Required(ErrorMessage = "Value is required")]
        // Specify AllowHtml attribute on MVC application alone
        public string Value { get; set; }

        public TicketModel()
        {
            DispositionList = new List<SelectListItem>();
            AssignedToList = new List<SelectListItem>();
            Value = String.Empty;
        }

    }

}
