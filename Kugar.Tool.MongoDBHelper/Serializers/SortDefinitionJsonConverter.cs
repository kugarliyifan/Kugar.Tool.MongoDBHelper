using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class SortDefinitionJsonConverter : JsonConverter<SortDefinition<BsonDocument>>
    {
        public static SortDefinitionJsonConverter Instance { get; } = new SortDefinitionJsonConverter();

        public override void WriteJson(JsonWriter writer, SortDefinition<BsonDocument> value, JsonSerializer serializer)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var doc = value.Render(documentSerializer, serializerRegistry);

            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);

            serializer.Serialize(writer, doc);
        }

        public override SortDefinition<BsonDocument> ReadJson(JsonReader reader, Type objectType, SortDefinition<BsonDocument> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var s = serializer.Deserialize(reader, typeof(BsonDocument));

            return (SortDefinition<BsonDocument>)s;
        }
    }
}