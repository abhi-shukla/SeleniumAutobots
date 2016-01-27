using System;
using System.Collections.ObjectModel;
using System.Globalization;
using OpenQA.Selenium;

namespace SeleniumDriver
{
    public class ElementFoundException : Exception
    {
        private readonly ReadOnlyCollection<IWebElement> _element;
        public ElementFoundException(ReadOnlyCollection<IWebElement> element)
        {
            _element = element;
        }
        public override string Message
        {
            get { return ToString(); }
        }

        public override string ToString()
        {
            return string.Format("Found {0} elements when expecting nothing",
                                 _element == null ? string.Empty : _element.Count.ToString(CultureInfo.InvariantCulture));
        }
    }
}
