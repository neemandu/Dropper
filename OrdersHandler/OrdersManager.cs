using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersHandler
{
    public class OrdersManager
    {
        public bool UpdateSupplierOrderId(string orderId, string supplierOrderId)
        {
            try
            {
                using (orders_entity entity = new orders_entity())
                {
                    var order = entity.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
                    if (order == null)
                        return false;
                    order.SupplierOrderID = supplierOrderId;
                    entity.SaveChanges();
                    return true;
                }
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public List<Order> GetPendingOrders()
        {
            using(orders_entity entity = new orders_entity())
            {
                return entity.Orders.Where(o => o.Status == (int)OrdersStatus.Pending).ToList();
            }
        }

        public List<Order> GetSubmittedOrders()
        {
            using (orders_entity entity = new orders_entity())
            {
                return entity.Orders.Where(o => o.Status == (int)OrdersStatus.Submitted).ToList();
            }
        }

        public List<Order> GetShippedOrders()
        {
            using (orders_entity entity = new orders_entity())
            {
                return entity.Orders.Where(o => o.Status == (int)OrdersStatus.Shipped).ToList();
            }
        }

        public List<Order> GetConfirmedOrders()
        {
            using (orders_entity entity = new orders_entity())
            {
                return entity.Orders.Where(o => o.Status == (int)OrdersStatus.Confirmed).ToList();
            }
        }
    }

    public enum OrdersStatus
    {
        Pending,
        Submitted,
        Shipped,
        Confirmed
    }
}
