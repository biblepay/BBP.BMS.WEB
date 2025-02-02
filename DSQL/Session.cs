using BiblePay.BMS.Extensions;
using BMSCommon.Model;
using Microsoft.AspNetCore.Http;
using System;
using static BMSCommon.Common;

namespace BiblePay.BMS.DSQL
{
	public static class SessionHelper
    {
        public static double GetSessionDouble(HttpContext h, string sKey)
        {
            string sChain = GetChain(h);
            double n = GetDouble(h.Session.GetString(sChain + "_" + sKey));
            return n;
        }

        public static void SetSessionDouble(HttpContext h, string sKey, double nValue)
        {
            string sChain = GetChain(h);
            h.Session.SetString(sChain + "_" + sKey, nValue.ToString());
        }
        public static string GetChain(HttpContext s)
        {
            string s1 = s.Session.GetString("Chain");
            if (s1 == null)
            {
                s.Session.SetString("Chain", "MAINNET");
                s1 = s.Session.GetString("Chain");
            }
            return s1;
        }

        public static bool IsTestNet(HttpContext s)
        {
            string sChain = GetChain(s);
            return sChain == "MAINNET" ? false : true;
        }
        public static string GetChainColor(HttpContext s1)
        {
            string sChain = GetChain(s1);
            if (sChain == "TESTNET")
                return "background-color:LIME;";
            return "";
        }


		// When we move away from ERC-20 we can continue to optimize this function:
		public static User GetUser(HttpContext s)
        {
            
            string sKey = IsTestNet(s) ? "tCUR_USER" : "CUR_USER";
            User u = s.Session.GetObject<User>(sKey) ?? new User();

            // If a session object exists; use it instead
            if (u.LoggedIn)
            {
                return u;
            }
			else if (!u.LoggedIn && !String.IsNullOrEmpty(u.BBPAddress))
            {
                u = BBPAPI.Model.UserFunctions.GetCachedUser(IsTestNet(s), u.BBPAddress);
                if (u==null)
                {
                    return new User();
                }
                u.LoggedIn = true;
				s.Session.SetObject(sKey, u);
				return u;
			}
            else
            {
			}
            return u;
		}
       
        public static User GetUserMFA(HttpContext s, string sNickName, string sMFACode)
        {
            string sKey = IsTestNet(s) ? "tCUR_USER" : "CUR_USER";
            User u = s.Session.GetObject<User>(sKey);
            u ??= new User();

            if (u.LoggedIn)
            {
                return u;
            }
            if (!u.LoggedIn)
            {
                u = BBPAPI.Model.UserFunctions.GetUserByNickName(IsTestNet(s), sNickName);
                var isValid = BBPAPI.ERCUtilities.ValidateMFA(u, sMFACode);
                if (false)
                {
                    u.LoggedIn = true;
                    s.Session.SetObject(sKey, u);
                }
                else
                {
                    u.LoggedIn = false;
                }
            }
            return u;
        }

        public static void SetUser(User u, HttpContext s)
        {
            string sTable = IsTestNet(s) ? "tCUR_USER" : "CUR_USER";
            s.Session.SetObject(sTable, u);
        }

        public static string GetLogInAction(HttpContext s)
        {
            string s1 = GetLogInStatus(s);
            string sLogOut = "var c = confirm('Are you sure you want to Log Out?'); if (c) { DoCallback('profile_logout','','profile/processdocallback'); }";
            string sLoc = s1 == "LOGGED OUT" ? "location.href='/profile/profile';" : sLogOut;
            return sLoc;
        }

        public static string GetNickName(HttpContext s)
        {
            string s1 = GetLogInStatus(s);
            if (s1 == "LOGGED IN")
            {
                User u = s.GetCurrentUser();
                return u.NickName;
            }
            else
            {
                return "Guest";
            }
        }
        public static string GetLogInStatus(HttpContext s)
        {
            User u = GetUser(s);
            string status = String.Empty;
            
            if (u == null || String.IsNullOrEmpty(u.BBPAddress))
            {
                status = "LOGGED OUT";
            }
            else
            {
                status = "LOGGED IN";
            }
            return status;
        }
       
    }
}
