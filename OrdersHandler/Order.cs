//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OrdersHandler
{
    using System;
    using System.Collections.Generic;
    
    public partial class Order
    {
        public string OrderId { get; set; }
        public int Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string SupplierUrl { get; set; }
        public string SupplierUrl2 { get; set; }
        public string SupplierUrl3 { get; set; }
        public string SupplierOrderID { get; set; }
        public string SKU { get; set; }
        public string Asin { get; set; }
    }
}
