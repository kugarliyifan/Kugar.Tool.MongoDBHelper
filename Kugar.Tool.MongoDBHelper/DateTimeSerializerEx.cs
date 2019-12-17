using System;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using JsonReader = MongoDB.Bson.IO.JsonReader;
using JsonWriter = MongoDB.Bson.IO.JsonWriter;

namespace Kugar.Tool.MongoDBHelper
{
    public class DateTimeSerializerEx: DateTimeSerializer
    {
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var dt = (DateTime)base.Deserialize(context, args);

            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt.ToLocalTime();
            }
            else
            {
                return dt;
            }
        }

        //public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        //{
        //    var dt = (DateTime)base.Deserialize(bsonReader, nominalType, actualType, options);

        //    if (dt.Kind == DateTimeKind.Utc)
        //    {
        //        return dt.ToLocalTime();
        //    }
        //    else
        //    {
        //        return dt;
        //    }
        //}

        //public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        //{
        //    var dt = (DateTime)base.Deserialize(bsonReader, nominalType, options);

        //    if (dt.Kind == DateTimeKind.Utc)
        //    {
        //        return dt.ToLocalTime();
        //    }
        //    else
        //    {
        //        return dt;
        //    }
        //}
    }

    public class BsonDateTimeJsonConverter : JsonConverter
    {
        private static BsonDateTimeJsonConverter _default=new BsonDateTimeJsonConverter();

        private string _dateFormat;

        public BsonDateTimeJsonConverter() : this("yyyy-MM-dd HH:mm:ss")
        { }

        public BsonDateTimeJsonConverter(string dateFormat)
        {
            _dateFormat = dateFormat;
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dt = (BsonDateTime) value;

            writer.WriteValue(dt.ToLocalTime().ToString(_dateFormat));
            writer.Flush();
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return existingValue.ToString().ToDateTime(_dateFormat);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BsonDateTime);
        }

        public static BsonDateTimeJsonConverter Instance => _default;
    }
}
