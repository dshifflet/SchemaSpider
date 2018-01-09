using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using log4net;

namespace SchemaSpider
{
    public static class DbTools
    {
        public static IDbDataParameter CreateParameter(IDbCommand cmd, string name, object value)
        {
            var result = cmd.CreateParameter();
            result.ParameterName = "searchFor";
            result.Value = value;
            return result;
        }    
    }

    public class DbRelationship
    {
        public FieldDefinition Parent { get; set; }
        public FieldDefinition Child { get; set; }
        public bool IsMany { get; set; }
    }

    public class DbTable
    {
        private static readonly ILog Log = LogManager.GetLogger("SchemaSpider.DbTable"); 
        public string Schema { get; set; }
        public string Name { get; set; }
        public int SearchDepth { get; set; }
        public List<FieldDefinition> Fields { get; set; }
        public List<DbRelationship> Relationships { get; set; }
        public DataTable SampleData { get; set; }
        public long NumberOfRows { get; set; }
        
        public static DbTable Materialize(IDbConnection connection, string name, int depth, char schemaSeperator, string[] ignorePrefixes)
        {
            Log.InfoFormat("Examining {0} with depth of {1}", name, depth);
            var result = new DbTable
            {
                Fields = new List<FieldDefinition>(),
                SampleData = new DataTable()
            };

            var splits = name.Split('.');
            if (splits.Length > 1)
            {
                result.Schema = splits[0];
                result.Name = splits[1];
            }
            else
            {
                result.Name = name;
            }

            //todo oracle dependency
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    "select num_rows from all_tables where owner=:owner and table_name=:tablename";
                cmd.Parameters.Add(DbTools.CreateParameter(cmd, "owner", result.Schema.ToUpper()));
                cmd.Parameters.Add(DbTools.CreateParameter(cmd, "tablename", result.Name.ToUpper()));
                var count = (long) (cmd.ExecuteScalar() as decimal? ?? default(decimal));
                result.NumberOfRows = count;
            }

            result.SearchDepth = depth;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = string.Format(@"
                        select * from {0} where rownum<{1}",
                        name, result.SearchDepth + 1);

                using (var reader = cmd.ExecuteReader())
                {
                    var wizard = new Wizard() {IgnorePrefixes = ignorePrefixes};
                    
                    result.Fields.Clear();
                    foreach (var item in wizard.GetFields(reader, name))
                    {
                        item.TableName = result.Name;
                        item.TableSchema = result.Schema;
                        result.Fields.Add(item);
                    }           
         
                    result.SampleData.Load(reader);
                    foreach(DataRow row in result.SampleData.Rows)
                    {                                            
                        foreach (var field in result.Fields.Where(o => o.Type.StartsWith("short")))
                        {
                            field.IsShortBool = true;
                            var val = row[field.DbFieldName] as short? ?? default(short);
                            if (val > 1 || val < -1)
                            {
                                field.IsShortBool = false;
                            }
                        }
                    }
                }
            }
            return result;
        }        
    }

    public class ExplorerResults
    {
        public DbTable[] Tables { get; set; }
    }

    public static class Explorer
    {
        private static readonly ILog Log = LogManager.GetLogger("SchemaSpider.Explorer");

        public static ExplorerResults Explore(IDbConnection connection, IEnumerable<string> tableNames, int searchDepth, string[] ignorePrefixes)
        {
            var schemaSeperator = ".";
            var paramIdent = ":";
            var result = tableNames.Select(name => DbTable.Materialize(connection, name, searchDepth, schemaSeperator[0], ignorePrefixes)).ToList();
            var idx = 0;
            //Look for relationships
            foreach (var table in result)
            {
                idx++;
                Log.InfoFormat("Working with {0} of {1}/{2}", table.Name, idx, result.Count);

                table.Relationships = new List<DbRelationship>();
                var id = table.Fields.FirstOrDefault(o => o.Name == "Id");
                if (id != null && id.DbFieldName!="ID") //The Name ID is too generic. :(
                {
                    var childTables = result.Where(o => o.Name != table.Name).ToArray();
                    foreach (var childTable in childTables)
                    {
                        var possibleFk =
                            childTable.Fields.FirstOrDefault(
                                o => o.DbFieldName.Equals(id.DbFieldName, StringComparison.OrdinalIgnoreCase));
                        if (possibleFk != null)
                        {
                            Log.InfoFormat("> Looking for children in {0} using {1}", childTable.Name, id.DbFieldName);

                            var isRelated = false;
                            var isMany = false;
                            foreach (DataRow row in table.SampleData.Rows)
                            {
                                using (var cmd = connection.CreateCommand())
                                {
                                    cmd.CommandTimeout = 60;

                                    cmd.CommandText = string.Format(@"
                                    select {3} from {0}{1}{2} where {3}={4}searchfor and rownum<{5}",
                                        table.Schema, schemaSeperator, childTable.Name, possibleFk.DbFieldName, 
                                        paramIdent, table.SearchDepth + 1);
                                    cmd.Parameters.Add(DbTools.CreateParameter(cmd, "searchFor", row[id.DbFieldName]));
                                    try
                                    {
                                        var cnt = 0;
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                cnt++;                                                
                                            }
                                        }

                                        if (cnt > 0)
                                        {
                                            isRelated = true;
                                            if (cnt > 1)
                                            {
                                                isMany = true;
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.ErrorFormat("SQL: \"{0}\" Exception: {1}", cmd.CommandText, ex);
                                        //empty
                                    }
                                }                                                            
                            }

                            if (isRelated)
                            {
                                table.Relationships.Add(new DbRelationship()
                                {
                                    IsMany = isMany, //todo full joins?  MANY to MANY???
                                    Parent = id,
                                    Child = possibleFk
                                });
                            }
                        }
                    }
                }
            }

            return new ExplorerResults() {Tables = result.ToArray()};
        }

        public static void BuildModel(DbTable table, string nameSpace, StreamWriter sw, 
            string[] ignorePrefixes, DbRelationship[] relationships, string pluralIndicator)
        {
            if (!table.Fields.Any()) return;

            var parents =
                relationships.Where(o => o.Child.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase) &&
                                         o.Child.TableSchema.Equals(table.Schema, StringComparison.OrdinalIgnoreCase))
                    .ToArray();           

            var groups = table.Fields.GroupBy(o => o.ClassName);
            foreach (var item in groups)
            {
                if (!item.Any()) continue;

                var className = item.First().ClassName;
                var wizard = new Wizard() { IgnorePrefixes = ignorePrefixes };
                var children = table.Relationships.ToArray();

                var useSystem = new[] {"DateTime", "DateTime?", "Guid", "Byte", "Object"};

                if (item.Any(o=>useSystem.Contains(o.Type)))
                {
                    sw.WriteLine("using System;");
                }

                if (children.Any())
                {                    
                    sw.WriteLine("using System.Collections.Generic;");
                }                
                sw.WriteLine("");
                sw.WriteLine("namespace {0}.Models", nameSpace);
                sw.WriteLine("{");
                wizard.WriteModelClass(className, string.Format("Model for {0}.{1} ({2} rows)",table.Schema.ToUpper(), table.Name.ToUpper(), table.NumberOfRows)
                    , sw, item, children, parents, pluralIndicator);
                sw.WriteLine("}");             
            }
        }

        public static void BuildMapping(DbTable table, string nameSpace, StreamWriter sw, string[] ignorePrefixes,
            DbRelationship[] relationships, string pluralIndicator)
        {
            if (!table.Fields.Any()) return;

            var parents =
                relationships.Where(o => o.Child.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase) &&
                                         o.Child.TableSchema.Equals(table.Schema, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

            var groups = table.Fields.GroupBy(o => o.ClassName);
            foreach (var item in groups)
            {
                const bool useOldOracleDates = true;

                if (!item.Any()) continue;

                var className = item.First().ClassName;
                var wizard = new Wizard() { IgnorePrefixes = ignorePrefixes };
                var children = table.Relationships.ToArray();
                if (parents.Any() || children.Any())
                {
                    sw.WriteLine("using NHibernate.Mapping.ByCode;");
                }
                sw.WriteLine("using NHibernate.Mapping.ByCode.Conformist;");

                // ReSharper disable once RedundantLogicalConditionalExpressionOperand Going to do something with this
                if (item.Any(o => o.Type.StartsWith("DateTime") && useOldOracleDates))
                {
                    sw.WriteLine("using {0}.Infrastructure;", nameSpace);    
                }

                sw.WriteLine("using {0}.Models;", nameSpace);

                sw.WriteLine("");
                sw.WriteLine("namespace {0}.Mappings", nameSpace);
                sw.WriteLine("{");
                wizard.WriteMappings(className, string.Format("Mapping for {0} {1}.{2} ({3} rows)", className, table.Schema.ToUpper(), table.Name.ToUpper(), table.NumberOfRows)
                    , sw, item, children, parents, table.Schema, table.Name, pluralIndicator, useOldOracleDates);

                sw.WriteLine("}");
            }
        }

        public static void BuildModelMappings(DirectoryInfo di, ExplorerResults result, string[] ignore, string nmSpace,
            string pluralIndicator)
        {
            di.CreateSubdirectory("models");
            di.CreateSubdirectory("mappings");

            foreach (var table in result.Tables)
            {
                var first = table.Fields.First();
                using (var sw = new StreamWriter(new FileInfo(string.Format(@"{0}\models\{1}.cs",
                    di.FullName,
                    first.ClassName
                )).Create()))
                {
                    BuildModel(table, nmSpace, sw, ignore, result.Tables.SelectMany(o => o.Relationships).ToArray(),
                        pluralIndicator);

                }
                using (var sw = new StreamWriter(new FileInfo(string.Format(@"{0}\mappings\{1}Mapping.cs",
                    di.FullName,
                    first.ClassName
                )).Create()))
                {
                    BuildMapping(table, nmSpace, sw, ignore,
                        result.Tables.SelectMany(o => o.Relationships).ToArray(),
                        pluralIndicator);
                }
            }            
        }
    }
}
