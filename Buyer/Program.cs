using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OrdersHandler;

namespace Buyer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            OrdersManager ordersManager = new OrdersManager();
            var orders = ordersManager.GetPendingOrders();
            
            foreach (var order in orders)
            {
                AddToCart(order, order.SupplierUrl);
                if (!string.IsNullOrWhiteSpace(order.SupplierUrl2))
                {
                    AddToCart(order, order.SupplierUrl2);
                }
                if (!string.IsNullOrWhiteSpace(order.SupplierUrl3))
                {
                    AddToCart(order, order.SupplierUrl3);
                }

                var SupplierOrderId = SubmitCart(order);

                ordersManager.UpdateSupplierOrderId(order.OrderId, SupplierOrderId);
            }
            
            Console.ReadLine();

        }

        private static string SubmitCart(Order order)
        {
            var driver = new EdgeDriver
            {
                Url = order.SupplierUrl
            };
            driver.Quit();

            return "";
        }

        private static void AddToCart(Order order, string supplierUrl)
        {
            var driver = new EdgeDriver
            {
                Url = supplierUrl
            };
            driver.Quit();
        }
    }
}
