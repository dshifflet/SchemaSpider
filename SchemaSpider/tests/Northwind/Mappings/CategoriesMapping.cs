using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for Categories NORTHWIND.CATEGORIES (0 rows)
	internal class CategoriesNhMapping : ClassMapping<Categories>
	{
		public CategoriesNhMapping()
		{
			Schema("NORTHWIND");
			Table("CATEGORIES");
			Id(prop => prop.Id, map =>
			{
				map.Column("CATEGORY_ID");
				//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
			});
			Property(prop => prop.CategoryName, map => map.Column("CATEGORY_NAME"));
			Property(prop => prop.Description, map => map.Column("DESCRIPTION"));
			Property(prop => prop.Picture, map => map.Column("PICTURE"));

			//CHILDREN
            Bag(prop => prop.Products, map =>
            {
                map.Key(key => key.Column("CATEGORY_ID"));
                map.Cascade(Cascade.All);
            }, mapping => mapping.OneToMany());

		}
	}
}
