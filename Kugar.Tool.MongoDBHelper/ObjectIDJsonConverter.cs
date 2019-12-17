using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper
{
    public class ObjectIDJsonConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue(String.Empty);
            }
            else
            {
                var s = (ObjectId)value;
                writer.WriteValue(s.ToStringEx());
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value.ToStringEx().Trim();

            if (objectType == typeof(ObjectId))
            {
                try
                {
                    return new ObjectId(str);
                }
                catch (Exception e)
                {
                    return ObjectId.Empty;
                }
            }
            else if (objectType == typeof(ObjectId?))
            {
                try
                {
                    return new ObjectId(str);
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectId) || objectType == typeof(ObjectId?);
        }
    }


}
