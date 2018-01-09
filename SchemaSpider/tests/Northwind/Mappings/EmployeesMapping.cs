using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Infrastructure;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for Employees NORTHWIND.EMPLOYEES (0 rows)
	internal class EmployeesNhMapping : ClassMapping<Employees>
	{
		public EmployeesNhMapping()
		{
			Schema("NORTHWIND");
			Table("EMPLOYEES");
			Id(prop => prop.Id, map =>
			{
				map.Column("EMPLOYEE_ID");
				//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
			});
			Property(prop => prop.Lastname, map => map.Column("LASTNAME"));
			Property(prop => prop.Firstname, map => map.Column("FIRSTNAME"));
			Property(prop => prop.Title, map => map.Column("TITLE"));
			Property(prop => prop.TitleOfCourtesy, map => map.Column("TITLE_OF_COURTESY"));
            Property(prop => prop.Birthdate, map => 
                {
                    map.Column("BIRTHDATE");
                    map.Type<OracleDateType>();                    
                });
            Property(prop => prop.Hiredate, map => 
                {
                    map.Column("HIREDATE");
                    map.Type<OracleDateType>();                    
                });
			Property(prop => prop.Address, map => map.Column("ADDRESS"));
			Property(prop => prop.City, map => map.Column("CITY"));
			Property(prop => prop.Region, map => map.Column("REGION"));
			Property(prop => prop.PostalCode, map => map.Column("POSTAL_CODE"));
			Property(prop => prop.Country, map => map.Column("COUNTRY"));
			Property(prop => prop.HomePhone, map => map.Column("HOME_PHONE"));
			Property(prop => prop.Extension, map => map.Column("EXTENSION"));
			Property(prop => prop.Photo, map => map.Column("PHOTO"));
			Property(prop => prop.Notes, map => map.Column("NOTES"));
			Property(prop => prop.ReportsTo, map => map.Column("REPORTS_TO"));

			//CHILDREN
            Bag(prop => prop.Orders, map =>
            {
                map.Key(key => key.Column("EMPLOYEE_ID"));
                map.Cascade(Cascade.All);
            }, mapping => mapping.OneToMany());

		}
	}
}
