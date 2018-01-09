using System.Collections.Generic;

namespace Northwind.Models
{
	//Model for NORTHWIND.PRODUCTS (0 rows)
	public class Products
	{
		public virtual int Id { get; set; } //PRODUCT_ID
		public virtual string ProductName { get; set; } //PRODUCT_NAME
		public virtual string QuantityPerUnit { get; set; } //QUANTITY_PER_UNIT
		public virtual double UnitPrice { get; set; } //UNIT_PRICE
		public virtual int UnitsInStock { get; set; } //UNITS_IN_STOCK
		public virtual int UnitsOnOrder { get; set; } //UNITS_ON_ORDER
		public virtual int ReorderLevel { get; set; } //REORDER_LEVEL
		public virtual string Discontinued { get; set; } //DISCONTINUED

		//PARENTS
		public virtual Categories Categories { get; set; } //CATEGORY_ID [*:1] CATEGORIES.CATEGORY_ID
		public virtual Suppliers Suppliers { get; set; } //SUPPLIER_ID [*:1] SUPPLIERS.SUPPLIER_ID

		//CHILDREN
		public virtual IList<OrderDetails> OrderDetails { get; set; } //PRODUCT_ID [1:*] ORDER_DETAILS.PRODUCT_ID
	}
}
