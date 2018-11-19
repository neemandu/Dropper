using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium;

namespace AmazonHelper
{
    public static class Helper
    {
        public static string GetAsinFromUrl(string link)
        {
            Match match;
            Regex shipmentIdReg = new Regex(@"/B+[a-zA-Z0-9]+/ref", RegexOptions.IgnoreCase);
            match = shipmentIdReg.Match(link);
            string asin = "";
            if (match.Success)
            {
                asin = match.Value.Substring(1, match.Value.Length - 5);
            }
            else
            {
                Regex otherReg = new Regex(@"(dp/B+[a-zA-Z0-9]{9})", RegexOptions.IgnoreCase);
                match = otherReg.Match(link);
                if (match.Success)
                {
                    asin = match.Value.Substring(3, match.Value.Length - 3);
                }
            }

            return asin;
        }

        public static string GetSellersUrlFromAsin(string asin)
        {
            return $"https://www.amazon.com/gp/offer-listing/{asin}/ref=dp_olp_all_mbc?ie=UTF8&amp;condition=all"; 
        }

        public static bool IsAmazonASeller(string url)
        {
            var driver = new EdgeDriver
            {
                Url = url
            };
            bool ans = false;
            try
            {
                if (driver.FindElementByXPath("//img[@alt='Amazon.com']").Displayed)
                    ans = true;
            }
            catch (NoSuchElementException)
            {
                ans = false;
            }

            driver.Quit();

            return ans;
        }
    }
}
