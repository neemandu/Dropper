using System;

namespace Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Submitted, 
        Shipped
    }
}
