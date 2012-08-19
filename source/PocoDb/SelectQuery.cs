using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.DebuggerVisualizers;
using PocoDb.Interfaces;
using PocoDb.Utility;

namespace PocoDb
{
    internal class SelectQuery<TTable> : ISelectQuery<TTable>
    {
        private readonly IDbContext _dbContext;
        private readonly List<string> _projectionColumns = new List<string>();
        private readonly string _tableName;

        public SelectQuery(IDbContext dbContext, string tableName = null)
        {
            _dbContext = dbContext;

            _tableName = SqlGenerationUtility.GetTableName<TTable>(tableName);
        }

        private IEnumerable<string> GetPropertyNames()
        {
            var properties = from p in typeof(TTable).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             select p.Name;

            return properties;
        }

        public IEnumerable<TTable> Execute()
        {
            _projectionColumns.AddRange(GetPropertyNames());

            var command = _dbContext.DbConnection.CreateCommand();
            command.CommandText = GenerateSql();

            IDataReader reader = command.ExecuteReader();

            var mapper = GenerateMapper<TTable>(reader);

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
                        throw;
                    }

                    yield return output;
                }
            }
        }

        private string GenerateSql()
        {
            return string.Format("SELECT {0} FROM {1}", string.Join(", ", _projectionColumns), _tableName);
        }

        private static Func<IDataReader, TOutput> GenerateMapper<TOutput>(IDataReader reader)
        {
            var m = new DynamicMethod("test_mapper", typeof(TOutput), new[] { typeof(IDataReader) }, true);
            var il = m.GetILGenerator();

            il.DeclareLocal(typeof(TOutput));

            var ctor = typeof(TOutput).GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc_0);

            var getValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var sourceType = reader.GetFieldType(i);
                var sourceName = reader.GetName(i);

                var outputProperty = from p in typeof(TOutput).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     where p.Name == sourceName
                                        && p.PropertyType == sourceType
                                     select p;

                if (!outputProperty.Any() || outputProperty.Count() > 1)
                    continue;

                MethodInfo setValue = outputProperty.First().GetSetMethod();

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

            var @delegate = (Func<IDataReader, TOutput>)m.CreateDelegate(typeof(Func<IDataReader, TOutput>));
            return @delegate;
        }

        public static void TestShowVisualizer(object objectToVisualize)
        {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(ClrTest.Reflection.MethodBodyVisualizer), typeof(ClrTest.Reflection.MethodBodyObjectSource));
            visualizerHost.ShowVisualizer();
        }
    }
}