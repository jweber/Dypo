using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace PocoDb.Utility
{
    internal static class ModelUtility
    {
        //#define ILVIZUALIZE

        private static readonly ConcurrentDictionary<string, Delegate> PocoMappers = new ConcurrentDictionary<string, Delegate>();
        private static readonly ConcurrentDictionary<string, List<Tuple<MemberInfo, string>>> ColumnNames = new ConcurrentDictionary<string, List<Tuple<MemberInfo, string>>>();

        internal static IEnumerable<string> GetColumnNames<TModel>()
        {
            return GetColumnInformationForModel<TModel>().Select(c => c.Item2);
        }

        internal static string GetColumnName<TModel>(MemberInfo memberInfo)
        {
            var columnInfo = GetColumnInformationForModel<TModel>()
                .FirstOrDefault(m => m.Item1 == memberInfo);

            return columnInfo == null 
                ? null 
                : columnInfo.Item2;
        }

        private static IEnumerable<Tuple<MemberInfo, string>> GetColumnInformationForModel<TModel>()
        {
            string cacheKey = GetCacheKey<TModel>();

            List<Tuple<MemberInfo, string>> columnNames;
            if (ColumnNames.TryGetValue(cacheKey, out columnNames))
                return columnNames;

            var modelProperties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            columnNames = (from p in modelProperties
                           select new Tuple<MemberInfo, string>(p, SqlGenerationUtility.GetColumnName(p))).ToList();

            ColumnNames[cacheKey] = columnNames;
            return columnNames;
        }

        internal static Func<IDataReader, TModel> GetMapper<TModel>(IDataReader reader)
        {
            string cacheKey = GetCacheKey<TModel>();

            Delegate mapper;
            if (PocoMappers.TryGetValue(cacheKey, out mapper))
                return mapper as Func<IDataReader, TModel>;

            var m = new DynamicMethod(string.Format("__poco_mapper_{0}", PocoMappers.Count), typeof(TModel), new[] { typeof(IDataReader) }, true);
            var il = m.GetILGenerator();

            il.DeclareLocal(typeof(TModel));

            var ctor = typeof(TModel).GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc_0);

            var getValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var sourceType = reader.GetFieldType(i);
                var sourceName = reader.GetName(i);

                var outputProperty = (from p in typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
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

            #if ILVIZUALIZE
            TestShowVisualizer(m);
            #endif

            var @delegate = m.CreateDelegate(typeof(Func<IDataReader, TModel>));
            PocoMappers[cacheKey] = @delegate;

            return @delegate as Func<IDataReader, TModel>;
        }

        private static string GetCacheKey<TModel>()
        {
            //string cacheKey = string.Format("{0}:{1}:{2}", _dbContext.DbConnection.ConnectionString, _tableName, typeof(TModel).FullName);
            string cacheKey = string.Format("{0}", typeof(TModel).FullName);
            return cacheKey;
        }

        private static void TestShowVisualizer(object objectToVisualize)
        {
            var visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(ClrTest.Reflection.MethodBodyVisualizer), typeof(ClrTest.Reflection.MethodBodyObjectSource));
            visualizerHost.ShowVisualizer();
        }
    }
}
