using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;

namespace Northwind.Infrastructure
{
    public static class NhFactory
    {
        public static HbmMapping[] HbmMappings { get; private set; }

        /// <summary>
        /// Provides a NHibernate Session Factory set up for use with the Reports projects.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="assemblies"></param>
        /// <param name="showSql"></param>
        /// <returns></returns>
        public static ISessionFactory CreateNhSessionFactory(string connectionString, Assembly[] assemblies = null, bool showSql = true)
        {

            if (assemblies == null)
            {
                assemblies = new[] {Assembly.GetExecutingAssembly()};
            }
            
            var nhConfiguration = new Configuration();
            nhConfiguration.Cache(properties => properties.Provider<HashtableCacheProvider>());

            nhConfiguration.DataBaseIntegration(dbi =>
            {
                dbi.Dialect<Oracle12cDialect>();
                dbi.Driver<OracleManagedDataClientDriver>();
                dbi.ConnectionProvider<DriverConnectionProvider>();
                dbi.IsolationLevel = IsolationLevel.ReadCommitted;
                dbi.ConnectionString = connectionString;
                dbi.Timeout = 60;
                dbi.LogFormattedSql = true;
                dbi.LogSqlInConsole = false;
            });
            if (showSql)
            {
                nhConfiguration.Properties["show_sql"] = "true";
            }
            

            if (HbmMappings == null || !HbmMappings.Any())
            {
                RegisterMappings(assemblies);
            }
            if (HbmMappings != null)
            {
                HbmMappings.ToList().ForEach(nhConfiguration.AddMapping);
                var assembly = Assembly.GetExecutingAssembly();
                nhConfiguration.AddAssembly(assembly);
                var sessionFactory = nhConfiguration.BuildSessionFactory();
                return sessionFactory;
            }
            throw new HibernateConfigException("Unable to find any mappings for NHibernate.");
        }

        private static void RegisterMappings(Assembly[] assemblies)
        {
            var mapper = new ModelMapper();

            //Any mapper in Mappings add it...
            foreach (Type t in GetMappings(assemblies))
            {
                //mapper.AddMapping<InputFileMapping>();

                var method = typeof(ModelMapper).GetMethods().FirstOrDefault(
                        o => o.Name.Equals("AddMapping") &&
                        o.GetParameters().Length == 0 &&
                        o.ContainsGenericParameters
                );

                if (method == null)
                {
                    throw new InvalidOperationException("Could not retrieve AddMapping method");
                }

                method
                    .MakeGenericMethod(t).Invoke(mapper, null);
            }

            var mappings = new List<ModelMapper>
                           {
                               mapper
                           };
            var hibernateMappings = mappings.Select(map =>
            {
                //If you have two persistent classes with the same (unqualified) name, you should set auto-import="false". 
                //NHibernate will throw an exception if you attempt to assign two classes to the same "imported" name.
                //Say two classes having same name from different assemblies (say ECTM.Models.MyClass and ECTM.DTO.MyClass)
                var hbm = map.CompileMappingForAllExplicitlyAddedEntities();
                hbm.autoimport = false;
                return hbm;
            }).ToArray();
            HbmMappings = hibernateMappings;
        }

        private static IEnumerable<Type> GetMappings(IEnumerable<Assembly> assemblies)
        {
            var result = new List<Type>();
            foreach (var assembly in assemblies)
            {
                result.AddRange(assembly.GetTypes()
                    .Where(o => o.Name.EndsWith("Mapping") && o.Namespace != null &&
                                o.Namespace.EndsWith("Mappings"))
                    .ToArray());
            }
            return result;
        }

    }

}
