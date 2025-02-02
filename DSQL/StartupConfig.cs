using BiblePay.BMS.Models;
using BMSCommon;
using BMSCommon.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using static BiblePay.BMS.BWS;
using static BMSCommon.Common;

namespace BiblePay.BMS
{
    public static class UseMiddlewareExtensions
    {
        public static IApplicationBuilder UseHalfordFileProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HalfordFileProvider>();
        }
    }

    public class StartupConfig
    {


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileProvider>(
                 new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            services.AddMvc(options =>
            {
                ServiceProvider serviceProvider = services.BuildServiceProvider();
            });

            services.AddHttpContextAccessor();

            services.AddControllersWithViews();
            services.AddSession();

            services.Configure<SmartSettings>(Configuration.GetSection(SmartSettings.SectionName));
            services.AddSingleton(s => s.GetRequiredService<IOptions<SmartSettings>>().Value);
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given 
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.AddTransient<IEmailSender, EmailSender>();

            services.AddControllersWithViews(options => {
                options.Filters.Add<BiblePay.BMS.BWS.ViewBagActionFilter>();
            });
            services.AddSingleton<IBBPSvc, BBPSvc>();



            services.AddRazorPages();

            services.AddRazorPages().AddRazorRuntimeCompilation();


            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            services.AddMvc(options =>
            {
                ServiceProvider serviceProvider = services.BuildServiceProvider();
            });

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins(new string[] {
                    "https://social.biblepay.org",
                    "https://localhost:8443", "https://*", "http://localhost", "https://localhost" })
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));










            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins(new string[] { "https://dec.app", "https://hitch.social", "https://www.hitch.social",
                    "https://social.biblepay.org", "http://social.biblepay.org", "https://*", "http://localhost", "https://localhost" })
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

        }




        public static IHostBuilder CreateHostBuilder(string[] args, string sBindURL, IEnumerable<ServiceDescriptor> services) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    Common.Log("*** USING BIND URL " + sBindURL);
                    if (!IsWindows() )


                    {
                        webBuilder.UseKestrel(serverOptions =>
                        {
                            // Video sizes
                            serverOptions.Limits.MaxRequestHeadersTotalSize = 500000;
                            serverOptions.Limits.MaxRequestBufferSize = 500000;
                            serverOptions.Limits.RequestHeadersTimeout = new TimeSpan(5000);
                            serverOptions.Limits.MaxRequestBodySize = 50000000;
                            //kestrelOptions.ConfigureHttpsDefaults(httpsOptions.ServerCertificate = FluffySpoon.X509.();
                            /*
                            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                            {
                                httpsOptions.ServerCertificate = FluffySpoon.X509.GetSSL();
                            });
                            */

                        })
                       .ConfigureServices(collection =>
                       {
                           if (services == null)
                           {
                               return;
                           }
                       })

                    .UseIISIntegration().UseKestrel()
                    .UseUrls(sBindURL)
                    .UseStartup<BiblePay.BMS.StartupConfig>();
                    }
                    else
                    {
                        webBuilder.UseIIS().UseIISIntegration().UseUrls(sBindURL)
                        .UseStartup<BiblePay.BMS.StartupConfig>();
                    }

                });


        public IConfigurationRoot Configuration { get; }

        [Obsolete]
        public StartupConfig(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Global.msContentRootPath = env.ContentRootPath;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        object GetFirstCP(dynamic oCollection)
        {
            foreach (object o in oCollection)
            {
                return o;
            }
            return null;
        }
        public static int GetPort(string URL)
        {
            string[] pieces = URL.Split(':');
            if (pieces.Length > 1)
            {
                return (int)GetDouble(pieces[2]);
            }
            return 0;
        }



        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=BMS}/{action=Status}");
                endpoints.MapRazorPages();
            });

            app.UseHalfordFileProvider();
            try
            {
             /*   var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
                string address = GetFirstCP(serverAddressesFeature.Addresses).ToString();
                int iPort = GetPort(address);
             */

                string sWWWRoot = GetFolder("video");
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(sWWWRoot),
                    RequestPath = "/video"
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(GetFolder("sql")),
                    RequestPath = "/sql"
                });

                // End of Biblepay MVC
            }
            catch (Exception ex)
            {
                Log("BWS::" + ex.Message);
            }
        }
    }
}

