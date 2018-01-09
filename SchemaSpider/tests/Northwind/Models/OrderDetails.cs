namespace Northwind.Models
{
	//Model for NORTHWIND.ORDER_DETAILS (0 rows)
	public class OrderDetails
	{
		public virtual int Id { get; set; } //ORDER_ID
		public virtual double UnitPrice { get; set; } //UNIT_PRICE
		public virtual int Quantity { get; set; } //QUANTITY
		public virtual float Discount { get; set; } //DISCOUNT

		//PARENTS
		public virtual Orders Orders { get; set; } //ORDER_ID [*:1] ORDERS.ORDER_ID
		public virtual Products Products { get; set; } //PRODUCT_ID [*:1] PRODUCTS.PRODUCT_ID

		//CHILDREN
		//public virtual IList<Orders> Orders { get; set; } //ORDER_ID [1:*] ORDERS.ORDER_ID
	}
}
