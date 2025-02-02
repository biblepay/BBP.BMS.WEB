#region Copyright Syncfusion Inc. 2001-2023.
// Copyright Syncfusion Inc. 2001-2023. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using BBPAPI;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace BiblePay.BMS.Controllers
{
	public partial class GridController : Controller
    {
        public class ColumnChartData
        {
            public DateTime Date { get; set; }
            public double ULPrice;
            public double PutExtrinsic;
            public double CallExtrinsic;
            public double Strike;
        }

        public IActionResult InlineEditing()
        {

            List<PhoneRate> dt = BBPAPI.Interface.Phone.GetRatesReport(1000);

            //var order = OrdersDetails.GetAllRecords();
            ViewBag.RateSource = dt;
            ViewBag.ddDataSource = new string[] { "Top", "Bottom" };

            List<ColumnChartData> chartData = new List<ColumnChartData>
            {
              
            };
            ViewBag.dataSource = chartData;

            string sql = "exec  GetReport '6-1-2008','10-1-2008','symbol'; ";
            DataTable dt1 = null;
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                double ulPrice = dt1.Rows[i]["ULPrice"].ToString().ToDouble();
                string dataDate = dt1.Rows[i]["datadate"].ToString();
                ColumnChartData ccd =new ColumnChartData();
                ccd.ULPrice = ulPrice;
                ccd.Date = Convert.ToDateTime(dataDate);
                ccd.PutExtrinsic = dt1.Rows[i]["PutExtrinsic"].ToString().ToDouble() * 10;
                ccd.CallExtrinsic = dt1.Rows[i]["CallExtrinsic"].ToString().ToDouble() * 10;
                ccd.Strike = dt1.Rows[i]["Strike"].ToString().ToDouble();

                if (ccd.Strike == 100 || ccd.Strike == 150)
                {
                    chartData.Add(ccd);
                }
            }

            return View();
        }
    }
}