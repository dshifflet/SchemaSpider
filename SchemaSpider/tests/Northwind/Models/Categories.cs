using System.Collections.Generic;

namespace Northwind.Models
{
	//Model for NORTHWIND.CATEGORIES (0 rows)
	public class Categories
	{
		public virtual int Id { get; set; } //CATEGORY_ID
		public virtual string CategoryName { get; set; } //CATEGORY_NAME
		public virtual string Description { get; set; } //DESCRIPTION
		public virtual string Picture { get; set; } //PICTURE

		//CHILDREN
		public virtual IList<Products> Products { get; set; } //CATEGORY_ID [1:*] PRODUCTS.CATEGORY_ID
	}
}
