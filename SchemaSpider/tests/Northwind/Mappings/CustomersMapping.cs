using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for Customers NORTHWIND.CUSTOMERS (0 rows)
	internal class CustomersNhMapping : ClassMapping<Customers>
	{
		public CustomersNhMapping()
		{
			Schema("NORTHWIND");
			Table("CUSTOMERS");
			Id(prop => prop.Id, map =>
			{
				map.Column("CUSTOMER_ID");
				//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
			});
			Property(prop => prop.CustomerCode, map => map.Column("CUSTOMER_CODE"));
			Property(prop => prop.CompanyName, map => map.Column("COMPANY_NAME"));
			Property(prop => prop.ContactName, map => map.Column("CONTACT_NAME"));
			Property(prop => prop.ContactTitle, map => map.Column("CONTACT_TITLE"));
			Property(prop => prop.Address, map => map.Column("ADDRESS"));
			Property(prop => prop.City, map => map.Column("CITY"));
			Property(prop => prop.Region, map => map.Column("REGION"));
			Property(prop => prop.PostalCode, map => map.Column("POSTAL_CODE"));
			Property(prop => prop.Country, map => map.Column("COUNTRY"));
			Property(prop => prop.Phone, map => map.Column("PHONE"));
			Property(prop => prop.Fax, map => map.Column("FAX"));

			//CHILDREN
            Bag(prop => prop.Orders, map =>
            {
                map.Key(key => key.Column("CUSTOMER_ID"));
                map.Cascade(Cascade.All);
            }, mapping => mapping.OneToMany());

		}
	}
}
