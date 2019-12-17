using System;
using MongoDB.Bson;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class ObjectIdToJObjectConverter : JsonConverterBase<ObjectId>
    {
        #region static
        private static readonly ObjectIdToJObjectConverter __instance = new ObjectIdToJObjectConverter();

        /// <summary>
        /// Gets a pre-created instance of a <see cref="ObjectIdConverter"/>.
        /// </summary>
        /// <value>
        /// A <see cref="ObjectIdConverter"/>.
        /// </value>
        public static ObjectIdToJObjectConverter Instance
        {
            get { return __instance; }
        }
        #endregion

        // public methods
        /// <inheritdoc/>
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            //var adapter = reader as BsonReaderAdapter;
            //if (adapter != null && adapter.BsonValue != null && adapter.BsonValue.BsonType == BsonType.ObjectId)
            //{
            //    return ((BsonObjectId)adapter.BsonValue).Value;
            //}

            switch (reader.TokenType)
            {
                case Newtonsoft.Json.JsonToken.Bytes:
                    return new ObjectId((byte[])reader.Value);

                case Newtonsoft.Json.JsonToken.StartObject:
                    return ReadExtendedJson(reader);

                default:
                    var message = string.Format("Error reading ObjectId. Unexpected token: {0}.", reader.TokenType);
                    throw new Newtonsoft.Json.JsonReaderException(message);
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var objectId = (ObjectId)value;

            //var adapter = writer as BsonWriterAdapter;
            //if (adapter != null)
            //{
            //    adapter.WriteObjectId(objectId);
            //}
            //else
            //{
            var jsonDotNetBsonWriter = writer as Newtonsoft.Json.Bson.BsonWriter;
            if (jsonDotNetBsonWriter != null)
            {
                jsonDotNetBsonWriter.WriteObjectId(objectId.ToByteArray());
            }
            else
            {
                WriteExtendedJson(writer, objectId);
            }
            //}
        }

        // private methods
        private ObjectId ReadExtendedJson(Newtonsoft.Json.JsonReader reader)
        {
            ReadExpectedPropertyName(reader, "$oid");
            var hex = ReadStringValue(reader);
            ReadEndObject(reader);

            return ObjectId.Parse(hex);
        }

        private void WriteExtendedJson(Newtonsoft.Json.JsonWriter writer, ObjectId value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("$oid");
            writer.WriteValue(BsonUtils.ToHexString(value.ToByteArray()));
            writer.WriteEndObject();
        }
    }
}