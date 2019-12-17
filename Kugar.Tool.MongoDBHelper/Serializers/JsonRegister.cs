using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Integrations.JsonDotNet.Converters;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    public static class JsonSerializerRegister
    {
        public static void RegisterConverter(this JsonSerializerSettings setting)
        {
            var converter = setting.Converters;

            converter.Add(BsonValueConverter.Instance);
            converter.Add(ObjectIdConverter.Instance);
            converter.Add(WriteModelJsonConverter.Instance);
            converter.Add(BulkWriteResultJsonConverter.Instance);
            converter.Add(FilterDefinitionJsonConverter.Instance);
            converter.Add(UpdateDefinitionJsonConverter.Instance);
            converter.Add(ProjectionDefinitionJsonConverter.Instance);
            converter.Add(SortDefinitionJsonConverter.Instance);
            converter.Add(FieldDefinitionJsonConverter.Instance);
            converter.Add(IndexKeysDefinitionJsonConverter.Instance);
        }

        public static void RegisterConverter()
        {
            if (JsonConvert.DefaultSettings == null)
            {
                JsonConvert.DefaultSettings = () =>new JsonSerializerSettings();
            }

            RegisterConverter(JsonConvert.DefaultSettings());
        }
    }
}
