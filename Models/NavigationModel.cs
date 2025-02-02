using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BiblePay.BMS.Extensions;
using BMSCommon;
using Microsoft.AspNetCore.Http;

namespace BiblePay.BMS.Models
{
    public static class NavigationModel
    {
        private const string Underscore = "_";
        private const string Dash = "-";
        private const string Space = " ";
        private static readonly string Empty = string.Empty;
        public static readonly string Void = "javascript:void(0);";

        public static SmartNavigation BuildNavigation(string sERC20, bool seedOnly = true)
        {
            var jsonText = File.ReadAllText("nav.json");
            var navigation = NavigationBuilder.FromJson(jsonText);
            var menu = FillProperties(sERC20, navigation.Lists, seedOnly);
            return new SmartNavigation(menu);
        }

        private static List<ListItem> FillProperties(string sERC20, IEnumerable<ListItem> items, bool seedOnly, ListItem parent = null)
        {
            var result = new List<ListItem>();
            bool fSanc = BBPAPI.Service.IsSanctuary();
            bool fIsWebNode = BBPAPI.Service.IsPrimary();
            //Common.Log("iswebnode=" + fIsWebNode.ToString());

            foreach (var item in items)
            {
                item.Text ??= item.Title;
                item.Tags = string.Concat(parent?.Tags, Space, item.Title.ToLower()).Trim();

                var parentRoute = (Path.GetFileNameWithoutExtension(parent?.Text ?? Empty)?.Replace(Space, Underscore) ?? Empty).ToLower();
                var sanitizedHref = parent == null ? item.Href?.Replace(Dash, Empty) : item.Href?.Replace(parentRoute, parentRoute.Replace(Underscore, Empty)).Replace(Dash, Empty);
                var route = Path.GetFileNameWithoutExtension(sanitizedHref ?? Empty)?.Split(Underscore) ?? Array.Empty<string>();
                bool fDisabled = false;
                item.Route = route.Length > 1 ? $"/{route.First()}/{string.Join(Empty, route.Skip(1))}" : item.Href;
                if (false)
                {
                    /*
                    if (item.Route != null)
                    {
                        if (item.Route.ToLower().Contains("nft") || item.Route == "/proposal/proposaladd" 
                            || item.Route == "/proposal/proposallist" || item.Route == "/turnkeysanc/turnkeysanctuaries" 
                            || item.Route == "/portfoliobuilder/portfoliobuilder" || item.Route == "/portfoliobuilder/portfoliobuilderleaderboard")
                        {
                            fDisabled = true;
                        }
                    }
                    */
                }

                // Quant Menus
                bool fQuant = sERC20 == "0xafe8c2709541e72f245e0da0035f52de5bdf3ee5" || sERC20== "BFjZ9eMmjCNZCBrtvBZSYqxvwhSPwxLBCT" || sERC20 == "0xb847ef0e5a428e2f81b7ac52524bda9ee416870c";
                if (!fQuant)
                {
                    if (item.Route != null)
                    {
                        if (item.Route.ToLower().Contains("quant"))
                        {
                            fDisabled = true;
                        }
                    }
                }

                if (!fSanc)
                {
                    if (item.Route != null)
                    {

                        if (item.Roles.Contains("sanc"))
                        {
                            fDisabled = true;
                        }
                    }
                }

                if (fIsWebNode)
                {
                    if (item.Route != null)
                    {

                        if (item.Roles.Contains("HomeUser"))
                        {
                            fDisabled = true;
                        }
                    }
                }

                item.I18n = parent == null
                    ? $"nav.{item.Title.ToLower().Replace(Space, Underscore)}"
                    : $"{parent.I18n}_{item.Title.ToLower().Replace(Space, Underscore)}";
                item.Type = parent == null ? item.Href == null ? ItemType.Category : ItemType.Single : item.Items.Any() ? ItemType.Parent : ItemType.Child;
                item.Items = FillProperties(sERC20, item.Items, seedOnly, item);

                if (item.Href.IsVoid() && item.Items.Any())
                    item.Type = ItemType.Sibling;

                if (!seedOnly || item.ShowOnSeed)
                {
                    if (!fDisabled)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}
