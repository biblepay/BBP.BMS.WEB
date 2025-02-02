using Microsoft.AspNetCore.Http;

namespace BiblePay.BMS.Models
{
    public class Theme
    {
        public string ThemeVersion { get; set; }
        public string IconPrefix { get; set; }
        public string Logo { get; set; }
        public string User { get; set; }
        public string Role { get; set; } = "Administrator";
        public string Email { get; set; }
        public string Twitter { get; set; }
        public string Avatar { get; set; }
    }

    public class Features
    {
        public bool AppSidebar { get; set; }
        public bool AppHeader { get; set; }
        public bool AppLayoutShortcut { get; set; }
        public bool AppFooter { get; set; }
        public bool ShortcutMenu { get; set; }
        public bool GoogleAnalytics { get; set; }
        public bool ChatInterface { get; set; }
        public bool LayoutSettings { get; set; }
    }

    public class SmartSettings
    {
        public const string SectionName = nameof(SmartSettings);

        public string Version { get; set; }
        public string App { get; set; }
        public string AppName { get; set; }
        public string AppFlavor { get; set; }
        public string AppFlavorSubscript { get; set; }
        public Theme Theme { get; set; }
        public Features Features { get; set; }

        public string ComputeSyncFusionTheme(HttpContext h)
        {
            string sURL = string.Empty;
            //mod-skin-dark
            //mod-skin-????
            string sMyTheme = h.Session.GetString("theme");
            
			if (!sMyTheme.Contains("mod-skin-dark"))
            {
                // light
                sURL = "https://cdn.syncfusion.com/ej2/22.1.34/fluent.css";
            }
            else
            {
                // dark
                sURL = "https://cdn.syncfusion.com/ej2/22.1.34/tailwind-dark.css";
            }
            /*        <!--<link rel="stylesheet" href="https://cdn.syncfusion.com/ej2/22.1.34/material3-dark.css"/>-->
                       <!--<link rel="stylesheet" href="https://cdn.syncfusion.com/ej2/22.1.34/fluent.css" /> -->
                        <link rel="stylesheet" href="https://cdn.syncfusion.com/ej2/22.1.34/tailwind-dark.css" />
            */
            return sURL;

        }
    }

    public class SmartError
    {
        public string[][] Errors { get; set; } = { };

        public static SmartError Failed(params string[] errors) => new SmartError { Errors = new[] { errors } };
    }
}
