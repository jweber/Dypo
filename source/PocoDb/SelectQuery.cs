using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.VisualStudio.DebuggerVisualizers;
using PocoDb.Interfaces;
using PocoDb.Utility;

namespace PocoDb
{
    internal class SelectQuery<TTable> : ISelectQuery<TTable>
    {
        private static readonly ConcurrentDictionary<string, Delegate> PocoMappers = new ConcurrentDictionary<string, Delegate>();
        private static readonly ConcurrentDictionary<string, string[]> ColumnNames = new ConcurrentDictionary<string, string[]>();

        private readonly IDbContext _dbContext;
        private readonly List<string> _projectionColumns = new List<string>();
        private readonly string _tableName;

        public SelectQuery(IDbContext dbContext, string tableName = null)
        {
            _dbContext = dbContext;
            _tableName = SqlGenerationUtility.GetTableName<TTable>(tableName);
        }

        public IEnumerable<TTable> Execute()
        {
            _projectionColumns.AddRange(GetColumnNames());

            using (var command = _dbContext.DbConnection.CreateCommand())
            {
                command.CommandText = GenerateSql();

                IDataReader reader = command.ExecuteReader();
                var mapper = GetMapper(reader);

                using (reader)
                {
                    while (true)
                    {
                        TTable output;
                        try
                        {
                            if (!reader.Read())
                                yield break;

                            output = mapper(reader);
                        }
                        catch (Exception ex)
                        {
                            var context = _dbContext as DbContext;
                            if (context != null)
                                context.HandleException(ex);

                            throw;
                        }

                        yield return output;
                    }
                }               
            }
        }

        private string GenerateSql()
        {
            return string.Format("SELECT {0} FROM {1}", string.Join(", ", _projectionColumns), _tableName);
        }

        private string GetCacheKey()
        {
            string cacheKey = string.Format("{0}:{1}:{2}", _dbContext.DbConnection.ConnectionString, _tableName, typeof(TTable).FullName);
            return cacheKey;
        }

        private IEnumerable<string> GetColumnNames()
        {
            string cacheKey = GetCacheKey();

            string[] columnNames;
            if (ColumnNames.TryGetValue(cacheKey, out columnNames))
                return columnNames;

            columnNames = (from p in typeof(TTable).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           select SqlGenerationUtility.GetColumnName(p)).ToArray();

            ColumnNames[cacheKey] = columnNames;
            return columnNames;
        }

        private Func<IDataReader, TTable> GetMapper(IDataReader reader)
        {
            string cacheKey = GetCacheKey();

            Delegate mapper;
            if (PocoMappers.TryGetValue(cacheKey, out mapper))
                return mapper as Func<IDataReader, TTable>;

            var m = new DynamicMethod(string.Format("__poco_mapper_{0}", PocoMappers.Count), typeof(TTable), new[] { typeof(IDataReader) }, true);
            var il = m.GetILGenerator();

            il.DeclareLocal(typeof(TTable));

            var ctor = typeof(TTable).GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc_0);

            var getValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var sourceType = reader.GetFieldType(i);
                var sourceName = reader.GetName(i);

                var outputProperty = (from p in typeof(TTable).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     where SqlGenerationUtility.GetColumnName(p) == sourceName
                                           && p.PropertyType == sourceType
                                     select p).FirstOrDefault();

                if (outputProperty == null)
                    continue;

                MethodInfo setValue = outputProperty.GetSetMethod();

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Callvirt, getValue);
                il.Emit(OpCodes.Unbox_Any, sourceType);
                il.Emit(OpCodes.Callvirt, setValue);
            }

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            //TestShowVisualizer(m);

            var @delegate = m.CreateDelegate(typeof(Func<IDataReader, TTable>));
            PocoMappers[cacheKey] = @delegate;

            return @delegate as Func<IDataReader, TTable>;
        }

        public static void TestShowVisualizer(object objectToVisualize)
        {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(ClrTest.Reflection.MethodBodyVisualizer), typeof(ClrTest.Reflection.MethodBodyObjectSource));
            visualizerHost.ShowVisualizer();
        }
    }
}