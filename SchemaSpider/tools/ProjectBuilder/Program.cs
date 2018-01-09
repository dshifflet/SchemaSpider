using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using log4net;
using log4net.Config;
using Oracle.ManagedDataAccess.Client;
using SchemaSpider;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace ProjectBuilder
{
    /*
        SAMPLE PARAMETERS
        "..\..\..\..\tests\Northwind\schema_generation\tables.txt" 
        "..\..\..\..\tests\Northwind\schema_generation\northwind_schema.xml" 
        "..\..\..\..\tests\Northwind" "Northwind" "" 
        "..\..\..\..\tests\Northwind\schema_generation\ignore.txt"
    */

    class Program
{
    private static readonly ILog Log = LogManager.GetLogger("ProjectBuilder");

    private static void Main(string[] args)
    {
        if (args.Length != 6)
        {
            Console.WriteLine("<control file> <serialized output> <nh path> <namespace> <plural indicator> <ignore list>");
            return;
        }

        var control = new FileInfo(args[0]);
        if (!control.Exists)
        {
            Console.WriteLine("Control file does not exist");
            return;
        }

        var output = new FileInfo(args[1]);
        var ignoreFile = new FileInfo(args[5]);

        var ignore = new string[] {};
        if (ignoreFile.Exists)
        {
            ignore = File.ReadAllLines(ignoreFile.FullName).OrderBy(s => s.ToString()).ToArray();
        }

        var serializer = new XmlSerializer(typeof(ExplorerResults));

        if (!output.Exists)
        {
            Console.WriteLine("Investigating");
            var cn = new OracleConnection(ConfigurationManager.ConnectionStrings["DATABASE"].ConnectionString);
            var result = Investigate(control, cn, 10, ignore);
            Console.WriteLine(result.Tables.Length);

            using (var sw = new StreamWriter(output.Create()))
            {
                serializer.Serialize(sw, result);
            }
        }
        output.Refresh();
        if (output.Exists)
        {                
            using (var sr = new StreamReader(output.OpenRead()))
            {
                var result = (ExplorerResults) serializer.Deserialize(sr);
                Console.WriteLine("Creating NH files");
                var outpath = new DirectoryInfo(args[2]);
                outpath.Create();
                Explorer.BuildModelMappings(outpath, result, ignore, args[3], args[4]);
                Console.WriteLine("Created at {0}", outpath.FullName);
            }
        }
    }

    private static ExplorerResults Investigate(FileInfo control, IDbConnection cn, int depth, string[] ignorePrefixes)
    {
        try
        {
            cn.Open();
            var tableNames = File.ReadAllLines(control.FullName).OrderBy(s => s.ToString()).ToList();

            Log.InfoFormat("Exploring {0} tables", tableNames.Count);

            return Explorer.Explore(cn,
                tableNames,
                depth, ignorePrefixes);
        }
        finally
        {
            cn.Close();    
        }
    }

}
}
