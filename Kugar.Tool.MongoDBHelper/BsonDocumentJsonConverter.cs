using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonToken = Newtonsoft.Json.JsonToken;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace Kugar.Tool.MongoDBHelper
{
    public class BsonDocumentJsonConverter : JsonConverter
    {

        private static HashSet<Type> _hash = new HashSet<Type>();
        private static HashSet<Type> _updateBuilderType = new HashSet<Type>();
        private static JsonWriterSettings settings = new JsonWriterSettings
        {
            OutputMode = JsonOutputMode.Strict,

        };

        static BsonDocumentJsonConverter()
        {
            _hash = new HashSet<Type>()
            {
                typeof(IEnumerable<BsonDocument>),
                typeof(BsonDocument),
                typeof(IMongoQuery),
                typeof(IMongoUpdate),
                typeof(IMongoFields),
                typeof(IMongoSortBy),
                typeof(FieldsBuilder),
                typeof(SortByBuilder),
                typeof(QueryDocument),
                typeof(UpdateDocument),
                typeof(UpdateBuilder),
                typeof(UpdateBuilder<>),
                typeof(FieldsDocument),
                typeof(SortByDocument),
                //typeof(BsonDocument[])
            };
        }

        public override void WriteJson(
            JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IMongoQuery)
            {
                var v = (QueryDocument)value;

                writer.WriteRawValue(v.ToJson(settings));
            }
            else if (value is UpdateBuilder)
            {
                var v = (UpdateBuilder)value;
                writer.WriteRawValue(v.ToBsonDocument().ToJson(settings));
            }
            else if (value is SortByBuilder)
            {
                var v = (SortByBuilder)value;
                writer.WriteRawValue(v.ToBsonDocument().ToJson(settings));
            }
            else if (value is FieldsBuilder)
            {
                var v = (FieldsBuilder)value;

                writer.WriteRawValue(v.ToBsonDocument().ToJson(settings));
            }
            else if (value is IMongoUpdate)
            {
                var v = (UpdateDocument)value;

                writer.WriteRawValue(v.ToJson(settings));
            }
            else if (value is IMongoFields)
            {
                var v = (FieldsDocument)value;

                writer.WriteRawValue(v.ToJson(settings));
            }
            else if (value is IMongoSortBy)
            {
                var v = (SortByDocument)value;

                writer.WriteRawValue(v.ToJson(settings));
            }
            else if (value is BsonDocument)
            {
                var v = (BsonDocument)value;

                writer.WriteRawValue(v.ToJson(settings));
            }
            else
            {
                var cursor = value as IEnumerable<BsonDocument>;

                if (cursor != null)
                {
                    writer.WriteStartArray();

                    foreach (BsonDocument document in cursor)
                    {
                        writer.WriteRawValue(document.ToJson(settings));
                    }

                    writer.WriteEndArray();
                }
                else
                {
                    var valueType = value.GetType();

                    if (_updateBuilderType.Contains(valueType))
                    {
                        var v = (IMongoUpdate)value;
                        writer.WriteRawValue(v.ToBsonDocument().ToJson(settings));
                    }
                    else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(UpdateBuilder<>))
                    {
                        _updateBuilderType.Add(valueType);

                        var v = (IMongoUpdate)value;
                        writer.WriteRawValue(v.ToBsonDocument().ToJson(settings));
                    }
                }

            }
            
        }

        public override object ReadJson(
            JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jtoken = JToken.ReadFrom(reader);


            var json = jtoken.ToString();

            var bson = BsonDocument.Parse(json);

            if (objectType == typeof(BsonDocument))
            {
                return bson;
            }
            else if (objectType == typeof(IMongoQuery) || objectType == typeof(QueryDocument))
            {
                return new QueryDocument(bson);
            }
            else if (objectType == typeof(IMongoUpdate) || objectType == typeof(UpdateDocument))
            {
                return new UpdateDocument(bson);
            }
            else if (objectType == typeof(IMongoFields) || objectType == typeof(FieldsDocument))
            {

                return new FieldsDocument(bson);
            }
            else if (objectType == typeof(IMongoSortBy) || objectType == typeof(SortByDocument))
            {
                return new SortByDocument(bson);
            }


            throw new NotImplementedException();
    
        }

        public override bool CanConvert(Type objectType)
        {
            return _hash.Contains(objectType);
        }
    }
}
