using System;
using System.Collections.Generic;

namespace Northwind.Models
{
	//Model for NORTHWIND.ORDERS (0 rows)
	public class Orders
	{
		public virtual int Id { get; set; } //ORDER_ID
		public virtual DateTime OrderDate { get; set; } //ORDER_DATE
		public virtual DateTime? RequiredDate { get; set; } //REQUIRED_DATE
		public virtual DateTime? ShippedDate { get; set; } //SHIPPED_DATE
		public virtual int? ShipVia { get; set; } //SHIP_VIA
		public virtual double? Freight { get; set; } //FREIGHT
		public virtual string ShipName { get; set; } //SHIP_NAME
		public virtual string ShipAddress { get; set; } //SHIP_ADDRESS
		public virtual string ShipCity { get; set; } //SHIP_CITY
		public virtual string ShipRegion { get; set; } //SHIP_REGION
		public virtual string ShipPostalCode { get; set; } //SHIP_POSTAL_CODE
		public virtual string ShipCountry { get; set; } //SHIP_COUNTRY

		//PARENTS
		public virtual Customers Customers { get; set; } //CUSTOMER_ID [*:1] CUSTOMERS.CUSTOMER_ID
		public virtual Employees Employees { get; set; } //EMPLOYEE_ID [*:1] EMPLOYEES.EMPLOYEE_ID
		//public virtual OrderDetails OrderDetails { get; set; } //ORDER_ID [*:1] ORDER_DETAILS.ORDER_ID

		//CHILDREN
		public virtual IList<OrderDetails> OrderDetails { get; set; } //ORDER_ID [1:*] ORDER_DETAILS.ORDER_ID
	}
}
