using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for Products NORTHWIND.PRODUCTS (0 rows)
	internal class ProductsNhMapping : ClassMapping<Products>
	{
		public ProductsNhMapping()
		{
			Schema("NORTHWIND");
			Table("PRODUCTS");
			Id(prop => prop.Id, map =>
			{
				map.Column("PRODUCT_ID");
				//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
			});
			Property(prop => prop.ProductName, map => map.Column("PRODUCT_NAME"));
			Property(prop => prop.QuantityPerUnit, map => map.Column("QUANTITY_PER_UNIT"));
			Property(prop => prop.UnitPrice, map => map.Column("UNIT_PRICE"));
			Property(prop => prop.UnitsInStock, map => map.Column("UNITS_IN_STOCK"));
			Property(prop => prop.UnitsOnOrder, map => map.Column("UNITS_ON_ORDER"));
			Property(prop => prop.ReorderLevel, map => map.Column("REORDER_LEVEL"));
			Property(prop => prop.Discontinued, map => map.Column("DISCONTINUED"));

			//PARENTS
            ManyToOne(prop => prop.Categories, map =>
                {
                    map.Column("CATEGORY_ID");
                    map.Fetch(FetchKind.Select);
                });
            ManyToOne(prop => prop.Suppliers, map =>
                {
                    map.Column("SUPPLIER_ID");
                    map.Fetch(FetchKind.Select);
                });

			//CHILDREN
            Bag(prop => prop.OrderDetails, map =>
            {
                map.Key(key => key.Column("PRODUCT_ID"));
                map.Cascade(Cascade.All);
            }, mapping => mapping.OneToMany());

		}
	}
}
