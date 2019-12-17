using System;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Kugar.Tool.MongoDBHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class MongoDB_DigitSerializer : SerializerBase<Digit>//  BsonBaseSerializer
    {
        //public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        //{
        //    bsonReader.ReadStartDocument();

        //    var str = bsonReader.FindStringElement("Value");

        //    var value= new Digit(str.ToDecimal());

        //    bsonReader.ReadEndDocument();


        //    return value;
        //    //return base.Deserialize(bsonReader, nominalType, actualType, options);
        //}

        //public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        //{
        //    bsonReader.ReadStartDocument();

        //    var str = bsonReader.FindStringElement("Value");

        //    var value = new Digit(str.ToDecimal());

        //    bsonReader.ReadEndDocument();


        //    return value;

        //    //return base.Deserialize(bsonReader, nominalType, options);
        //}

        //public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        //{
        //    var v = (Digit) value;

        //    if (v==null)
        //    {
        //        bsonWriter.WriteStartDocument();

        //        bsonWriter.WriteDouble("CalcValue", 0);
        //        bsonWriter.WriteString("Value", "0");

        //        bsonWriter.WriteEndDocument();
        //    }
        //    else
        //    {
        //        bsonWriter.WriteStartDocument();

        //        bsonWriter.WriteDouble("CalcValue",v.CalcValue);
        //        bsonWriter.WriteString("Value",v.Value.ToString());

        //        bsonWriter.WriteEndDocument();                
        //    }


        //    //base.Serialize(bsonWriter, nominalType, value, options);
        //}

  

        public override Digit Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader= context.Reader;

            reader.ReadStartDocument();

            var str = reader.FindStringElement("Value");

            var value = new Digit(str.ToDecimal());

            reader.ReadEndDocument();


            return value;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Digit value)
        {
            var writer = context.Writer;

            var v = (Digit)value;

            if (v == null)
            {
                writer.WriteStartDocument();

                writer.WriteDouble("CalcValue", 0);
                writer.WriteString("Value", "0");

                writer.WriteEndDocument();
            }
            else
            {
                writer.WriteStartDocument();

                writer.WriteDouble("CalcValue", v.CalcValue);
                writer.WriteString("Value", v.Value.ToString());

                writer.WriteEndDocument();
            }
        }

        public Type ValueType { get; } = typeof (ObjectId);
    }
}
