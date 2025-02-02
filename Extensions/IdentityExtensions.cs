using BBPAPI.Model;
using BiblePay.BMS.Models;
using BMSCommon.Model;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using static BiblePay.BMS.DSQL.SessionHelper;

namespace BiblePay.BMS.Extensions
{
    public static class IdentityExtensions
    {
        [DebuggerStepThrough]
        private static bool HasRole(this ClaimsPrincipal principal, params string[] roles)
        {
            if (principal == null)
                return default;

            var claims = principal.FindAll(ClaimTypes.Role).Select(x => x.Value).ToSafeList();

            return claims?.Any() == true && claims.Intersect(roles ?? new string[] { }).Any();
        }

        public static User GetCurrentUser(this HttpContext h)
        {
            User u0 = GetUser(h);
            return u0;
        }

        public static void EraseUserCache(this HttpContext h)
        {
            string sKey = IsTestNet(h) ? "tUser" : "User";
           
            h.Session.Set(sKey, new byte[0]);

            UserFunctions.EraseUserCache();
        }
        [DebuggerStepThrough]
        public static IEnumerable<ListItem> AuthorizeFor(this IEnumerable<ListItem> source, ClaimsPrincipal identity)
            => source.Where(x => x.Roles.IsNullOrEmpty() || (x.Roles.HasItems() && identity.HasRole(x.Roles))).ToSafeList();

        [DebuggerStepThrough]
        public static HtmlString AsRaw(this string value) => new HtmlString(value);

        [DebuggerStepThrough]
        public static string ToPage(this string href) => System.IO.Path.GetFileNameWithoutExtension(href)?.ToLower();

        [DebuggerStepThrough]
        public static bool IsVoid(this string href) => href?.ToLower() == NavigationModel.Void;

        [DebuggerStepThrough]
        public static bool IsRelatedTo(this ListItem item, string pageName) => item?.Type == ItemType.Parent && item?.Href?.ToPage() == pageName?.ToLower();

      

    }
}
