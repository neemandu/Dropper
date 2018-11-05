using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropper.Contracts
{
    public interface IYahooService
    {
        Task<List<string>> GetShippingOrders();
    }
}
