using System;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class BulkWriteResultJsonConverter : JsonConverter<BulkWriteResult>
    {
        public static BulkWriteResultJsonConverter Instance { get; } = new BulkWriteResultJsonConverter();

        public override void WriteJson(JsonWriter writer, BulkWriteResult value, JsonSerializer serializer)
        {
            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);

            writer.WriteStartObject();

            writer.WriteProperty("IsAcknowledged", value.IsAcknowledged);
            writer.WriteProperty("IsModifiedCountAvailable", value.IsModifiedCountAvailable);
            writer.WriteProperty("RequestCount", value.RequestCount);


            if (value.IsAcknowledged)
            {
                writer.WriteProperty("DeletedCount", value.DeletedCount);
                writer.WriteProperty("InsertedCount", value.InsertedCount);
                writer.WriteProperty("MatchedCount", value.MatchedCount);
            }

            if (value.IsModifiedCountAvailable)
            {
                writer.WriteProperty("ModifiedCount", value.ModifiedCount);
                writer.WriteProperty("Upserts", value.Upserts);
            }

            writer.WriteEndObject();
        }

        public override BulkWriteResult ReadJson(JsonReader reader, Type objectType, BulkWriteResult existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var json = (JObject)JToken.ReadFrom(reader);

            var isAcknowledged = json.GetBool("IsAcknowledged");
            var isModifiedCountAvailable = json.GetBool("IsModifiedCountAvailable");
            var requestCount = json.GetInt("RequestCount");

            if (isAcknowledged)
            {
                var ret = new BulkWriteResult<BsonDocument>.Acknowledged(
                    requestCount,
                    json.GetLong("MatchedCount"),
                    json.GetLong("DeletedCount"),
                    json.GetLong("InsertedCount"),
                    isModifiedCountAvailable ? (long?)null : json.GetLong("ModifiedCount"),
                    null,
                    null
                );

                return ret;
            }
            else
            {
                var ret = new BulkWriteResult<BsonDocument>.Unacknowledged(requestCount, null);

                return ret;
            }
        }
    }
}