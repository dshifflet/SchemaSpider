# SCHEMA SPIDER
```
  _________      .__                            _________      .__    .___            
 /   _____/ ____ |  |__   ____   _____ _____   /   _____/_____ |__| __| _/___________ 
 \_____  \_/ ___\|  |  \_/ __ \ /     \\__  \  \_____  \\____ \|  |/ __ |/ __ \_  __ \
 /        \  \___|   Y  \  ___/|  Y Y  \/ __ \_/        \  |_> >  / /_/ \  ___/|  | \/
/_______  /\___  >___|  /\___  >__|_|  (____  /_______  /   __/|__\____ |\___  >__|   
        \/     \/     \/     \/      \/     \/        \/|__|           \/    \/       
```

## OVERVIEW:
This is a tool for generating NHibernate Models and Mappings from an existing database (right now Oracle).  This tool can infer the relationships even if you don't have PK and XKs defined.

This contains a windows console application called ProjectBuilder.  It can be ran like...

EXAMPLE:
```
"..\..\..\..\tests\Northwind\schema_generation\tables.txt" "..\..\..\..\tests\Northwind\schema_generation\northwind_schema.xml" "..\..\..\..\tests\Northwind" "Northwind" "" "..\..\..\..\tests\Northwind
```
USAGE:
```
<table file> <serialized output> <nh path> <namespace> <plural indicator> <ignore list>
```
**table file**: A file that lists the tables to work with.  Look at the file in tests\Northwind\schema_generation\tables.txt.  It will handle multiple schemas.
**serialized output**: It can serialize the output from exploring the database to an xml to save time.  Like if you need to change something, so you don't have to explore a remote DB again.
**nh path**: The path to where the NH mapping and model files will be placed.
**namespace**: The namespace to use
**plural indicator**: If your tables are like northwind "ITEMS" use "".  If they are "ITEM" use "s", this is used for the children properties to denote it's plural.
**ignore list**: Table prefixes to ignore, basically a file with wildcard.  For example if you named everything "TBL_ITEM" use "TBL_" in the ignore list file so your classes are called "Item" and not "TblItem"

## HOW IT EXPLORES:
Basically it will walk across the tables that are defined in the "Table File" via a database, the connection is defined in the "data-connectionStrings.config" file.  It will also look for relationships between tables.  It doesn't use the PK and XK relationships, it's based off the field names.  So if you have INVOICE.INVOICE_ID and you had INVOICE_DETAILS.INVOICE_ID it would infer a relationship of 1:* between INVOICE and INVOICE_DETAILS on INVOICE_ID.  It will do both sides of the relationships both parent and child.

If you are considering generating some mappings and models.  Review the Northwind solution in the tests folder for a sample NhFactory and a work around for the poor OracleDateTime support in NH.  Once you generate the mappings and models, I would build it, correct the errors and review the code that was generated.   A test project working thru the most common paths and how you might use it is also a good idea.

## WHAT DOESN'T IT DO?
* Sequences, It doesn't look these up.  You would need to do them by hand.  So for inserts you would need to do these.
* Only works with Oracle right now.  I tried to stick with what was discoverable via ADO and no special queries, so this should be portable to other DBEs.
* Shorts which are bools or actually shorts.  It will try to guess based on the data it encounters.  Sometimes it might get it wrong.
* There are row counts, you might want to change things around to optimize it.  FetchStrategy etc...

## SAMPLE OUTPUT:

**MODEL**
```
using System.Collections.Generic;

namespace Northwind.Models
{
	//Model for NORTHWIND.CATEGORIES (8 rows)
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
```

**MAPPING**
```
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Northwind.Models;

namespace Northwind.Mappings
{
	//NHIBERNATE MAPPING
	//Mapping for Categories NORTHWIND.CATEGORIES (8 rows)
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
```