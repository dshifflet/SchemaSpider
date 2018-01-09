using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for OrderDetails NORTHWIND.ORDER_DETAILS (0 rows)
	internal class OrderDetailsNhMapping : ClassMapping<OrderDetails>
	{
		public OrderDetailsNhMapping()
		{
			Schema("NORTHWIND");
			Table("ORDER_DETAILS");
			Id(prop => prop.Id, map =>
			{
				map.Column("ORDER_ID");
				//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
			});
			Property(prop => prop.UnitPrice, map => map.Column("UNIT_PRICE"));
			Property(prop => prop.Quantity, map => map.Column("QUANTITY"));
			Property(prop => prop.Discount, map => map.Column("DISCOUNT"));

			//PARENTS
            ManyToOne(prop => prop.Orders, map =>
                {
                    map.Column("ORDER_ID");
                    map.Fetch(FetchKind.Select);
                });
            ManyToOne(prop => prop.Products, map =>
                {
                    map.Column("PRODUCT_ID");
                    map.Fetch(FetchKind.Select);
                });
            /*
			//CHILDREN
            Bag(prop => prop.Orders, map =>
            {
                map.Key(key => key.Column("ORDER_ID"));
                map.Cascade(Cascade.All);
            }, mapping => mapping.OneToMany());
            */
		}
	}
}
