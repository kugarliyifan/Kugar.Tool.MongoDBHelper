using System;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public class WriteModelJsonConverter : JsonConverter
    {
        public static WriteModelJsonConverter Instance { get; } = new WriteModelJsonConverter();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Converters.Add(ObjectIdToJObjectConverter.Instance);

            writer.WriteStartObject();

            if (value is UpdateOneModel<BsonDocument>)
            {
                writer.WriteProperty("type", 1);

                var model = (UpdateOneModel<BsonDocument>)value;

                writer.WriteProperty("filter", ((BsonDocumentFilterDefinition<BsonDocument>)model.Filter).Document);
                writer.WriteProperty("update", ((BsonDocumentUpdateDefinition<BsonDocument>)model.Update).Document);
                writer.WriteProperty("upsert", model.IsUpsert);
            }
            else if (value is UpdateManyModel<BsonDocument>)
            {
                writer.WriteProperty("type", 2);

                var model = (UpdateManyModel<BsonDocument>)value;

                writer.WriteProperty("filter", ((BsonDocumentFilterDefinition<BsonDocument>)model.Filter).Document);
                writer.WriteProperty("update", ((BsonDocumentUpdateDefinition<BsonDocument>)model.Update).Document);
                writer.WriteProperty("upsert", model.IsUpsert);
            }
            else if (value is DeleteOneModel<BsonDocument>)
            {
                writer.WriteProperty("type", 3);

                var model = (DeleteOneModel<BsonDocument>)value;

                writer.WriteProperty("filter", ((BsonDocumentFilterDefinition<BsonDocument>)model.Filter).Document);
            }
            else if (value is DeleteManyModel<BsonDocument>)
            {
                writer.WriteProperty("type", 4);

                var model = (DeleteManyModel<BsonDocument>)value;

                writer.WriteProperty("filter", ((BsonDocumentFilterDefinition<BsonDocument>)model.Filter).Document);
            }
            else if (value is InsertOneModel<BsonDocument>)
            {
                writer.WriteProperty("type", 5);

                var model = (InsertOneModel<BsonDocument>)value;

                writer.WriteProperty("document", model.Document);
            }

            writer.WriteEndObject();
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var json = (JObject)JObject.ReadFrom(reader);

            var type = json.GetInt("type");

            WriteModel<BsonDocument> model = null;

            switch (type)
            {
                case 1: //1为UpdateOneModel
                {
                    var filter =
                        new BsonDocumentFilterDefinition<BsonDocument>(json.GetJObject("filter")
                            .ToObject<BsonDocument>());
                    var update =
                        new BsonDocumentUpdateDefinition<BsonDocument>(json.GetJObject("update")
                            .ToObject<BsonDocument>());
                    model = new UpdateOneModel<BsonDocument>(filter, update)
                            {
                                IsUpsert = json.GetBool("upsert", false)
                            };
                    break;
                }
                case 2: //UpdateManyModel
                {
                    var filter =
                        new BsonDocumentFilterDefinition<BsonDocument>(json.GetJObject("filter")
                            .ToObject<BsonDocument>());
                    var update =
                        new BsonDocumentUpdateDefinition<BsonDocument>(json.GetJObject("update")
                            .ToObject<BsonDocument>());
                    model = new UpdateManyModel<BsonDocument>(filter, update)
                            {
                                IsUpsert = json.GetBool("upsert", false)
                            };
                    break;
                }
                case 3: //DeleteOneModel
                {
                    var filter =
                        new BsonDocumentFilterDefinition<BsonDocument>(json.GetJObject("filter")
                            .ToObject<BsonDocument>());
                    model = new DeleteOneModel<BsonDocument>(filter);
                    break;
                }
                case 4: //DeleteManyModel
                {
                    var filter =
                        new BsonDocumentFilterDefinition<BsonDocument>(json.GetJObject("filter")
                            .ToObject<BsonDocument>());
                    model = new DeleteManyModel<BsonDocument>(filter);
                    break;
                }
                case 5: //InsertOne
                {
                    model = new InsertOneModel<BsonDocument>(json.GetJObject("document").ToObject<BsonDocument>());
                    break;
                }
                //case 6: //InsertMany
                //{
                //    model = new insertb<BsonDocument>(json.GetJObject("document").ToObject<BsonDocument>());
                //    break;
                //}
                default:
                {
                    throw new ArgumentOutOfRangeException("type");
                }
            }

            return model;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(WriteModel<BsonDocument>));
        }
    }
}