using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for Shippers NORTHWIND.SHIPPERS (0 rows)
	internal class ShippersNhMapping : ClassMapping<Shippers>
	{
		public ShippersNhMapping()
		{
			Schema("NORTHWIND");
			Table("SHIPPERS");
			Id(prop => prop.Id, map =>
			{
				map.Column("SHIPPER_ID");
				//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
			});
			Property(prop => prop.CompanyName, map => map.Column("COMPANY_NAME"));
			Property(prop => prop.Phone, map => map.Column("PHONE"));
		}
	}
}
