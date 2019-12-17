using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class ProjectionDefinitionJsonConverter : JsonConverter<ProjectionDefinition<BsonDocument>>
    {
        public static ProjectionDefinitionJsonConverter Instance { get; } = new ProjectionDefinitionJsonConverter();

        public override void WriteJson(JsonWriter writer, ProjectionDefinition<BsonDocument> value, JsonSerializer serializer)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var doc = value.Render(documentSerializer, serializerRegistry);

            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);
            
            serializer.Serialize(writer, doc);
        }

        public override ProjectionDefinition<BsonDocument> ReadJson(JsonReader reader, Type objectType, ProjectionDefinition<BsonDocument> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var s = serializer.Deserialize(reader, typeof(BsonDocument));

            return (ProjectionDefinition<BsonDocument>)s;
        }
    }
}