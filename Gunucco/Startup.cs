using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Gunucco.Models.Database;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Targets;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MailKit.Security;
using Gunucco.Models.Services;
using System.Text;

namespace Gunucco
{
    public class Startup
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public Startup(IHostingEnvironment env)
        {
            this.InitializeLogger();

            log.Info("==== StartUp Start ====");

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            log.Info("==== StartUp End ====");
        }

        private void InitializeLogger()
        {
#if !RELEASE || UNITTEST
            var logLevel = NLog.LogLevel.Trace;
#else
            var logLevel = NLog.LogLevel.Info;
#endif

            var logconfig = new NLog.Config.LoggingConfiguration();
            logconfig.AddTarget("logfile", new FileTarget
            {
                FileName = "./logs/gunucco.log",
                DeleteOldFileOnStartup = true,
                Layout = "${longdate} [${threadid:padding=8}] [${uppercase:${level:padding=-5}}] ${callsite}() ${message} ${exception:format=tostring}",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = "./logs/gunucco.{#}.log",
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                ArchiveDateFormat = "yyyy-MM-dd",
                ArchiveAboveSize = 1024 * 1024 * 10,        // 10MB
                MaxArchiveFiles = 14,
            });
            logconfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, "logfile");
            LogManager.Configuration = logconfig;
            log = LogManager.GetCurrentClassLogger();

            log.Info("==== Start logging ====");
            log.Info("");
            log.Info("    ***************************");
            log.Info("    *                         *");
            log.Info("    *   Welcome to Gunucco!   *");
            log.Info("    *                         *");
            log.Info("    ***************************");
            log.Info("");
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            log.Info("==== ConfigureServices Start ====");

            // Connect database
            MainContext.ConnectionString = this.Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<MainContext>(options => options.UseMySql(this.Configuration.GetConnectionString("DefaultConnection")));

            // automatic migration on start up
            log.Info("==== Database Migration Start ====");
            using (var db = new MainContext())
            {
                db.Database.Migrate();
            }
            log.Info("==== Database Migration End ====");

            // get application configs
            {
                var config = this.Configuration.GetSection("GunuccoConfigs");
                Config.ServerVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                Config.ServerLanguage = config.GetValue<string>("ServerLanguage", "en");
                Config.AdministratorName = config.GetValue<string>("AdministratorName", "Do not look at me");
                Config.AdministratorUri = config.GetValue<string>("AdministratorUri", "https://www.google.com/");
                Config.ServerPath = config.GetValue<string>("ServerPath", "http://localhost");
                Config.MediaDirPath = config.GetValue<string>("MediaDirPath", "./media/");
                Config.IsDebugMode = config.GetValue<bool>("IsDebugMode", false);
                Config.IsAllowOutsideMedias = config.GetValue<bool>("IsAllowOutsideMedias", false);
                Config.IsAllowHtmlContent = config.GetValue<bool>("IsAllowHtmlContent", false);
                Config.IsAllowNewSignUp = config.GetValue<bool>("IsAllowNewSignUp", true);

                Config.IsEmailValidationNeed = config.GetValue<bool>("IsEmailValidationNeed", true);
                Config.SmtpServer = config.GetValue<string>("SmtpServer", "");
                Config.SmtpPort = config.GetValue<int>("SmtpPort", 25);
                {
                    var smtpSecureOptions = config.GetValue<string>("SmtpSecureSocketOptions", "None");
                    SecureSocketOptions opt = SecureSocketOptions.None;
                    Enum.TryParse(smtpSecureOptions, out opt);
                    Config.SmtpSecureSocketOptions = opt;
                }
                Config.SmtpAccountId = config.GetValue<string>("SmtpAccountId", "");
                Config.SmtpPassword = config.GetValue<string>("SmtpPassword", "");
                Config.MailFrom = config.GetValue<string>("MailFrom", "");
                Config.MailFromName = config.GetValue<string>("MailFromName", "Gunucco System");
            }

            // enable cookie
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            // enable session
            services.AddMemoryCache();
            services.AddSession(opt =>
            {
                opt.IdleTimeout = new TimeSpan(22, 0, 0);
            });

            // Add framework services.
            services.AddMvc();

            log.Info("==== ConfigureServices End ====");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            log.Info("==== Configure Start ====");

            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // enable sjis for sending ping
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // start gunucco services
            DBCleanerService.StartServices();
            PingService.StartService();

            // start global streaming
            StreamingService.GlobalTimeline.StartListening();

            log.Info("==== Configure End ====");
        }
    }
}
