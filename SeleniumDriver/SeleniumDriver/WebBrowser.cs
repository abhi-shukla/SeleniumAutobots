using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using TechTalk.SpecFlow;

namespace SeleniumDriver
{
    public static class WebBrowser
    {
        private static readonly IWebDriver Browser;

        static WebBrowser()
        {
            AppDomain.CurrentDomain.DomainUnload += CleanUpDriver;
            switch (EnvironmentConfiguration.Browser)
            {
                case BrowserMake.Firefox:
                    Browser = new FirefoxDriver();
                    break;
                case BrowserMake.InternetExplorer:
                    Browser = new InternetExplorerDriver();
                    break;
                case BrowserMake.Phantom:
                    Browser = new PhantomJSDriver();
                    break;
                default:
                    Browser = new ChromeDriver();
                    break;
            }
        }

        public static IWebDriver CurrentBrowser
        {
            get { return Browser; }
        }

        public static void EnsureMinBrowserSize(IWebDriver browser, int x, int y)
        {
            browser.Manage().Window.Position = new Point(0, 0);
            var browserSize = browser.Manage().Window.Size;
            browser.Manage().Window.Size = new Size(Math.Max(browserSize.Width, x), Math.Max(browserSize.Height, y));
        }

        public static void TakeScreenShot(string saveTo)
        {
            var photographer = CurrentBrowser as ITakesScreenshot;
            if (photographer != null)//If screenshots are supported
            {
                photographer.GetScreenshot().SaveAsFile(saveTo, ImageFormat.Png);
            }
        }

        public static void CleanUp()
        {
            foreach (var cookie in CurrentBrowser.Manage().Cookies.AllCookies.ToList())
            {
                CurrentBrowser.Manage().Cookies.DeleteCookie(cookie);
            }
        }

        public static void NavigateTo(string url)
        {
            CurrentBrowser.Navigate().GoToUrl(url);
        }

        static void CleanUpDriver(object sender, EventArgs eventArgs)
        {
            Browser.Quit();
            // Dispose this is not required as Quit method calls the dispose, so don't need to call it explicitly.
            // Calling Browser.Dispose() after Browser.Quit() is causing an error: no such session exception in tests
        }
    }
}
