using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Infrastructure;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for Orders NORTHWIND.ORDERS (0 rows)
	internal class OrdersNhMapping : ClassMapping<Orders>
	{
		public OrdersNhMapping()
		{
			Schema("NORTHWIND");
			Table("ORDERS");
			Id(prop => prop.Id, map =>
			{
				map.Column("ORDER_ID");
				//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
			});
            Property(prop => prop.OrderDate, map => 
                {
                    map.Column("ORDER_DATE");
                    map.Type<OracleDateType>();                    
                });
            Property(prop => prop.RequiredDate, map => 
                {
                    map.Column("REQUIRED_DATE");
                    map.Type<OracleDateType>();                    
                });
            Property(prop => prop.ShippedDate, map => 
                {
                    map.Column("SHIPPED_DATE");
                    map.Type<OracleDateType>();                    
                });
			Property(prop => prop.ShipVia, map => map.Column("SHIP_VIA"));
			Property(prop => prop.Freight, map => map.Column("FREIGHT"));
			Property(prop => prop.ShipName, map => map.Column("SHIP_NAME"));
			Property(prop => prop.ShipAddress, map => map.Column("SHIP_ADDRESS"));
			Property(prop => prop.ShipCity, map => map.Column("SHIP_CITY"));
			Property(prop => prop.ShipRegion, map => map.Column("SHIP_REGION"));
			Property(prop => prop.ShipPostalCode, map => map.Column("SHIP_POSTAL_CODE"));
			Property(prop => prop.ShipCountry, map => map.Column("SHIP_COUNTRY"));

			//PARENTS
            ManyToOne(prop => prop.Customers, map =>
                {
                    map.Column("CUSTOMER_ID");
                    map.Fetch(FetchKind.Select);
                });
            ManyToOne(prop => prop.Employees, map =>
                {
                    map.Column("EMPLOYEE_ID");
                    map.Fetch(FetchKind.Select);
                });
            /*
            ManyToOne(prop => prop.OrderDetails, map =>
                {
                    map.Column("ORDER_ID");
                    map.Fetch(FetchKind.Select);
                });
            */

			//CHILDREN
            Bag(prop => prop.OrderDetails, map =>
            {
                map.Key(key => key.Column("ORDER_ID"));
                map.Cascade(Cascade.All);
            }, mapping => mapping.OneToMany());

		}
	}
}
