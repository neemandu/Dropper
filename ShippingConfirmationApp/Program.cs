using dropper.Contracts;
using dropper.Services;
using MarketplaceWebServiceOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingConfirmationApp
{
    class Program
    {
        static void Main(string[] args)
        {

            IYahooService yahooSrv = new YahooService();
            List<string> orders = yahooSrv.GetShippingOrders();


            MarketplaceWebServiceOrdersSample m = new MarketplaceWebServiceOrdersSample();
            m.Activate();

        }
    }
}
