using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using Kugar.Core.BaseStruct;
using MongoDB.Bson;
using MongoDB.Driver;
using Kugar.Core.ExtMethod;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Wrappers;
using Newtonsoft.Json.Linq;


namespace Kugar.Tool.MongoDBHelper
{
    public static class BsonValueExtMethod
    {
        private static BsonValue[] _empty = new BsonValue[0];

        public static bool IsBsonNull(this BsonDocument doc, string name)
        {
            if (!doc.Contains(name))
            {
                return false;
            }

            return doc.GetValue(name).IsBsonNull;
        }

        public static IEnumerable<BsonValue> GetBsonArray(this BsonDocument doc, string name)
        {
            if (!doc.Contains(name) || doc.GetValue(name).IsBsonNull)
            {
                return _empty;
            }

            return doc.GetValue(name).AsBsonArray;
        }

        public static IEnumerable<T> GetBsonArray<T>(this BsonDocument doc, string name,Func<BsonValue,T> castFunc)
        {
            var array = GetBsonArray(doc,name);

            return array.Select(castFunc);
        }

        public static IEnumerable<BsonDocument> GetBsonDocumentArray(this BsonDocument doc, string name)
        {
            foreach (var value in GetBsonArray(doc, name))
            {
                yield return value.AsBsonDocument;
            }
        }

        public static object ToPrimitiveValue(this BsonValue value)
        {
            if (value.IsInt64)
            {
                return value.AsInt64;
            }
            else if (value.IsInt32)
            {
                return value.AsInt32;
            }
            else if (value.IsBoolean)
            {
                return value.AsBoolean;
            }
            else if (value.IsValidDateTime)
            {
                return value.ToUniversalTime();
            }
            else if (value.IsDouble)
            {
                return value.AsDouble;
            }
            else if (value.IsString)
            {
                return value.AsString;
            }
            else if (value.IsObjectId)
            {
                return value.AsObjectId;
            }
            else if (value.IsBsonNull)
            {
                return null;
            }

            else if (value.IsGuid)
            {
                return value.AsGuid;
            }

            return null;
        }

        public static bool HasData(this BsonValue value)
        {
            if (value==null)
            {
                return false;
            }

            if (value.IsBsonNull)
            {
                return false;
            }

            if (value.IsBsonArray)
            {
                return value.AsBsonArray.Count > 0;
            }

            if (value.IsBsonDocument)
            {
                return value.AsBsonDocument.Names.HasData();
            }

            return true;
        }

        public static bool HasData(this BsonDocument value)
        {
            if (value==null)
            {
                return false;
            }

            if (value.IsBsonNull)
            {
                return false;
            }

            if (value.ElementCount<=0)
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<BsonDocument> AsBsonDocumentArray(this BsonValue value)
        {
            if (value != null || value.IsBsonNull || !value.IsBsonArray)
            {
                yield break;
            }

            foreach (var doc in value.AsBsonArray)
            {
                if (doc.IsBsonDocument)
                {
                    yield return doc.AsBsonDocument;
                }
            }

        }

        public static IEnumerable<BsonDocument> AsBsonDocumentArray(this BsonArray value)
        {
            if (value!=null || value.IsBsonNull || !value.IsBsonArray )
            {
                yield break;
            }


            foreach (var doc in value.AsBsonArray)
            {
                if (doc.IsBsonDocument)
                {
                    yield return doc.AsBsonDocument;
                }
            }

        }

        public static BsonDocument GetBsonDocument(this BsonDocument value, string key,BsonDocument defaultValue=null)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var doc = value.GetValue(key);

            if (doc.IsBsonDocument)
            {
                return (BsonDocument) doc;
            }

            return defaultValue;
        }

        public static int GetInt32(this BsonDocument value, string key)
        {
            return GetInt32(value, key, 0);
        }

        public static int GetInt32(this BsonDocument value, string key, int defaultValue = 0)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonNull)
            {
                return defaultValue;
            }
            else if (v.IsNumeric)
            {
                return v.AsInt32;
            }
            else
            {
                //return v.AsInt32;

                return defaultValue;
            }
        }

        public static int? GetInt32Nullable(this BsonDocument doc, string key, int? defaultValue = null)
        {
            if (doc.IsBsonNull(key))
            {
                return defaultValue;
            }
            else
            {
                return doc.GetInt32(key);
            }
        }

        public static long GetInt64(this BsonDocument value, string key,long defaultValue=0l)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonNull)
            {
                return defaultValue;
            }
            else if (v.IsNumeric)
            {
                return v.AsInt64;
            }
            else
            {
                //return v.AsInt32;

                return defaultValue;
            }
        }
        
        public static bool GetBool(this BsonDocument value, string key, bool defaultValue=false)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonNull)
            {
                return defaultValue;
            }
            else
            {
                return v.AsBoolean;
            }

        }

        public static bool? GetBooleanNullable(this BsonDocument doc, string key, bool? defaultValue = null)
        {
            if (doc.IsBsonNull(key))
            {
                return defaultValue;
            }
            else
            {
                return doc.GetBool(key);
            }
        }

        public static DateTime GetDateTime(this BsonDocument value, string key,string format,DateTime defaultValue)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonNull)
            {
                return defaultValue;
            }
            else
            {
                if (v.IsValidDateTime || v.IsBsonDateTime)
                {
                    return v.ToLocalTime();
                }
                else if (v.IsString)
                {
                    return v.AsString.ToDateTime(format, defaultValue).ToLocalTime();
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        public static DateTime GetDateTime(this BsonDocument value, string key)
        {
            return GetDateTime(value,key, DateTime.MinValue);
        }

        public static DateTime? GetDateTimeNullable(this BsonDocument value, string key,string format,DateTime? defaultValue)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonNull)
            {
                return defaultValue;
            }
            else
            {
                if (v.IsValidDateTime || v.IsBsonDateTime)
                {
                    return v.ToLocalTime();
                }
                else if (v.IsString)
                {
                    return v.AsString.ToDateTimeNullable(format, defaultValue)?.ToLocalTime();
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        public static DateTime GetDateTime(this BsonDocument value, string key, DateTime defaultValue)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonDateTime || v.IsValidDateTime)
            {
                return v.ToLocalTime();
            }
            else
            {
                return defaultValue;
            }
        }

        public static DateTime? GetDateTimeNullable(this BsonDocument value, string key, DateTime? defaultValue)
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonDateTime || v.IsValidDateTime)
            {
                return v.ToLocalTime();
            }
            else
            {
                return defaultValue;
            }
        }

        public static DateTime? GetDateTimeNullable(this BsonDocument value, string key)
        {
            return GetDateTimeNullable(value, key, null);
        }

        public static object GetNull(this BsonDocument value, string key)
        {
            return value.GetValue(key).AsBsonNull;
        }

        public static ObjectId GetObjectId(this BsonDocument value, string key)
        {
            return value.GetValue(key).AsObjectId;
        }

        public static ObjectId? GetNullObjectId(this BsonDocument value, string key)
        {
            if (value.Contains(key))
            {
                return value.GetValue(key).AsNullableObjectId;
            }
            else
            {
                return null;
            }

            //return value.GetValue(key).AsNullableObjectId;
        }

        public static string GetString(this BsonDocument value, string key, string defaultValue = "")
        {
            if (!value.Contains(key))
            {
                return defaultValue;
            }

            var v = value.GetValue(key);

            if (v.IsBsonNull)
            {
                return defaultValue;
            }
            else
            {
                if (v.BsonType== BsonType.String)
                {
                    return v.AsString;
                }
                else
                {
                    return v.ToPrimitiveValue()?.ToStringEx() ?? defaultValue;
                }
                return v.AsString;
            }
        }

        public static Guid GetGuid(this BsonDocument value, string key)
        {
            return value.GetValue(key).AsGuid;
        }

        public static BsonDocument SetValue(this BsonDocument doc, string name, BsonValue value)
        {
            if (doc.Contains(name))
            {
                doc[name] = value;
            }
            else
            {
                doc.Add(name, value);
            }

            return doc;
        }

        public static Digit GetDigit(this BsonDocument doc, string name, Digit defaultValue = null)
        {
            var v = doc.GetValue(name);

            if (!v.IsBsonDocument)
            {
                return defaultValue;
                //throw new ArgumentOutOfRangeException("name","数据类型错误，必须是Digit类型的BsonDocument");
            }

            var tmp = (BsonDocument) v;

            if (tmp.Contains("Value"))
            {
                return new Digit(tmp.GetString("Value").ToDecimal());
            }
            else
            {
                return defaultValue;
            }
        }

        public static decimal GetDecimal(this BsonDocument doc, string name, decimal defaultValue = 0m)
        {
            if (doc==null)
            {
                return defaultValue;
            }

            if (!doc.Contains(name))
            {
                return defaultValue;
                //throw new ArgumentOutOfRangeException("name", "数据类型错误");
            }

            var value = doc.GetValue(name);

            if (value.IsBsonNull)
            {
                throw new ArgumentOutOfRangeException("name", "数据为null");
            }

            if (value.IsBsonDocument)
            {
                var tmp = value.AsBsonDocument;

                if (tmp.Contains("Value") && tmp.Contains("CalcValue"))
                {
                    return tmp.GetString("Value").ToDecimal();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(name), "数据不是Digit类型,无法转换");
                }
            }

            if (value.IsString || value.IsNumeric)
            {
                if (value.IsDecimal128)
                {
                    return (decimal)value.AsDecimal128;
                }
                else
                {
                    return value.AsString.ToDecimal(defaultValue);
                }
            }

            return defaultValue;
        }

        public static decimal GetDecimal(this BsonDocument doc, string name)
        {
            if (doc==null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (!doc.Contains(name))
            {
                throw new ArgumentOutOfRangeException("name", "数据类型错误"); 
            }

            var value = doc.GetValue(name);

            if (value.IsBsonNull)
            {
                throw new ArgumentOutOfRangeException("name", "数据为null"); 
            }

            if (value.IsBsonDocument)
            {
                var tmp = value.AsBsonDocument;

                if (tmp.Contains("Value") && tmp.Contains("CalcValue"))
                {
                    return tmp.GetString("Value").ToDecimal();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(name), "数据不是Digit类型,无法转换");
                }
            }

            if (value.IsString || value.IsNumeric)
            {
                if (value.IsDecimal128)
                {
                    return (decimal) value.AsDecimal128;
                }
                else
                {
                    return value.AsString.ToDecimal();
                }
            }
            
            throw new ArgumentOutOfRangeException("name", "数据类型错误"); 

            //var v = doc.GetValue(name);

            //if (!v.IsBsonDocument)
            //{
            //    //return defaultValue;
            //    throw new ArgumentOutOfRangeException("name", "数据类型错误，必须是Digit类型的BsonDocument");
            //}

            //var str = v.AsBsonDocument.GetValue("Value").AsString;

            //return str.ToDecimal();
        }

        public static decimal? GetDecimalNullable(this BsonDocument doc, string name, decimal? defaultValue = null)
        {
            if (doc.GetValue(name).IsBsonNull)
            {
                return defaultValue;
            }

            try
            {
                return GetDecimal(doc, name);
            }
            catch (Exception e)
            {
                return defaultValue;
                throw;
            }

            if (!doc.Contains(name))
            {
                //throw new ArgumentOutOfRangeException("name", "数据类型错误"); 

                return defaultValue;
            }

            var value = doc.GetValue(name);

            if (value.IsBsonNull)
            {
                return defaultValue;
                throw new ArgumentOutOfRangeException("name", "数据类型错误"); 
            }

            if (value.IsString || value.IsNumeric)
            {
                return value.AsString.ToDecimalNullable(defaultValue);
            }

            return defaultValue;

            throw new ArgumentOutOfRangeException("name", "数据类型错误"); 
        }

        /// <summary>
        /// 将指定的BsonDocument 转为泛型对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="doc"></param>
        /// <param name="castFunc">自定义转换函数,如该函数为null,则调用BsonSerializer.Deserialize反序列化到指定的对象中</param>
        /// <returns></returns>
        public static TResult ToObject<TResult>(this BsonDocument doc, Func<BsonDocument, TResult> castFunc=null)
            where TResult : class
        {
            if (doc==null)
            {
                return default(TResult);
            }

            if (castFunc==null)
            {
                return BsonSerializer.Deserialize<TResult>(doc);
            }
            else
            {
                return castFunc(doc);
            }
        }

        public static BsonDocument ToBsonDocument(this BsonElement elm)
        {
            return new BsonDocument(elm);
        }
    }

    public static class JsonExtMethod
    {
        public static ObjectId GetObjectId(this JObject value, string key, ObjectId defaultValue)
        {
            JToken temp = null;

            if (value.TryGetValue(key,out temp))
            {
                return temp.ToStringEx().ToObjectId(defaultValue);
            }
            else
            {
                return defaultValue;
            }

            //return value.GetValue(key).ToStringEx().ToObjectId(defaultValue);
        }

        public static ObjectId? GetNullObjectId(this JObject value, string key, ObjectId? defaultValue = null)
        {
            JToken temp = null;

            if (value.TryGetValue(key,out temp))
            {
                return temp.ToStringEx().ToObjectIdNullable(defaultValue);
            }
            else
            {
                return defaultValue;
            }

            return value.GetValue(key).ToStringEx().ToObjectIdNullable(defaultValue);
        }
    }

    public static class BsonWriterExtMethod
    {
        public static void WriteDateTime(this IBsonWriter writer, string propName, DateTime dt)
        {
            writer.WriteName(propName);

            BsonSerializer.Serialize(writer, typeof(DateTime), dt);
        }

        public static void WriteDateTime(this IBsonWriter writer, string propName, DateTime? dt)
        {
            writer.WriteName(propName);

            if (dt.HasValue)
            {
                BsonSerializer.Serialize(writer, typeof(DateTime), dt.Value);
            }
            else
            {
                writer.WriteNull();
            }
            
        }

        public static void WriteDateTime(this IBsonWriter writer, DateTime dt)
        {
            BsonSerializer.Serialize(writer, typeof(DateTime), dt);
        }

        /// <summary>
        /// 写入一个数组,并且如果value为null时,可以选择是否写入一个空数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="defaultEmptyArray">如果value为null时,可以选择是否写入一个空数组</param>
        public static void WriteArray<T>(this IBsonWriter writer,string propName, T[] value,bool defaultEmptyArray=true)
        {
            writer.WriteName(propName);
            
            if (value==null)
            {
                if (defaultEmptyArray)
                {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                }
                else
                {
                    writer.WriteNull();
                }
            }
            else
            {
                BsonSerializer.Serialize(writer,typeof(T[]),value);
            }
        }

        public static void WriteObjectIdNullable(this IBsonWriter writer, string propName, ObjectId? value)
        {
            writer.WriteName(propName);

            if (value.HasValue)
            {
                writer.WriteObjectId(value.Value);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public static void WriteDateTimeNullable(this IBsonWriter writer, string propName, DateTime? value)
        {
            writer.WriteName(propName);

            if (value.HasValue)
            {
                writer.WriteDateTime(value.Value);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }

    public static class MongoDBExtMethod
    {
        static MongoDBExtMethod()
        {
            _emptySortDefinition = new BsonDocumentSortDefinition<BsonDocument>(new BsonDocument());
            _emptyProjection = new BsonDocumentProjectionDefinition<BsonDocument>(new BsonDocument());
            _emptyFilterDefinition= new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument());
            _emptyUpdateDefinition= new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument());
        }

        //public static VM_PagedList<T> GetPagedList<T>(this MongoCursor<T> src, int pageIndex, int pageSize)
        //{
        //    if (pageIndex <= 0)
        //    {
        //        pageIndex = 1;
        //    }

        //    var tempCursor = new MongoCursor<T>(src.Collection, src.Query, src.ReadPreference, src.Serializer,
        //        src.SerializationOptions);

        //    var count = (int)tempCursor.Count();

        //    if (count <= 0)
        //    {
        //        return new VM_PagedList<T>(new T[0], pageIndex, pageSize, 0);
        //    }

        //    IEnumerable<T> data = null;

        //    src.SetBatchSize(pageSize);

        //    if (pageIndex == 1)
        //    {
        //        data = src.SetLimit(pageSize).ToArray();
        //    }
        //    else
        //    {
        //        data = src.SetSkip((pageIndex - 1) * pageSize).SetLimit(pageSize).ToArray();
        //    }

        //    return new VM_PagedList<T>(data, pageIndex, pageSize, count);
        //}

        //public static VM_PagedList<T> GetPagedList<T>(this MongoCursor<BsonDocument> src, Func<BsonDocument, T> castFunc, int pageIndex, int pageSize)
        //{
        //    if (pageIndex <= 0)
        //    {
        //        pageIndex = 1;
        //    }

        //    var count = (int)src.Count();

        //    if (count <= 0)
        //    {
        //        return new VM_PagedList<T>(null, 1, pageSize);
        //    }

        //    IEnumerable<BsonDocument> tempdata = null;

        //    if (pageIndex == 1)
        //    {
        //        tempdata = src.SetLimit(pageSize);
        //    }
        //    else
        //    {
        //        tempdata = src.SetSkip((pageIndex - 1) * pageSize).SetLimit(pageSize);
        //    }


        //    return new VM_PagedList<T>(tempdata.Cast(x => castFunc(x)), pageIndex, pageSize, count);
        //}

        public static ObjectId ToObjectId(this string str)
        {
            return ToObjectId(str, ObjectId.Empty);
        }

        public static ObjectId ToObjectId(this string str, ObjectId defaultValue)
        {
            ObjectId id;

            if (ObjectId.TryParse(str, out id))
            {
                return id;
            }
            else
            {
                return defaultValue;
            }
        }

        public static ObjectId? ToObjectIdNullable(this string str, ObjectId? defaultValue=null)
        {
            ObjectId id;

            if (ObjectId.TryParse(str, out id))
            {
                return id;
            }
            else
            {
                return defaultValue;
            }
        }

        public static bool IsEmpty(this ObjectId id)
        {
            return id == ObjectId.Empty;
        }

        public static bool IsEmpty(this ObjectId? id)
        {
            if (id.HasValue)
            {
                return IsEmpty(id.Value);
            }
            else
            {
                return true;
            }
        }

        public static BsonArray ToBsonArray<T>(this T[] data)
        {
            if (!data.HasData())
            {
                return new BsonArray();
            }

            if (typeof(T).IsClass && typeof(T) != typeof(string))
            {
                return new BsonArray(data.Select(x => x.ToBsonDocument()));
            }
            else
            {
                return new BsonArray(data);
            }
        }

        public static BsonArray ToBsonArray<T>(this IEnumerable<T> data)
        {
            if (!data.HasData())
            {
                return new BsonArray();
            }

            if (typeof(T).IsClass && typeof(T) != typeof(string))
            {
                return new BsonArray(data.Select(x => x.ToBsonDocument()));
            }
            else
            {
                return new BsonArray(data);
            }
        }

        public static BsonDocument ToBsonDocumentEx(this IMongoQuery query)
        {
            if (query == null || query == Query.Null)
            {
                return null;
            }

            return (BsonDocument)query.GetFieldValue("_wrapped", Flags.InstancePrivate);

            //return BsonDocument.Parse(query.ToString());
        }

        private static readonly BsonDocumentFilterDefinition<BsonDocument> _emptyFilterDefinition;
        public static BsonDocumentFilterDefinition<BsonDocument> ToFilterDefinition(this IMongoQuery query)
        {
            if (query==Query.Null)
            {
                return _emptyFilterDefinition;
                //return new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument());
            }

            return new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx());
        }

        public static BsonDocument ToBsonDocumentEx(this IMongoUpdate updater)
        {
            if (updater == null)
            {
                return null;
            }

            if (updater is UpdateDocument)
            {
                var doc = new BsonDocument();

                var updaterDoc = (UpdateDocument)updater;

                foreach (var c in updaterDoc)
                {
                    doc.Add(c);
                }

                return doc;
            }
            else if (updater is UpdateBuilder)
            {
                return ((UpdateBuilder) updater).ToBsonDocument();
            }
            else if (updater.GetType() == typeof(UpdateBuilder<>))
            {
                return (BsonDocument)updater.CallMethod("ToBsonDocument", Flags.InstancePublic);
            }
            else 
            {
                return updater.ToBsonDocument();
            }
        }

        private static readonly BsonDocumentUpdateDefinition<BsonDocument> _emptyUpdateDefinition = null;
        public static BsonDocumentUpdateDefinition<BsonDocument> ToUpdateDefinition(this IMongoUpdate updater)
        {
            if (updater==null)
            {
                return _emptyUpdateDefinition;
            }

            return new BsonDocumentUpdateDefinition<BsonDocument>(updater.ToBsonDocumentEx());
        }

        public static BsonDocument ToBsonDocumentEx(this IMongoSortBy sort)
        {
            if (sort == null || sort == SortBy.Null)
            {
                return null;
            }

            if (sort is SortByDocument)
            {
                var doc = new BsonDocument();

                var sortDoc = (SortByDocument)sort;

                foreach (var c in sortDoc)
                {
                    doc.Add(c);
                }

                return doc;
            }
            else if (sort is SortByBuilder)
            {
                return ((SortByBuilder) sort).ToBsonDocument();
            }
            else if (sort.GetType() == typeof(SortByBuilder<>))
            {
                return (BsonDocument)sort.CallMethod("ToBsonDocument", Flags.InstancePublic);
            }
            else
            {
                return sort.ToBsonDocument();
            }

        }

        private static readonly BsonDocumentSortDefinition<BsonDocument> _emptySortDefinition = null;
        public static BsonDocumentSortDefinition<BsonDocument> ToSortDefinition(this IMongoSortBy sort)
        {
            if (sort == SortBy.Null || sort==null)
            {
                return _emptySortDefinition;
                //return new BsonDocumentSortDefinition<BsonDocument>(new BsonDocument());
            }

            return new BsonDocumentSortDefinition<BsonDocument>(sort.ToBsonDocumentEx());
        }

        public static BsonDocument ToBsonDocumentEx(this IMongoFields fields)
        {
            if (fields == null || fields == Fields.Null)
            {
                return null;
            }

            if (fields is FieldsDocument)
            {
                var doc = new BsonDocument();

                var fieldsDoc = (FieldsDocument)fields;

                foreach (var c in fieldsDoc)
                {
                    doc.Add(c);
                }

                return doc;
            }
            else if (fields is FieldsBuilder)
            {
                return ((FieldsBuilder) fields).ToBsonDocument();
            }
            else if (fields.GetType() == typeof(FieldsBuilder<>))
            {
                return (BsonDocument) fields.CallMethod("ToBsonDocument",Flags.InstancePublic);
            }
            else
            {
                return fields.ToBsonDocument();

            }
        }

        private static readonly BsonDocumentProjectionDefinition<BsonDocument> _emptyProjection = null;
        public static BsonDocumentProjectionDefinition<BsonDocument> ToProjectionDefinition(this IMongoFields fields)
        {
            if (fields==null || fields == Fields.Null)
            {
                //return null;
                return _emptyProjection;
                //return new BsonDocumentProjectionDefinition<BsonDocument>(new BsonDocument());
            }

            return new BsonDocumentProjectionDefinition<BsonDocument>(fields.ToBsonDocumentEx());
        }
    }

    public static class QueryExtMethod
    {
        /// <summary>
        /// 用于处理当check为true的时候才使用and连接queries参数
        /// </summary>
        /// <param name="query">源查询</param>
        /// <param name="check">为true的时候才连接</param>
        /// <param name="queries">后续查询表达式</param>
        /// <returns></returns>
        public static IMongoQuery AndIf(this IMongoQuery query, bool check,params IMongoQuery[] queries)
        {
            if (check)
            {
                query = Query.And(
                        new JoinMultiSouceToEnumable<IMongoQuery>(query,queries)
                    );
            }

            return query;
        }

        /// <summary>
        /// 用于处理当check为true的时候才使用or连接queries参数
        /// </summary>
        /// <param name="query">源查询</param>
        /// <param name="check">为true的时候才连接</param>
        /// <param name="queries">后续查询表达式</param>
        /// <returns></returns>
        public static IMongoQuery OrIf(this IMongoQuery query, bool check, params IMongoQuery[] queries)
        {
            if (check)
            {
                query = Query.Or(
                    new JoinMultiSouceToEnumable<IMongoQuery>(query,queries)
                );
            }

            return query;
        }

        /// <summary>
        /// 用于处理当check为true的时候才使用not连接queries参数
        /// </summary>
        /// <param name="query">源查询</param>
        /// <param name="check">为true的时候才连接</param>
        /// <param name="query2">后续查询表达式</param>
        /// <returns></returns>
        public static IMongoQuery NotIf(this IMongoQuery query, bool check, IMongoQuery query2)
        {
            if (check)
            {
                query = Query.And(
                    query,
                    Query.Not(query2)
                );
            }

            return query;
        }

    }

    public static class UpdateExtMethod
    {
        /// <summary>
        /// 用于处理当check为true的时候才合并nextUpdater参数指定的更新表达式
        /// </summary>
        /// <param name="sourceUpdater">源更新表达式</param>
        /// <param name="check">为true的时候才连接</param>
        /// <param name="nextUpdater">后续更新表达式</param>
        /// <returns></returns>
        public static IMongoUpdate UpdateIf(this IMongoUpdate sourceUpdater, bool check, IMongoUpdate nextUpdater)
        {
            if (check)
            {
                sourceUpdater=Update.Combine(sourceUpdater,nextUpdater);
            }

            return sourceUpdater;
        }
    }
}
