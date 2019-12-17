using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class IndexKeysDefinitionJsonConverter: JsonConverter<IndexKeysDefinition<BsonDocument>>
    {
        public static IndexKeysDefinitionJsonConverter Instance { get; }=new IndexKeysDefinitionJsonConverter();

        public override void WriteJson(JsonWriter writer, IndexKeysDefinition<BsonDocument> value, JsonSerializer serializer)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var doc = value.Render(documentSerializer, serializerRegistry);

            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);

            serializer.Serialize(writer, doc);
        }


        public override IndexKeysDefinition<BsonDocument> ReadJson(JsonReader reader, Type objectType, IndexKeysDefinition<BsonDocument> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var s = serializer.Deserialize(reader, typeof(BsonDocument));

            return (BsonDocumentIndexKeysDefinition<BsonDocument>)((BsonDocument)s);
        }
    }
}
