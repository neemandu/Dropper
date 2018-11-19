using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using OrdersHandler;
using NLog;

namespace Buyer
{
    public class Program
    {
       static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            try
            {
                logger.Info("**** Buyer Started ****");

                OrdersManager ordersManager = new OrdersManager();
                var orders = ordersManager.GetPendingOrders();

                logger.Info($"Got {orders.Count} new orders");

                foreach (var order in orders)
                {
                    try
                    {
                        logger.Info($"Starting orderId: {order.OrderId}...");
                        AddToCart(order, order.SupplierUrl, 1);
                        if (!string.IsNullOrWhiteSpace(order.SupplierUrl2))
                        {
                            AddToCart(order, order.SupplierUrl2, 2);
                        }
                        if (!string.IsNullOrWhiteSpace(order.SupplierUrl3))
                        {
                            AddToCart(order, order.SupplierUrl3, 3);
                        }

                        var SupplierOrderId = SubmitCart(order);
                        if (string.IsNullOrEmpty(SupplierOrderId) == false) { }
                       //     ordersManager.UpdateSupplierOrderId(order.OrderId, SupplierOrderId);
                    }
                    catch(Exception ex)
                    {
                        logger.Error(ex, ex.Message);
                    }
                }
                logger.Info("**** Buyer Finished ****");
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error");
            }
        }

        private static string SubmitCart(Order order)
        {
            try
            {
                logger.Info($"Submitting order: {order.ToString()}");
                var driver = new EdgeDriver
                {
                    Url = "https://www.amazon.com/gp/cart/view.html?ref=nav_cart"
                };
                Thread.Sleep(3000);
                var quantity = LoadElements(driver, By.Name("quantity"));
                foreach (var sel in quantity)
                {
                    var selectElement = new SelectElement(sel);
                    selectElement.SelectByValue("" + order.Quantity);
                }
                var gifts = driver.FindElementByClassName("sc-gift-option");
                var gift = gifts.FindElement(By.XPath(".//input[@type='checkbox']"));
                gift.Click();
                driver.FindElementByXPath("//input[@name='proceedToCheckout']").Click();
                var addrs = LoadElement(driver, By.XPath("//a[contains(@id, 'add-new-address')]"));
                addrs.Click();
                var addPopup = LoadElement(driver, By.XPath("//input[@name='enterAddressFullName']"));
                addPopup.Clear();
                addPopup.SendKeys($"{order.FirstName} {order.LastName}");
                var add1 = driver.FindElementByXPath("//input[@name='enterAddressAddressLine1']");
                add1.Clear();
                add1.SendKeys(order.Address1);
                var add2 = driver.FindElementByXPath("//input[@name='enterAddressAddressLine2']");
                add2.Clear();
                add2.SendKeys(order.Address2);
                var city = driver.FindElementByXPath("//input[@name='enterAddressCity']");
                city.Clear();
                city.SendKeys(order.City);
                var enterAddressStateOrRegion = driver.FindElementByXPath("//input[@name='enterAddressStateOrRegion']");
                enterAddressStateOrRegion.Clear();
                enterAddressStateOrRegion.SendKeys(order.State);
                var enterAddressPostalCode = driver.FindElementByXPath("//input[@name='enterAddressPostalCode']");
                enterAddressPostalCode.Clear();
                enterAddressPostalCode.SendKeys(order.Zipcode);
                var enterAddressPhoneNumber = driver.FindElementByXPath("//input[@name='enterAddressPhoneNumber']");
                enterAddressPhoneNumber.Clear();
                enterAddressPhoneNumber.SendKeys(order.PhoneNumber);
                var footer = driver.FindElementByClassName("a-popover-footer");
                var submit = footer.FindElements(By.TagName("input"));
                submit.ElementAt(0).Click();
                Thread.Sleep(5000);
                var txts = LoadElements(driver, By.XPath("//textarea[contains(@id, 'message-area')]"));
                foreach (var txtArea in txts)
                {
                    if (txtArea.Displayed)
                    {
                        txtArea.Clear();
                        txtArea.SendKeys($@"Hi {order.FirstName},{Keys.Enter}Thank you for your order!{Keys.Enter}{Keys.Enter}D-Global");
                    }
                }
                var saveGiftOptions = driver.FindElement(By.ClassName("save-gift-button-box"));
                submit = saveGiftOptions.FindElements(By.TagName("input"));
                Thread.Sleep(500);
                submit.ElementAt(0).Click();
                var payment = LoadElement(driver, By.Id("useThisPaymentMethodButtonId"));
                var confirmPayment = payment.FindElement(By.TagName("input"));
                Thread.Sleep(500);
                confirmPayment.Click();
                var placeOrder = LoadElement(driver, By.XPath("//input[contains(@name,'placeYourOrder')]"));
                Thread.Sleep(500);
                placeOrder.Click();
                var orderId = LoadElement(driver, By.XPath("//span[contains(@id,'order-number')]"));
                var orderIdText = orderId.Text;
                driver.Quit();
                logger.Info($"Succefull submitted orderId {order.OrderId}, supplier orderId: {orderIdText} ");
                return orderIdText;
            }
            catch(Exception ex)
            {
                logger.Error(ex, $"Error submitting cart for orderId: {order.OrderId}");
                return null;
            }
        }

        private static void AddToCart(Order order, string supplierUrl, int supplierNumber)
        {
            logger.Info($"Adding item to cart from supplier #{supplierNumber}: {supplierUrl}");
            string asin = AmazonHelper.Helper.GetAsinFromUrl(supplierUrl);
            logger.Info($"ASIN: {asin}");
            string url = AmazonHelper.Helper.GetSellersUrlFromAsin(asin);
            var driver = new EdgeDriver
            {
                Url = url
            };
            var offers = driver.FindElementsByClassName("olpOffer");

            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                try
                {
                    if (offer.FindElement(By.XPath(".//img[@alt='Amazon.com']")).Displayed)
                    {
                        offer.FindElement(By.XPath(".//input[@name='submit.addToCart']")).Click();
                        var elem = LoadElement(driver, By.XPath("//a[contains(@href, '/gp/cart/view.html/ref=lh_cart')]"));
                        elem.Click();
                        break;
                    }
                }
                catch (NoSuchElementException) { }
            }
            driver.Quit();

            logger.Info($"Done adding item to cart from supplier #{supplierNumber}");
        }

        private static ReadOnlyCollection<IWebElement> LoadElements(EdgeDriver driver, By by)
        {
            while (true)
            {
                try
                {
                    var element = driver.FindElements(by);
                    return element;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(2000);
                }
            }
        }

        private static IWebElement LoadElement(EdgeDriver driver, By by)
        {
            while (true)
            {
                try
                {
                    var element = driver.FindElement(by);
                    if (element.Displayed)
                        return element;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
