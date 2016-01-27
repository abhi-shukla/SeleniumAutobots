using System;
using System.Configuration;

namespace SeleniumDriver
{
    public enum BrowserMake
    {
        Chrome, Firefox, Phantom, InternetExplorer
    }

    public static class EnvironmentConfiguration
    {
        static EnvironmentConfiguration()
        {
            var configuredAddress = ConfigurationManager.AppSettings["WebUiAddress"];

            BrowserMake browser;
            if (!Enum.TryParse(ConfigurationManager.AppSettings["Browser"], true, out browser))
            {
                //Default to chrome
                browser = BrowserMake.Chrome;
            }
            Browser = browser;

            Uri address;
            if (!Uri.TryCreate(configuredAddress, UriKind.Absolute, out address))
            {
                address = new Uri("http://localhost:18223");
            }

            WebUiAddress = address.ToString().TrimEnd('/');
            WebServer = address.Host;
            WebServerPort = address.Port;
            IsWebServerSecure = address.Scheme == Uri.UriSchemeHttps;
        }

        public static BrowserMake Browser { get; private set; }
        public static string WebUiAddress { get; private set; }
        public static string WebServer { get; private set; }

        public static int WebServerPort { get; private set; }
        public static bool IsWebServerSecure { get; private set; }
    }
}
