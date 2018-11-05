using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace Buyer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var driver = new EdgeDriver();

            // Navigate to Bing
            driver.Url = "https://www.amazon.com/Bestpriceam-Stylish-Colorful-Celluloid-Plectrums/dp/B014UR6K7A/ref=sr_1_3?s=musical-instruments&ie=UTF8&qid=1541357750&sr=1-3&keywords=guitar+pick&refinements=p_36%3A-150";

            // Find the search box and query for webdriver
            var element = driver.FindElementById("olp-upd-new");

            element.Click();
            //element.SendKeys(Keys.Enter);

            Console.ReadLine();
            driver.Quit();
        }
    }
}
