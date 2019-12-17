using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class FilterDefinitionJsonConverter : JsonConverter<FilterDefinition<BsonDocument>>
    {
        public static FilterDefinitionJsonConverter Instance { get; } = new FilterDefinitionJsonConverter();

        public override void WriteJson(JsonWriter writer, FilterDefinition<BsonDocument> value, JsonSerializer serializer)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var doc = value.Render(documentSerializer, serializerRegistry);

            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);

            serializer.Serialize(writer, doc);
        }

        public override FilterDefinition<BsonDocument> ReadJson(JsonReader reader, Type objectType, FilterDefinition<BsonDocument> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var s = serializer.Deserialize(reader, typeof(BsonDocument));

            return (FilterDefinition<BsonDocument>)s;
        }
    }
}