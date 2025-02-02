using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static BiblePay.BMS.DSQL.DOMItem;
using static BiblePay.BMS.DSQL.SessionExtensions;
using static BiblePay.BMS.DSQL.SessionHelper;
using static BMSCommon.Common;

namespace BiblePay.BMS.Controllers
{

    // ToDo:  When T/F questions appear we need to make options C&D invisible.

    public static class BibleRef
    {
		public static Bible _bible = new Bible();
    }
	public class Bible
	{
		private NBitcoin.Crypto.BibleHash _kjv = new NBitcoin.Crypto.BibleHash();

		public string GetVerse(string sBook, int iChapter, int iVerse)
		{
			string sBook1 = _kjv.GetBookByName(sBook);
			int iStart = 0;
			int iEnd = 0;
			_kjv.GetBookStartEnd(sBook, ref iStart, ref iEnd);
			string h = _kjv.GetVerse(sBook, iChapter, iVerse, iStart, iEnd);
			return h.Trim();
		}
		public List<string> GetBookList()
		{
			return _kjv.GetBookList();
		}
		public string GetBookByName(string sBook)
		{
			return _kjv.GetBookByName(sBook);
		}
	}

	public class UnivFinalExamController : Controller
	{


		[HttpPost]
		public JsonResult ProcessDoCallback([FromBody] ClientToServer o)
		{
			ServerToClient returnVal = new ServerToClient();
			User u0 = GetUser(HttpContext);

			if (o.Action == "Univ_Next")
			{
				string test = Request.Query["test"];
				UnivStoreAnswer(HttpContext, o.FormData);
				UnivFinalExamController.btnNext_Click(HttpContext);
				string m = "location.href='univfinalexam/univfinalexam';";
				returnVal.returnbody = m;
				returnVal.returntype = "javascript";
				string o1 = JsonConvert.SerializeObject(returnVal);
				return Json(o1);
			}
			else if (o.Action == "Univ_Grade")
			{
				string sResults = BiblePay.BMS.Controllers.UnivFinalExamController.ShowResults(HttpContext);
				string modal = DSQL.UI.GetModalDialog("Final Exam Results", sResults);
				returnVal.returnbody = modal;
				returnVal.returntype = "modal";
				string outdata = JsonConvert.SerializeObject(returnVal);
				return Json(outdata);
			}
			else if (o.Action == "Univ_Back")
			{
				string test = Request.Query["test"];
				UnivStoreAnswer(HttpContext, o.FormData);
				UnivFinalExamController.btnBack_Click(HttpContext);
				string m = "location.href='univfinalexam/univfinalexam';";
				returnVal.returnbody = m;
				returnVal.returntype = "javascript";
				string o1 = JsonConvert.SerializeObject(returnVal);
				return Json(o1);
			}
			else if (o.Action == "Univ_Switch")
			{
				string test = Request.Query["test"];
				UnivFinalExamController.btnSwitch_Click(HttpContext);
				string m = "location.href='univfinalexam/univfinalexam';";
				returnVal.returnbody = m;
				returnVal.returntype = "javascript";
				string o1 = JsonConvert.SerializeObject(returnVal);
				return Json(o1);
			}
			else
			{
				throw new Exception("Unknown.");
			}
		}


		protected ExamMemory _exammemory = new ExamMemory();

		private static string Coalesce(string a, string b, string c, string d)
		{
			if (a != "")
				return a;
			if (b != "")
				return b;
			if (c != "")
				return c;
			if (d != "")
				return d;
			return "";
		}
		public static void UnivStoreAnswer(HttpContext h, string sFormData)
		{
			string sRadioA = GetFormData(sFormData, "radioAnswerA");
			string sRadioB = GetFormData(sFormData, "radioAnswerB");
			string sRadioC = GetFormData(sFormData, "radioAnswerC");
			string sRadioD = GetFormData(sFormData, "radioAnswerD");
			string sChosen = Coalesce(sRadioA, sRadioB, sRadioC, sRadioD);
			UnivFinalExamController.RecordAnswer(sChosen, h);
		}


		public IActionResult UnivFinalExam()
		{
			this.Page_Load(HttpContext);
			return View();
		}

		public enum ExamMode
		{
			TESTING = 0,
			TRAIN = 1
		}

		public struct ExamMemory
		{
			public int nCurrentQuestion;
			public List<string> vecQ;
			public List<string> vecA;
			public Dictionary<int, string> mapChosen;
			public string[] vecAnswerKey;
			public bool fTesting;
			public ExamMode ExamMode;
			public int nTrainingQuestionsTaken;
			public int nTestingQuestionsTaken;
			public double nTrainingScore;
			public double nTestingScore;
			public string Course;
			public int StartTime;
		};


			protected void Timer1_Tick()
			{
				int nElapsed = UnixTimestamp() - _exammemory.StartTime;
				int nMins = 0;
				int nSeconds = nElapsed;
				if (nElapsed > 60)
				{
					nMins = nElapsed / 60;
					nSeconds = nElapsed - (nMins * 60);
				}

				ViewBag.lblElapsed = string.Format("{0:D2}", nMins) + ":" + String.Format("{0:D2}", nSeconds);
			}
		public string GetBaseUrl()
		{
			var request = HttpContext.Request;
			var host = request.Host.ToUriComponent();
			var pathBase = request.PathBase.ToUriComponent();
			return $"{request.Scheme}://{host}{pathBase}";
		}
		protected void InitializeTest()
		{ 
    			string sFinalExam = Request.Query["test"];

				string sURL = GetBaseUrl() + "/wwwroot/Univ/" + sFinalExam + "_key.xml";
				string sExam = ExecuteMVCCommand(sURL);
				string sAnswerKey = ExtractXML(sExam, "<KEY>", "</KEY>");
				if (sAnswerKey == "")
				{
    				ViewBag.Error = "Answer key missing";
	     			return;
				}

				_exammemory.vecAnswerKey = sAnswerKey.Split(",");
				string sQ = ExtractXML(sExam, "<QUESTIONS>", "</QUESTIONS>").ToString();
				_exammemory.Course = "Final Exam: " + ExtractXML(sExam, "<COURSE>", "</COURSE>");
				_exammemory.vecQ.Clear();
				_exammemory.vecA.Clear();

				string[] vQ = sQ.Split("<QUESTIONRECORD>");
				for (int i = 0; i < (int)vQ.Length; i++)
				{
					string sQ1 = ExtractXML(vQ[i], "<Q>", "</Q>");
					string sA1 = ExtractXML(vQ[i], "<A>", "</A>");
					if (sQ1 != "" && sA1 !="")
					{
						_exammemory.vecQ.Add(sQ1);
						_exammemory.vecA.Add(sA1);
					}
				}
				_exammemory.fTesting = true;
     			_exammemory.ExamMode = ExamMode.TESTING;
				_exammemory.StartTime = UnixTimestamp();
     			HttpContext.Session.SetObject("exammemory", _exammemory);

			}
			protected void Page_Load(HttpContext h)
			{


			if (HttpContext.Session.ObjectExists("exammemory"))
			{
				_exammemory = HttpContext.Session.GetObject<ExamMemory>("exammemory");
			}
			string sFinalExam = Request.Query["test"];
			if (sFinalExam != null && sFinalExam.Length > 1)
			{
				// clear
				_exammemory.Course = null;
			}
			

			if (_exammemory.Course == null)
			{ 
				_exammemory.ExamMode = ExamMode.TRAIN;
				_exammemory.mapChosen = new Dictionary<int, string>();
				_exammemory.vecA = new List<string>();
				_exammemory.vecQ = new List<string>();
				_exammemory.mapChosen.Clear();
				_exammemory.nCurrentQuestion = 0;
				HttpContext.Session.SetObject("exammemory", _exammemory);
				InitializeTest();
			}
			    ViewBag.lblTitle = _exammemory.Course;
				// Load initial values
				string sMode = _exammemory.ExamMode == ExamMode.TRAIN ? "<font color=red>LEARNING MODE</font>" : "<font color=red>TESTING MODE</font>";
				string sInfo = sMode + "<br><br>Welcome to your Final Exam, " + HttpContext.GetCurrentUser().NickName + "!";
				ViewBag.lblInfo = sInfo;
     			PopulateQuestion();
	      		AfterPopulatingQuestion(this.HttpContext);
		}

		string ExtractAnswer(string sLetter)
		{
				string[] vAnswers = _exammemory.vecA[_exammemory.nCurrentQuestion].Split("|");

				if (sLetter == "A")
				{
					return vAnswers[0];
				}
				else if (sLetter == "B")
				{
					return vAnswers[1];
				}
				else if (sLetter == "C")
				{
					return vAnswers[2];
				}
				else if (sLetter == "D")
				{
					return vAnswers[3];
				}
				return "N/A";
			}

			void StripNumber()
			{
				string sSource = _exammemory.vecQ[_exammemory.nCurrentQuestion];
				int pos = sSource.IndexOf(".");
				string sPrefix = sSource.Substring(0, pos + 1);
				string sMyQ = sSource.Replace(sPrefix, "");
				sMyQ = sMyQ.Trim();
				int nQN = (int)GetDouble(sPrefix + "0");
			    ViewBag.txtQuestionNo = nQN.ToString();
			    ViewBag.txtQuestion = sMyQ;
			}

			string GetPopUpVerses(string sRange)
			{
				string[] vR = sRange.Split(" ");
				if (vR.Length < 2)
					return "";

				string sBook = vR[0];
				string sChapterRange = vR[1];
				string[] vChap = sChapterRange.Split(":");
				if (vChap.Length < 2)
					return "";

				double nChapter = GetDouble(vChap[0]);
				if (nChapter < 1)
					return "";
				string[] vChapRange = vChap[1].Split("-");

				if (vChapRange.Length < 2)
				{
					vChap[1] = vChap[1] + "-" + vChap[1];
					vChapRange = vChap[1].Split("-");
				}
				int nVerseStart = (int)GetDouble(vChapRange[0]);
				double nVerseEnd = GetDouble(vChapRange[1]);
				if (nVerseStart < 1 || nVerseEnd < 1)
					return "";

				if (sBook == "I Corinthians")
					sBook = "1 Corinthians"; // Harvest Time format->KJV format
				if (sBook == "I John")
					sBook = "1 John";
				if (sBook == "Corinthians")
					sBook = "1 Corinthians";

				string sShortBook = BibleRef._bible.GetBookByName(sBook);

				string sTotalVerses = sRange + "\r\n";

				for (int j = nVerseStart; j <= nVerseEnd; j++)
				{
					string sVerse = BibleRef._bible.GetVerse(sShortBook, (int)nChapter, j);
					sTotalVerses += sVerse + "\r\n";
				}
				return sTotalVerses;
			}


			string ScanAnswerForPopUpVerses(string sRefText)
			{
				string[] vSourceScripture = sRefText.Split(" ");
				string sExpandedAnswer = "";
				for (int i = 0; i < (int)vSourceScripture.Length - 1; i++)
				{
					string sScrip = vSourceScripture[i] + " " + vSourceScripture[i + 1];
					string sExpandedVerses = GetPopUpVerses(sScrip);
					if (sExpandedVerses != "")
					{
						sExpandedAnswer += sExpandedVerses + "\r\n\r\n";
					}
				}
				return sExpandedAnswer;
			}

			void PopulateQuestion()
			{
				if (_exammemory.nCurrentQuestion > _exammemory.vecA.Count)
				{
  		    		ViewBag.Error = "FinalExam::Error Size too small";
	   		    	return;
				}

				string[] vAnswers = _exammemory.vecA[_exammemory.nCurrentQuestion].Split("|");
				if (vAnswers.Length > 3)
				{
					ViewBag.lblA = vAnswers[0];
				    ViewBag.lblB = vAnswers[1];
			    	ViewBag.lblC = vAnswers[2];
				    ViewBag.lblD = vAnswers[3];
					//radioAnswerC.Visible = true;
					//radioAnswerD.Visible = true;
				}
				else if (vAnswers.Length > 1)
				{
					ViewBag.lblA = vAnswers[0];
					ViewBag.lblB = vAnswers[1];
				    ViewBag.lblC = "";
					ViewBag.lblD = "";
					//radioAnswerC.Visible = false;
					//radioAnswerD.Visible = false;
				}

			ViewBag.radioAnswerA = "";
			ViewBag.radioAnswerB = "";
			ViewBag.radioAnswerC = "";
			ViewBag.radioAnswerD = "";

				string sChosen = "";
				_exammemory.mapChosen.TryGetValue(_exammemory.nCurrentQuestion, out sChosen);

				if (sChosen == "A")
				{
					ViewBag.radioAnswerA = "CHECKED";
				}
				else if (sChosen == "B")
				{
					ViewBag.radioAnswerB = "CHECKED";
				}
				else if (sChosen == "C")
				{
					ViewBag.radioAnswerC = "CHECKED";
				}
				else if (sChosen == "D")
				{
					ViewBag.radioAnswerD = "CHECKED";
				}

				if (sChosen == "")
				{
					//ResetRadios();
				}

				StripNumber();

				if (_exammemory.ExamMode == ExamMode.TESTING)
				{
					// TEST mode
					ViewBag.txtAnswer = "";
				}
				else
				{
					// In Training Mode, if we have any bible verses in the answer, lets include the actual scripture to help the student:
					string sExpandedAnswer = ExtractAnswer(_exammemory.vecAnswerKey[_exammemory.nCurrentQuestion]);
					string sRefText = sExpandedAnswer + " " + _exammemory.vecQ[_exammemory.nCurrentQuestion];
					string sBiblicalRefs = ScanAnswerForPopUpVerses(sRefText);
					sExpandedAnswer += "\r\n\r\n" + sBiblicalRefs;
					ViewBag.txtAnswer = sExpandedAnswer;
				}
				string sCaption = _exammemory.ExamMode == ExamMode.TRAIN ? "Switch to TEST Mode" : "Switch to REVIEW Mode";
				ViewBag.btnSwitch = sCaption;
			}

			private static double CalculateScores(HttpContext h)
			{
				ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
				double nTotalCorrect = 0;
				double nTaken = 0;
				for (int i = 0; i < e.vecAnswerKey.Length; i++)
				{
					string sChosen = "";
					e.mapChosen.TryGetValue(i, out sChosen);
					double nCorrect = e.vecAnswerKey[i] == sChosen ? 1 : 0;
					nTotalCorrect += nCorrect;
					if (sChosen != "")
					{
						nTaken++;
					}
				}
				e.nTestingQuestionsTaken = (int)nTaken;
				double nScore = nTotalCorrect / (e.vecAnswerKey.Length + .001);
     			h.Session.SetObject("exammemory", e);
				return nScore;
			}

		public static void ClearTestResults(HttpContext h)
        {
			ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
			e.nTrainingQuestionsTaken = 0;
			e.nTestingQuestionsTaken = 0;
			e.nTrainingScore = 0;
			e.nTestingScore = 0;
			h.Session.SetObject("exammemory", e);
		}
		public static string ShowResults(HttpContext h)
		{

				ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
				
				e.fTesting = false;
     			double nTestingPct = CalculateScores(h);
				string sSummary = "Congratulations!";
				sSummary += "<br>You worked through " + e.nTestingQuestionsTaken.ToString()
					+ ", and your score is " + Math.Round(nTestingPct * 100, 2).ToString() + "%! ";
				sSummary += "<br>Please come back and see us again. ";
				// Clear the results so they can start again if they want:
				ClearTestResults(h);
		      	return sSummary;
		}

		
			public static void RecordAnswer( string sChosen, HttpContext h)
			{
				// should be a,b,c,d or blank
				ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
				e.mapChosen[e.nCurrentQuestion] = sChosen;
    			h.Session.SetObject("exammemory", e);

			double nTestPct = Grade(h);
			if (e.ExamMode == ExamMode.TESTING)
			{
				e.nTrainingQuestionsTaken++;
				e.nTrainingScore += nTestPct;
			}
			// Score the current Testing session
			if (e.ExamMode == ExamMode.TRAIN)
			{
				e.nTestingQuestionsTaken++;
				e.nTestingScore += nTestPct;
			}
			h.Session.SetObject("exammemory", e);


		}

		public static double Grade(HttpContext h)
		{
				string sChosen = "";
     			ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
				e.mapChosen.TryGetValue(e.nCurrentQuestion, out sChosen);
				double nCorrect = (e.vecAnswerKey[e.nCurrentQuestion] == sChosen) ? 1 : 0;
				return nCorrect;
		}

		protected void AfterPopulatingQuestion(HttpContext h)
		{
				ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
				double nCorrect = Grade(h);
				if (e.ExamMode == ExamMode.TESTING)
				{
					ViewBag.lblGrade = "";
				}
				else
				{
					string sChosen = "";
					bool fGot = e.mapChosen.TryGetValue(e.nCurrentQuestion, out sChosen);
					if (fGot)
					{
						string sCorrNarr = nCorrect == 1 ? "<font color=red>Correct</font>" : "<font color=red>Incorrect</font>";
						ViewBag.lblGrade = sCorrNarr;
					}
					else
					{
						ViewBag.lblGrade = "";
					}
				}
			ViewBag.TestingMode = e.ExamMode == ExamMode.TESTING ? "Test Mode" : "Review Mode";
		}


		public static void btnNext_Click(HttpContext h)
		{
			ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
			e.nCurrentQuestion++;
     		h.Session.SetObject("exammemory", e);
	     	if (e.nCurrentQuestion > e.vecQ.Count - 1)
			{
					e.nCurrentQuestion = e.vecQ.Count - 1;
					h.Session.SetObject("exammemory", e);
			}
		}

		public static void btnBack_Click(HttpContext h)
		{
			ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
			e.nCurrentQuestion--;
			h.Session.SetObject("exammemory", e);
    		if (e.nCurrentQuestion < 0)
			{
				e.nCurrentQuestion = 0;
				h.Session.SetObject("exammemory", e);
	    	}
		}

		public static void btnSwitch_Click(HttpContext h)
		{
			ExamMemory e = h.Session.GetObject<ExamMemory>("exammemory");
			if (!e.fTesting)
			{
				e.fTesting = true;
			}
			if (e.ExamMode == ExamMode.TESTING)
			{
				e.ExamMode = ExamMode.TRAIN;
			}
			else
			{
				e.ExamMode = ExamMode.TESTING;
			}
			h.Session.SetObject("exammemory", e);
		}
	}
}
