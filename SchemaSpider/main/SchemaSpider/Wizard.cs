using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SchemaSpider
{
    public class FieldDefinition
    {
        public string TableName { get; set; }
        public string TableSchema { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public string DbFieldName { get; set; }
        public int Length { get; set; }
        public bool AllowsNull { get; set; }
        public string Type { get; set; }
        public bool IsShortBool { get; set; }
    }
    
    public class Wizard
    {
        public string[] IgnorePrefixes { get; set; }

        private readonly Dictionary<string, string> _fieldTypes = new Dictionary<string, string>
                {
                    {"Boolean", "bool"},
                    {"Byte", "byte"},
                    {"SByte", "sbyte"},
                    {"Char", "char"},
                    {"Decimal", "decimal"},
                    {"Double", "double"},
                    {"Single", "float"},
                    {"Int32", "int"},
                    {"Int64", "long"},
                    {"UInt32", "uint"},
                    {"UInt64", "ulong"},
                    {"Object", "object"},
                    {"Int16", "short"},
                    {"UInt16", "ushort"},
                    {"String", "string"},
                    {"DateTime" , "DateTime"}
                };

        public void WriteCsWizard(DataTable table, StreamWriter sw, string thenamespace, string sql, string pluralIndicator)
        {
            sw.WriteLine("using System;");
            sw.WriteLine("using NHibernate.Mapping.ByCode.Conformist;");
            sw.WriteLine("");
            sw.WriteLine("namespace {0}", thenamespace);
            sw.WriteLine("{");
            sw.WriteLine("/*");
            sw.WriteLine(sql);
            sw.WriteLine("*/");
            sw.WriteLine("");
            var fields = GetFields(table.CreateDataReader()).ToArray();
            WriteModelClass(table.TableName, "", sw, fields, null, null, pluralIndicator);
            WriteMappings(table.TableName,"", sw, fields, null,null,null,null, pluralIndicator);
            WriteMaterializer(table, sw, fields);
            sw.WriteLine("}");
        }

        public void WriteModelClass(string className, string comment, StreamWriter sw, IEnumerable<FieldDefinition> fields,
            DbRelationship[] relationships, DbRelationship[] parents, string pluralIndicator)
        {
            sw.WriteLine("\t//{0}", comment);
            sw.WriteLine("\tpublic class {0}", className);            
            sw.WriteLine("\t{");           

            foreach (var field in fields)
            {
                if (parents.Any(o => o.Child.DbFieldName.Equals(field.DbFieldName)) && !field.Name.Equals("Id"))
                {
                    continue;
                }

                var fieldType = field.Type;
                if (field.IsShortBool)
                {
                    fieldType = field.AllowsNull ? "bool?" : "bool";
                }
                sw.WriteLine("\t\tpublic virtual {0} {1} {{ get; set; }} //{2}", fieldType, field.Name, field.DbFieldName);
            }

            if (parents != null && parents.Any())
            {
                sw.WriteLine("");
                sw.WriteLine("\t\t//PARENTS");
                foreach (var item in parents)
                {
                    sw.WriteLine(
                        "\t\tpublic virtual {0} {1} {{ get; set; }} //{2} [*:1] {3}.{4}", 
                        item.Parent.ClassName,
                        item.Parent.ClassName,
                        item.Child.DbFieldName,
                        item.Parent.TableName.ToUpper(),
                        item.Parent.DbFieldName.ToUpper());
                }
            }

            if (relationships != null && relationships.Any())
            {
                sw.WriteLine("");
                sw.WriteLine("\t\t//CHILDREN");
                foreach (var item in relationships)
                {
                    sw.WriteLine(
                        "\t\tpublic virtual IList<{0}> {1}{5} {{ get; set; }} //{2} [1:*] {3}.{4}",
                        item.Child.ClassName,
                        item.Child.ClassName,
                        item.Parent.DbFieldName, 
                        item.Child.TableName.ToUpper(), 
                        item.Child.DbFieldName.ToUpper(),
                        pluralIndicator);
                }
            }
            sw.WriteLine("\t}");
        }

        private void WriteMaterializer(DataTable table, StreamWriter sw, IEnumerable<FieldDefinition> fields)
        {
            var classNm = GetFieldName(table.TableName);

            sw.WriteLine("\t//BASIC MATERIALIZER");
            sw.WriteLine("\tprivate class Materializer{0}", classNm);
            sw.WriteLine("\t{");
            sw.WriteLine("\t\tprivate {0} Materialize(DbDataReader reader)", classNm);
            sw.WriteLine("\t\t{");
            sw.WriteLine("\t\t\treturn new {0}() {{", classNm);

            foreach (var field in fields)
            {
                var elements = new string[4];
                elements[3] = ",";

                if (!field.Type.EndsWith("?") && !field.Type.Equals("string"))
                {
                    elements[0] = string.Format("({0}) ", field.Type);
                }
                else
                {
                    elements[2] = string.Format(" as {0}", field.Type);
                }

                elements[1] = string.Format("reader[\"{0}\"]", field.DbFieldName);
                if (field.IsShortBool)
                {
                    if (field.AllowsNull)
                    {
                        sw.WriteLine("\t\t\t\t{0} = {1}", field.Name,
                            string.Format(
                                "reader[\"{0}\"]==DBNull.Value ? (bool?) null : (short)reader[\"{0}\"]==1,",
                                field.DbFieldName));
                    }
                    else
                    {
                        sw.WriteLine("\t\t\t\t{0} = {1}", field.Name,
                            string.Format("(short) reader[\"{0}\"]==1,", elements[1]));
                    }
                }
                else
                {
                    sw.WriteLine("\t\t\t\t{0} = {1}", field.Name, string.Join("", elements));
                }
            }
            sw.WriteLine("\t\t\t};");
            sw.WriteLine("\t\t}");
            sw.WriteLine("\t}");
            sw.WriteLine();
            sw.WriteLine();
        }

        public void WriteMappings(string className, string comment, StreamWriter sw, IEnumerable<FieldDefinition> fields,
            DbRelationship[] relationships, DbRelationship[] parents, string schemaName, string tableName,
            string pluralIndicator, bool useOldOracleDates = false)
        {
            sw.WriteLine("\t//NHIBERNATE MAPPING");
            sw.WriteLine("\t//{0}", comment);
            sw.WriteLine("\tinternal class {0}NhMapping : ClassMapping<{0}>", className);
            sw.WriteLine("\t{");
            sw.WriteLine("\t\tpublic {0}NhMapping()", className);
            sw.WriteLine("\t\t{");
            sw.WriteLine("\t\t\tSchema(\"{0}\");", schemaName);
            sw.WriteLine("\t\t\tTable(\"{0}\");", tableName);
            //sw.WriteLine("\t\t\tLazy(false);");

            foreach (var field in fields)
            {
                if (parents.Any(o => o.Child.DbFieldName.Equals(field.DbFieldName)) && !field.Name.Equals("Id"))
                {
                    continue;
                }

                if (field.Name == "Id")
                {
                    sw.WriteLine("\t\t\tId(prop => prop.Id, map =>");
                    sw.WriteLine("\t\t\t{");
                    sw.WriteLine("\t\t\t\tmap.Column(\"{0}\");", field.DbFieldName);
                    //todo sequences
                    sw.WriteLine("\t\t\t\t//map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = \"DATA_FILE_ID_SEQ\"}));");
                    sw.WriteLine("\t\t\t});");
                    continue;
                }
                if (field.Type.Equals("object"))
                {
                    sw.WriteLine("\t\t\t//TODO CANNOT MAP TO OBJECT");    
                    sw.WriteLine("\t\t\t//Property(prop => prop.{0}, map => map.Column(\"{1}\"));", field.Name, field.DbFieldName);    
                }
                else if (field.Type.StartsWith("DateTime") && useOldOracleDates)
                {
                    sw.WriteLine(
@"            Property(prop => prop.{0}, map => 
                {{
                    map.Column(""{1}"");
                    map.Type<OracleDateType>();                    
                }});", field.Name, field.DbFieldName);       
                }
                else
                {
                    sw.WriteLine("\t\t\tProperty(prop => prop.{0}, map => map.Column(\"{1}\"));", field.Name, field.DbFieldName);    
                }
            }

            if (parents != null && parents.Any())
            {
                sw.WriteLine("");
                sw.WriteLine("\t\t\t//PARENTS");
                foreach (var item in parents)
                {
                    sw.WriteLine(
@"            ManyToOne(prop => prop.{0}, map =>
                {{
                    map.Column(""{1}"");
                    map.Fetch(FetchKind.Select);
                }});", item.Parent.ClassName, item.Child.DbFieldName);
                }
            }
            
            if (relationships != null && relationships.Any())
            {

                sw.WriteLine("");
                sw.WriteLine("\t\t\t//CHILDREN");                
                foreach (var item in relationships)
                {
                        sw.WriteLine(
@"            Bag(prop => prop.{0}{2}, map =>
            {{
                map.Key(key => key.Column(""{1}""));
                map.Cascade(Cascade.All);
            }}, mapping => mapping.OneToMany());
",
                            item.Child.ClassName,
                            item.Parent.DbFieldName,
                            pluralIndicator
                        );
                }
            }
            sw.WriteLine("\t\t}");
            sw.WriteLine("\t}");
        }

        public IEnumerable<FieldDefinition> GetFields(IDataReader reader)
        {
            return GetFields(reader, null);
        }

        public IEnumerable<FieldDefinition> GetFields(IDataReader reader, string table)
        {
            var result = new List<FieldDefinition>();
            var schema = reader.GetSchemaTable();

            if(schema==null) throw new InvalidOperationException("Unable to find schema");
            if(table==null) table = schema.TableName;
            
            var idx = 0;
            foreach (DataRow row in schema.Rows)
            {
                var field = new FieldDefinition {TableName = table};
                if (table.Contains("."))
                {
                    var splits = table.Split('.');
                    field.TableName = splits[1];
                    field.ClassName = GetFieldName(splits[1]);
                    foreach (var item in IgnorePrefixes)
                    {
                        if (field.TableName.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                        {
                            field.ClassName = GetFieldName(field.TableName.Substring(item.Length));
                        }
                    }
                    field.TableSchema = splits[0];
                }
                field.DbFieldName = row["ColumnName"].ToString();
                field.Length = (int)row["ColumnSize"];
                field.AllowsNull = (bool)row["AllowDBNull"];

                if (field.DbFieldName.ToUpper().EndsWith("_ID") && idx == 0)
                {
                    //this is the PK
                    field.Name = "Id";
                }
                else
                {
                    field.Name = GetFieldName(field.DbFieldName);
                    foreach (var item in IgnorePrefixes)
                    {
                        if (field.DbFieldName.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                        {
                            field.Name = GetFieldName(field.DbFieldName.Substring(item.Length));
                        }
                    }                    
                }
                field.Type = GetDataType(row["DataType"].ToString(), field.AllowsNull);
                if (field.Type.Equals("Byte[]") && field.Length==32)
                {
                    field.Type = "Guid";
                }
                if (field.Type.Equals("object?"))
                {
                    field.Type = "object";
                }
                result.Add(field);
                idx++;
            }
            return result;
        }

        private string GetDataType(string input, bool allownull)
        {
            //https://msdn.microsoft.com/en-us/library/ya5y69ds.aspx            
            if (input.StartsWith("System."))
            {
                var result = input.Replace("System.", "");
                string found;
                if (_fieldTypes.TryGetValue(result, out found))
                {
                    if (found != "string" && allownull)
                    {
                        return string.Concat(found, "?");
                    }
                    return found;
                }
                return result;
            }
            return input;
        }

        private string GetFieldName(string input)
        {
            var result = "";
            for (var i = 0; i < input.Length; i++)
            {
                if (i == 0)
                {
                    result += input[i].ToString().ToUpper();
                    continue;
                }
                if (input[i] == '_')
                {
                    continue;
                }
                if (input[i - 1] == '_')
                {
                    result += input[i].ToString().ToUpper();
                    continue;
                }
                result += input[i].ToString().ToLower();
            }
            return result;
        }

    }
}
