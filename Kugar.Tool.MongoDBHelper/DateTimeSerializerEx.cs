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
    public class DateTimeNullableSerializer: SerializerBase<DateTime?>
    {
        public override DateTime? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            if (reader.CurrentBsonType == BsonType.DateTime)
            {
                var value = new BsonDateTime(reader.ReadDateTime()).ToLocalTime();

                return (DateTime?)value;
            }
            else if (reader.CurrentBsonType == BsonType.String)
            {
                var str = reader.ReadString();

                return str.ToDateTimeNullable("yyyy-MM-dd HH:mm:ss");
            }
            else if (reader.CurrentBsonType== BsonType.Null)
            {
                reader.ReadNull();

                return null;
            }
            else
            {
                throw new ArgumentException($"数据无法转换为时间");
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime? value)
        {
            var writer = context.Writer;

            var dt = (DateTime?)value;

            if (dt == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteDateTime(dt.Value);
            }
        }

        //public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, decimal value)
        //{
        //    var writer = context.Writer;

        //    var v = (decimal)value;

        //    if (_isGreaterThan34)
        //    {
        //        writer.WriteDecimal128(v);
        //    }
        //    else
        //    {

        //        writer.WriteDecimal128((Decimal128)v);
        //        writer.WriteStartDocument();

        //        writer.WriteDouble("CalcValue", (double)v);
        //        writer.WriteString("Value", v.ToString());

        //        writer.WriteEndDocument();
        //    }
        //}
    }

    public class DateTimeSerializerEx: DateTimeSerializer
    {
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            
            if (reader.CurrentBsonType== BsonType.DateTime)
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
            else if (reader.CurrentBsonType == BsonType.String)
            {
                var str = reader.ReadString();

                return str.ToDateTime();
            }
            else
            {
                throw new ArgumentOutOfRangeException("类型无法转为时间");
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
