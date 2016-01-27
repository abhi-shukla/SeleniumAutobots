using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace SeleniumDriver
{
    public static class SeleniumExtensions
    {

        public static By Where(this By baseFilter, Func<IWebElement, bool> isMatch)
        {
            return new PredicateBy(baseFilter, isMatch);
        }

        public static By Nth(this By baseFilter, int count)
        {
            return new NthBy(baseFilter, count);
        }

        public static void EnsureNoElementFound(this IWebDriver driver, By filter)
        {
            Thread.Sleep(5000);
            var element = driver.FindElements(filter.Where(x => x.Displayed));
            if (element.Any())
            {
                throw new ElementFoundException(element);
            }
        }

        public static void Hover(this IWebDriver driver, IWebElement element)
        {
            var action = new Actions(driver);
            action.MoveToElement(element).Perform();
            Thread.Sleep(250); //There maybe a animation so wait for that to finish (example, our menus have an delay before they show up)
        }

        public static void EnterText(this IWebElement element, string text)
        {
            element.Clear();
            element.SendKeys(text);
        }

        public static void SendKeys(this IWebDriver driver, string keys)
        {
            Actions action = new Actions(driver);
            action.SendKeys(keys).Build().Perform();
        }

        public static IWebElement FindNthDisplayedElements(this IWebDriver driver, By filter, int count)
        {
            IEnumerable<IWebElement> elements;
            try
            {
                elements = driver.FindElements(filter.Where(e => e.Displayed));

                if (elements.Count() < count)
                {
                    throw new NoSuchElementException();
                }
            }
            catch (StaleElementReferenceException)
            {
                //Try again
                elements = driver.FindElements(filter.Where(e => e.Displayed));
            }

            var element = elements.ElementAt(count - 1);
            BringToVisibleRegion(driver, element);
            return element;
        }

        public static IWebElement FindDisplayedElement(this IWebDriver driver, By filter)
        {
            IWebElement element;
            try
            {
                element = driver.FindElement(filter.Where(e => e.Displayed));
            }
            catch (StaleElementReferenceException)
            {
                //Try again
                element = driver.FindElement(filter.Where(e => e.Displayed));
            }

            BringToVisibleRegion(driver, element);
            return element;
        }

        private const int TopNavigationAreaSize = 83;
        private static void BringToVisibleRegion(IWebDriver driver, IWebElement element)
        {
            if (element.Location.Y <= TopNavigationAreaSize)
                return;
            var scriptRunner = (IJavaScriptExecutor)driver;
            var windowYPos = (long)scriptRunner.ExecuteScript("return window.scrollY");
            if (element.Location.Y - windowYPos < TopNavigationAreaSize)
            {
                scriptRunner.ExecuteScript("window.scrollTo(window.scrollX,arguments[0])", element.Location.Y - TopNavigationAreaSize);
            }
        }

        public static TResult WaitUntil<T, TResult>(this T driver, Func<IWebDriver, TResult> condition)
        {
            var wait = new WebDriverWait(WebBrowser.CurrentBrowser, TimeSpan.FromSeconds(5));
            return wait.Until(condition);
        }

        public static bool IsDisplaying(this IWebDriver browser, By query)
        {
            try
            {
                return browser.FindElement(query).Displayed;
            }
            catch (StaleElementReferenceException)
            {
                //Try again
                return IsDisplaying(browser, query);
            }
        }

        private class NthBy : By
        {
            private readonly By _baseFilter;
            private readonly int _n;

            public NthBy(By baseFilter, int n)
            {
                _baseFilter = baseFilter;
                _n = n;
                FindElementMethod = FindByPredicate;
                FindElementsMethod = FindListByPredicate;
            }

            private ReadOnlyCollection<IWebElement> FindListByPredicate(ISearchContext context)
            {

                return new ReadOnlyCollection<IWebElement>(new[] { FindByPredicate(context) });
            }

            private IWebElement FindByPredicate(ISearchContext context)
            {
                try
                {
                    return context.WaitUntil(c =>
                    {
                        var element = _baseFilter.FindElements(context);
                        if (element.Count < _n + 1) throw new NoSuchElementException();

                        return element.ElementAt(_n);
                    });
                }
                catch (WebDriverTimeoutException)
                {
                    throw new NoSuchElementException();
                }
            }
        }


        private class PredicateBy : By
        {
            private readonly By _baseFilter;
            private readonly Func<IWebElement, bool> _isMatch;

            public PredicateBy(By baseFilter, Func<IWebElement, bool> isMatch)
            {
                _baseFilter = baseFilter;
                _isMatch = isMatch;
                FindElementMethod = FindByPredicate;
                FindElementsMethod = FindListByPredicate;
            }

            private ReadOnlyCollection<IWebElement> FindListByPredicate(ISearchContext context)
            {
                return new ReadOnlyCollection<IWebElement>(_baseFilter.FindElements(context).Where(_isMatch).ToList());
            }

            private IWebElement FindByPredicate(ISearchContext context)
            {
                try
                {
                    return context.WaitUntil(c => _baseFilter.FindElements(context).Where(_isMatch).FirstOrDefault());
                }
                catch (WebDriverTimeoutException)
                {
                    throw new NoSuchElementException();
                }
            }
        }

        public static void ResetMouseCursor(this IWebDriver driver, IWebElement element)
        {
            new Actions(driver).MoveToElement(element, 0, 0).Click().Perform();
        }

        public static IJavaScriptExecutor Scripts(this IWebDriver driver)
        {
            return (IJavaScriptExecutor)driver;
        }

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(InvalidOperationException));
                return wait.Until(ExpectedConditions.ElementExists(by));
            }
            return driver.FindElement(by);
        }

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(InvalidOperationException), typeof(StaleElementReferenceException));
                return wait.Until(drv => (drv.FindElements(by).Count > 0) ? drv.FindElements(by) : null);
            }
            return driver.FindElements(by);
        }

        public static void ExecuteJavascript(string fileName)
        {
            try
            {
                var script = File.ReadAllText(fileName);
                Scripts(WebBrowser.CurrentBrowser).ExecuteScript(script);
            }
            catch (FileNotFoundException exception)
            {
                Assert.Fail("The filename {0} can't be found. {1}", fileName, exception.StackTrace);
            }
        }

        // ToDO this method is a temporary workaround for issue in IE11 where signin button isn't being clicked using click property of selenium 
        // http://code.google.com/p/selenium/issues/detail?id=2766 
        public static void ClickViaJavascript(this IWebElement element)
        {
            var js = (IJavaScriptExecutor)WebBrowser.CurrentBrowser;
            js.ExecuteScript("arguments[0].click();", element);
        }
    }
}
