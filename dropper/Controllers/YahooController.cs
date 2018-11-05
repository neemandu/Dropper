using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.IO;
using EAGetMail;
using System.Text.RegularExpressions;
using System.IO.Compression;
using dropper.Contracts;
using dropper.Services;
using System.Threading.Tasks;
using MarketplaceWebServiceOrders;

namespace dropper.Controllers
{
    public class YahooController : ApiController
    {
        [HttpGet]
        public async Task<List<string>> GetEmails()
        {
            MarketplaceWebServiceOrdersSample m = new MarketplaceWebServiceOrdersSample();
            m.Activate();


            IYahooService yahooSrv = new YahooService();
            List<string> orders = await yahooSrv.GetShippingOrders();


            return orders;
        }
    }
}
