using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropper.Contracts
{
    public interface IAmazonOrders
    {
        string GetTrackingNumberBuOrderId(string orderId);
    }
}
