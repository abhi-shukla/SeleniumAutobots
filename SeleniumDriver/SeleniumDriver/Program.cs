using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumDriver
{
    public class Program
    {
        public static void Main()
        {
            
        }

        public static void Run(Dictionary<string, bool> emails, out List<string> validEmails, out List<string> inValidEmails )
        {
            validEmails = new List<string>();
            inValidEmails = new List<string>();

            
            GoToWebsite();

            Thread.Sleep(1000);

            foreach (KeyValuePair<string, bool> email in emails)
            {
                try
                {
                    var isEmailValid = VerifyEmail(email.Key);
                    if (isEmailValid)
                    {
                        validEmails.Add(email.Key);
                    }
                    else
                    {
                        inValidEmails.Add(email.Key);
                    }
                }
                catch (Exception ex)
                {
                    //Add the email as invalid
                    inValidEmails.Add(email.Key);
                }
            }
        }

        private static bool VerifyEmail(string email)
        {
            var userNameBox = WebBrowser.CurrentBrowser.FindElement(By.CssSelector("input[name='email']"));
            userNameBox.Clear();
            userNameBox.SendKeys(email);

            var verifyBtn = WebBrowser.CurrentBrowser.FindElement(By.Name("bulk"));
            verifyBtn.Submit();

            Thread.Sleep(1000);

            try
            {
                var isInvalid = WebBrowser.CurrentBrowser.FindDisplayedElement(By.CssSelector(".faliure")).Displayed;
                if (isInvalid)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Sollow exception if faliure class is not found
            }
            
            var element2 = WebBrowser.CurrentBrowser.FindDisplayedElement(By.CssSelector("li.success.valid"));
            if (element2 != null)
            {
                return true;
            }
            return false;
        }

        private static void GoToWebsite()
        {
            try
            {
                WebBrowser.CurrentBrowser.Navigate().GoToUrl("http://www.verifyemailaddress.org/");
            }
            catch (Exception)
            {
                GoToWebsite();
            }
        }
    }

    public class DriverInstance
    {

    }
}
