using System.Collections.Generic;

namespace Northwind.Models
{
	//Model for NORTHWIND.CUSTOMERS (0 rows)
	public class Customers
	{
		public virtual int Id { get; set; } //CUSTOMER_ID
		public virtual string CustomerCode { get; set; } //CUSTOMER_CODE
		public virtual string CompanyName { get; set; } //COMPANY_NAME
		public virtual string ContactName { get; set; } //CONTACT_NAME
		public virtual string ContactTitle { get; set; } //CONTACT_TITLE
		public virtual string Address { get; set; } //ADDRESS
		public virtual string City { get; set; } //CITY
		public virtual string Region { get; set; } //REGION
		public virtual string PostalCode { get; set; } //POSTAL_CODE
		public virtual string Country { get; set; } //COUNTRY
		public virtual string Phone { get; set; } //PHONE
		public virtual string Fax { get; set; } //FAX

		//CHILDREN
		public virtual IList<Orders> Orders { get; set; } //CUSTOMER_ID [1:*] ORDERS.CUSTOMER_ID
	}
}
