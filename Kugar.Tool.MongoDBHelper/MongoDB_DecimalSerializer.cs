using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Kugar.Tool.MongoDBHelper
{
    public class MongoDB_DecimalSerializer: SerializerBase<decimal>
    {
        //private IMongoDBContext _context = null;
        private bool _isGreaterThan34 = true;

        public MongoDB_DecimalSerializer(/*IMongoDBContext context*/)
        {
            //_context = context;

            //_isGreaterThan34 = _context.ServerVersion.Major >= 3 && _context.ServerVersion.Major >= 4;
        }

        public override decimal Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            //context.

            var reader = context.Reader;

            if (reader.CurrentBsonType == BsonType.Decimal128)
            {
                return (decimal)reader.ReadDecimal128();
            }
            else if (reader.CurrentBsonType == BsonType.String)
            {
                return reader.ReadString().ToDecimal();
            }
            else if (reader.CurrentBsonType == BsonType.Double)
            {
                return (decimal) reader.ReadDouble();
            }
            else if (reader.CurrentBsonType == BsonType.Int32)
            {
                return (decimal) reader.ReadInt32();
            }
            else if (reader.CurrentBsonType == BsonType.Int64)
            {
                return (decimal) reader.ReadInt64();
            }
            else
            {
                reader.ReadStartDocument();

                var str = reader.FindStringElement("Value");

                //var value = new Digit(str.ToDecimal());

                reader.ReadEndDocument();


                return str.ToDecimal();
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, decimal value)
        {
            var writer = context.Writer;

            var v = (decimal)value;

            if (_isGreaterThan34)
            {
                writer.WriteDecimal128(v);
            }
            else
            {

                writer.WriteDecimal128((Decimal128)v);
                writer.WriteStartDocument();

                writer.WriteDouble("CalcValue", (double)v);
                writer.WriteString("Value", v.ToString());

                writer.WriteEndDocument();
            }
        }

        //public Type ValueType { get; } = typeof(decimal);
    }
}
