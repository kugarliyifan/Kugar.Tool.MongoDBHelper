using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class FieldDefinitionJsonConverter : JsonConverter<FieldDefinition<BsonDocument>>
    {
        public static FieldDefinitionJsonConverter Instance { get; } = new FieldDefinitionJsonConverter();

        public override void WriteJson(JsonWriter writer, FieldDefinition<BsonDocument> value, JsonSerializer serializer)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var doc = value.Render(documentSerializer, serializerRegistry);

            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);

            serializer.Serialize(writer, doc);
        }

        public override FieldDefinition<BsonDocument> ReadJson(JsonReader reader, Type objectType, FieldDefinition<BsonDocument> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var s = serializer.Deserialize(reader, typeof(BsonDocument));

            return (FieldDefinition<BsonDocument>)s;
        }
    }
}