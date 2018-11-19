using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersHandler
{
    public class OrdersManager
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public void UpdateSupplierOrderId(string orderId, string supplierOrderId)
        {
            try
            {
                logger.Info($"UpdateSupplierOrderId orderId: {orderId} with supplierorderId: {supplierOrderId}");
                using (orders_entity entity = new orders_entity())
                {
                    var order = entity.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
                    if (order == null)
                        throw new ArgumentNullException();
                    order.SupplierOrderID = supplierOrderId;
                    entity.SaveChanges();
                    logger.Info($"Successfully UpdateSupplierOrderId orderId: {orderId} with supplierorderId: {supplierOrderId}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error UpdateSupplierOrderId orderId: {orderId} with supplierorderId: {supplierOrderId}");
            }
        }

        public List<Order> GetPendingOrders()
        {
            return new List<Order>
            {
                new Order
                {
                    SupplierUrl = "https://www.amazon.com/gp/product/B0006KQH6A/ref=ox_sc_act_title_1?smid=ATVPDKIKX0DER&psc=1",
                    PhoneNumber = "7013311836",
                    FirstName = "Laine",
                    LastName = "White",
                    Address1 = "1139 30TH AVE W",
                    City = "WEST FARGO",
                    State = "NORTH DAKOTA",
                    Zipcode = "58078-7939",
                    Quantity = 1,
                    OrderId = "112-5652142-4455408"
                }
            };
            //using(orders_entity entity = new orders_entity())
            //{
            //    return entity.Orders.Where(o => o.Status == (int)OrdersStatus.Pending).ToList();
            //}
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
