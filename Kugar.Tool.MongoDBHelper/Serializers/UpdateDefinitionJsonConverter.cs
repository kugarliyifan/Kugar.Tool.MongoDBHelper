using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class UpdateDefinitionJsonConverter : JsonConverter<UpdateDefinition<BsonDocument>>
    {
        public static UpdateDefinitionJsonConverter Instance { get; } = new UpdateDefinitionJsonConverter();

        public override void WriteJson(JsonWriter writer, UpdateDefinition<BsonDocument> value, JsonSerializer serializer)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var doc = value.Render(documentSerializer, serializerRegistry);

            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);

            serializer.Serialize(writer, doc);
        }

        public override UpdateDefinition<BsonDocument> ReadJson(JsonReader reader, Type objectType, UpdateDefinition<BsonDocument> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var s = serializer.Deserialize(reader, typeof(BsonDocument));

            return (UpdateDefinition<BsonDocument>)s;
        }
    }
}