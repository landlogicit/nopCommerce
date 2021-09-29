﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Common;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo450
{
    [NopMigration("2021-04-23 00:00:00", "4.50.0", UpdateMigrationType.Localization, MigrationProcessType.Update)]
    public class LocalizationMigration : MigrationBase
    {
        /// <summary>Collect the UP migration expressions</summary>
        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //do not use DI, because it produces exception on the installation process
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            //use localizationService to add, update and delete localization resources
            localizationService.DeleteLocaleResourcesAsync(new List<string>
            {
                "Admin.Configuration.AppSettings.Hosting.UseHttpClusterHttps",
                "Admin.Configuration.AppSettings.Hosting.UseHttpClusterHttps.Hint",
                "Admin.Configuration.AppSettings.Hosting.UseHttpXForwardedProto",
                "Admin.Configuration.AppSettings.Hosting.UseHttpXForwardedProto.Hint",
                "Admin.Configuration.AppSettings.Hosting.ForwardedHttpHeader",
                "Admin.Configuration.AppSettings.Hosting.ForwardedHttpHeader.Hint"
            }).Wait();

            var languageService = EngineContext.Current.Resolve<ILanguageService>();
            var languages = languageService.GetAllLanguagesAsync(true).Result;
            var languageId = languages
                .Where(lang => lang.UniqueSeoCode == new CultureInfo(NopCommonDefaults.DefaultLanguageCulture).TwoLetterISOLanguageName)
                .Select(lang => lang.Id).FirstOrDefault();

            localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                //#5696
                ["Admin.ContentManagement.MessageTemplates.List.SearchKeywords"] = "Search keywords",
                ["Admin.ContentManagement.MessageTemplates.List.SearchKeywords.Hint"] = "Search message template(s) by specific keywords.",

				//New configurations to forward proxied headers
                ["Admin.Configuration.AppSettings.Hosting.ForwardedForHeaderName"] = "The header used to retrieve the originating client IP",
                ["Admin.Configuration.AppSettings.Hosting.ForwardedForHeaderName.Hint"] = "Specify a custom HTTP header name to determine the originating IP address (e.g., CF-Connecting-IP, X-ProxyUser-Ip).",
                ["Admin.Configuration.AppSettings.Hosting.ForwardedProtoHeaderName"] = "The header used to retrieve the value for the originating scheme",
                ["Admin.Configuration.AppSettings.Hosting.ForwardedProtoHeaderName.Hint"] = "Specify a custom HTTP header name for identifying the protocol (HTTP or HTTPS) that a client used to connect to your proxy or load balancer.",
                ["Admin.Configuration.AppSettings.Hosting.UseProxy"] = "Use proxy servers",
                ["Admin.Configuration.AppSettings.Hosting.UseProxy.Hint"] = "Enable this setting to apply forwarded headers to their matching fields on the current HTTP-request.",
                ["Admin.Configuration.AppSettings.Hosting.KnownProxies"] = "Addresses of known proxies",
                ["Admin.Configuration.AppSettings.Hosting.KnownProxies.Hint"] = "Specify a list of IP addresses (comma separated) to accept forwarded headers.",

                //#5618
                ["Admin.Configuration.Settings.GeneralCommon.EnableCssBundling.Hint"] = "Enable to combine (bundle) multiple CSS files into a single file. Do not enable if you're running nopCommerce in IIS virtual directory. And please note it could take up to two minutes for changes to existing files to be applied (when enabled).",
                ["Admin.Configuration.Settings.GeneralCommon.EnableJsBundling.Hint"] = "Enable to combine (bundle) multiple JavaScript files into a single file. And please note it could take up to two minutes for changes to existing files to be applied (when enabled).",

                //#4880
                ["Admin.Configuration.AppSettings.Common.ServeUnknownFileTypes"] = "Serve unknown types of static files",
                ["Admin.Configuration.AppSettings.Common.ServeUnknownFileTypes.Hint"] = "Enable this setting to serve files that are in the .well-known directory without a recognized content-type",

                //#5532
                ["Admin.Configuration.Languages.CLDR.Warning"] = "Please make sure the appropriate CLDR package is installed for this culture. You can set CLDR for the specified culture on the <a href=\"{0}\" target=\"_blank\">General Settings page, Localization tab</a>.",

                //#4325
                ["Admin.Common.Alert.NothingSelected"] = "Please select at least one record.",

                //#5316
                ["Account.Login.AlreadyLogin"] = "You are already logged in as {0}. You may log in to another account.",

                //#5511
                ["Admin.Configuration.AppSettings.Data"] = "Data configuration",
                ["Admin.Configuration.AppSettings.Data.ConnectionString"] = "Connection string",
                ["Admin.Configuration.AppSettings.Data.ConnectionString.Hint"] = "Sets a connection string.",
                ["Admin.Configuration.AppSettings.Data.DataProvider"] = "Data provider",
                ["Admin.Configuration.AppSettings.Data.DataProvider.Hint"] = "Sets a data provider.",
                ["Admin.Configuration.AppSettings.Data.SQLCommandTimeout"] = "SQL command timeout",
                ["Admin.Configuration.AppSettings.Data.SQLCommandTimeout.Hint"] = "Gets or sets the wait time (in seconds) before terminating the attempt to execute a command and generating an error. By default, timeout isn't set and a default value for the current provider used. Set 0 to use infinite timeout.",
                ["Admin.Configuration.AppSettings.Description"] = "Configuration in ASP.NET Core is performed using a different configuration providers (e.g. external appsettings.json configuration file, environment variables, etc). These settings are used when the application is launched, so after editing them, the application will be restarted. You can find a detailed description of all configuration parameters in <a href=\"{0}\" target=\"_blank\">our documentation</a>. Please note that changing the values here will overwrite the external appsettings.json file, settings from other configuration providers will not be affected.",
                ["Enums.Nop.Data.DataProviderType.Unknown"] = "Unknown",
                ["Enums.Nop.Data.DataProviderType.SqlServer"] = "Microsoft SQL Server",
                ["Enums.Nop.Data.DataProviderType.MySql"] = "MySQL",
                ["Enums.Nop.Data.DataProviderType.PostgreSQL"] = "PostgreSQL",
                
                //#5838
                ["Admin.Configuration.Languages.NeedRestart"] = "Since language cultures are loaded only when the application is starting, you have to restart the application for it to work correctly once the language is changed.",

                //#5155
                ["Admin.System.Warnings.PluginNotInstalled"] = "You haven't installed the following plugin(s)",
                ["Admin.System.Warnings.PluginNotInstalled.HelpText"] = "You may delete the plugins you don't use in order to increase startup time"
            }, languageId).Wait();

            // rename locales
            var localesToRename = new[]
            {
                new { Name = "", NewName = "" }
            };

            foreach (var lang in languages)
            {
                foreach (var locale in localesToRename)
                {
                    var lsr = localizationService.GetLocaleStringResourceByNameAsync(locale.Name, lang.Id, false).Result;
                    if (lsr != null)
                    {
                        lsr.ResourceName = locale.NewName;
                        localizationService.UpdateLocaleStringResourceAsync(lsr).Wait();
                    }
                }
            }
        }

        /// <summary>Collects the DOWN migration expressions</summary>
        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
