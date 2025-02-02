using BMSCommon;
using System;
using System.Collections.Generic;
using static OptionsShared.Model.Schwab;
using QFramework;
using BBPModelShared;

namespace BMSWeb.BusinessLogic
{
	public class Finance
	{

		public class FPoint
		{
			public int X { get; set; }
			public int Y { get; set; }
		}


		public static DateTime ThirdFriday(DateTime dt)
		{
			string sStart = dt.Month.ToString() + "/1/" + dt.Year.ToString();
			DateTime dtStart = Convert.ToDateTime(sStart);
			int iCt = 0;
			for (int i = 0; i < 30; i++)
			{
				DateTime newDate = dtStart.AddDays(i);
				if (newDate.DayOfWeek == DayOfWeek.Friday)
				{
					iCt++;
					if (iCt == 3)
					{
						return newDate;
					}
				}
			}
			return dt;
		}


		public static async void UpdateSpreadsWithMarketData(string sSymbol, List<SchwabSpread> s)
		{
			if (s == null || s.Count < 1)
			{
				return;
			}
			SymbolRequest sr = new SymbolRequest();
			sr.Symbol = sSymbol;
			List<SchwabPrice> lChain = await BBPAPI.Interface.Finance.GetOptionChain(sr);
			for (int i = 0; i < s.Count; i++)
			{
				SchwabSpread s0 = s[i];
				for (int j = 0; j < s0.Positions.Count; j++)
				{
					// for any position, match the strike and option to a live option
					SchwabPrice myPrice = GetOptionFromChain(lChain, s0.Positions[j]);
					if (myPrice != null)
					{
						s0.Positions[j].MidPrice = myPrice.MidPrice;
						s0.Positions[j].UnderlyingPrice = myPrice.UnderlyingPrice;
						s0.Positions[j].Volatility = myPrice.Volatility / 100;
					}
				}
			}
		}


		public static SchwabPrice GetOptionFromChain(List<SchwabPrice> lChain, SchwabPrice myPosition)
		{
			//Price pNew = new BMSCommon.Model.Price();
			for (int i = 0; i < lChain.Count; i++)
			{
				if (myPosition.Symbol == lChain[i].Symbol
				&& myPosition.Strike == lChain[i].Strike
					&& myPosition.Type == lChain[i].Type)
				{

					if (myPosition.Expiration.ToShortDateString() == lChain[i].Expiration.ToShortDateString())
					{
						return lChain[i];
					}
				}
			}
			return null;
		}


		private static DateTime GetEarliestExpiration(List<SchwabSpread> s0)
		{
			DateTime dt = Convert.ToDateTime("12-31-2039");
			foreach (SchwabSpread s in s0)
			{
				for (int i = 0; i < s.Positions.Count; i++)
				{
					SchwabPrice p = s.Positions[i];
					if (p.Expiration < dt)
					{
						dt = p.Expiration;
					}
				}
			}
			return dt;
		}
		public static void ConvertSpreadToChartData(List<SchwabSpread> s1,BBPChart b0)
		{
			ChartData cd0 = new ChartData();
			ChartData cd1 = new ChartData();
			cd0.SeriesColor = "Purple";
			cd1.SeriesColor = "Green";

			// ANALYZER ENTRY POINT
			if (s1.Count < 1)
			{
				return;
			}
			double nUL = s1[0].Positions[0].UnderlyingPrice;
			DateTime dtAsOf = DateTime.Now.AddDays(7);
			double nOrigValue = BusinessLogic.Finance.GetStartValueOfSpreads(s1, nUL, dtAsOf);
			// Value at expiration
			DateTime dtEarliestExp = GetEarliestExpiration(s1);
			double nEndValue = BusinessLogic.Finance.GetTheoreticalValueOfSpreads(s1, nUL, dtEarliestExp);
			// for the spread go through the range
			int nStart = (int)(nUL * .25);
			int nEnd = (int)(nUL * 2.25);
			double nSteps = 100.01;
			double nStepRate = ((nEnd - nStart) / nSteps);
			int pos = 0;
			if (nUL <= 0)
			{
				return;
			}
			for (double iUL = nStart; iUL <= nEnd; iUL += nStepRate)
			{
				ChartPoint p1 = new ChartPoint();
				double nValue = BusinessLogic.Finance.GetTheoreticalValueOfSpreads(s1, iUL, dtAsOf);
				double nPL =nValue - nOrigValue;
				p1.XValue = (int)iUL;
				p1.YValue = (int)nPL;
				cd0.ChartPoints.Add(p1);
				ChartPoint p2  = new ChartPoint();	
				double nExpSimValue = BusinessLogic.Finance.GetTheoreticalValueOfSpreads(s1, iUL, dtEarliestExp);
				double nPLEnd =nExpSimValue -  nOrigValue;
				p2.XValue = (int)iUL;
				p2.YValue = (int)nPLEnd;
				cd1.ChartPoints.Add(p2);
				if (iUL >= 130 && iUL <= 140)
				{
					bool fBreak = true;
				}
				pos++;
			}
			b0.ChartData.Add(cd0);
			b0.ChartData.Add(cd1);
			return;
		}


		public static double GetTheoreticalValueOfSpreads(List<SchwabSpread> s0, double nULPrice, DateTime dtAsOf)
		{
			double nTotal = 0;
			double nRFR = .05;
			double nDiv = 0;
			for (int y = 0; y < s0.Count; y++)
			{
				SchwabSpread s00 = s0[y];
				for (int i = 0; i < s00.Positions.Count; i++)
				{
					SchwabPrice p0 = s00.Positions[i];
					if (p0.Enabled)
					{
						TimeSpan ts = dtAsOf.Subtract(p0.Expiration);
						double nDays = Math.Abs(ts.TotalDays) + .01;
						double nPrice = QFramework.BBPOptionsPricingModels.GetOptionPriceUsingDays(p0.Symbol, p0.Type, nULPrice, p0.Strike, nDays, nRFR, p0.Volatility, nDiv);
						double nFullPrice = nPrice * p0.Quantity * 100;
						nTotal += nFullPrice;
					}
				}
			}
			return nTotal;
		}

        public static double GetStartValueOfSpreads(List<SchwabSpread> s0, double nULPrice, DateTime dtAsOf)
        {
            double nTotal = 0;
            for (int y = 0; y < s0.Count; y++)
            {
                SchwabSpread s00 = s0[y];
                for (int i = 0; i < s00.Positions.Count; i++)
                {
                    SchwabPrice p0 = s00.Positions[i];
                    if (p0.Enabled)
                    {
                        TimeSpan ts = dtAsOf.Subtract(p0.Expiration);
                        double nDays = Math.Abs(ts.TotalDays) + .01;
						double nPrice = p0.MidPrice;
                        double nFullPrice = nPrice * p0.Quantity * 100;
                        nTotal += nFullPrice;
                    }
                }
            }
            return nTotal;
        }


        // mock up a TSLA box
        public static SchwabSpread GetBox(int nQty, double nHiStrike, double nLoStrike, string sSymbol, double nUL, DateTime dtExpiry)
		{
			SchwabSpread s = new SchwabSpread();
			SchwabPrice p1 = new SchwabPrice();
			p1.Type = "P";
			p1.Strike = nHiStrike;
			p1.Quantity = -1 * nQty;
			p1.MidPrice = 30;
			p1.Symbol = sSymbol;
			p1.Expiration = dtExpiry;
			p1.UnderlyingPrice = nUL;
			s.Positions.Add(p1);
			SchwabPrice p2 = new SchwabPrice();
			p2.Type = "P";
			p2.Strike = nLoStrike;
			p2.Quantity = 1 * nQty;
			p2.MidPrice = 30;
			p2.Symbol = sSymbol;
			p2.Expiration = dtExpiry;
			p2.UnderlyingPrice = nUL;
			s.Positions.Add(p2);
			SchwabPrice p3 = new SchwabPrice();
			p3.Type = "C";
			p3.Strike = nLoStrike;
			p3.Quantity = -1 * nQty;
			p3.MidPrice = 30;
			p3.Symbol = sSymbol;
			p3.Expiration = dtExpiry;
			p3.UnderlyingPrice = nUL;
			s.Positions.Add(p3);
			SchwabPrice p4 = new SchwabPrice();
			p4.Type = "C";
			p4.Strike = nHiStrike;
			p4.Quantity = 1 * nQty;
			p4.MidPrice = 30;
			p4.Symbol = sSymbol;
			p4.Expiration = dtExpiry;
			p4.UnderlyingPrice = nUL;
			s.Positions.Add(p4);
			return s;
		}
	}
}
