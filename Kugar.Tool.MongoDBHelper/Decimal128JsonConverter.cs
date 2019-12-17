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
    public class Decimal128JsonConverter : JsonConverter
    {
        private static Decimal128JsonConverter _default=new Decimal128JsonConverter();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = (Decimal128) value;

            writer.WriteRaw(d.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
          
                if (objectType== typeof(int))
                {
                    return new Decimal128((int)existingValue);
                }
                else if(objectType==typeof(decimal))
                {
                    return new Decimal128((decimal)existingValue);
                }
                else if (objectType == typeof(long))
                {
                    return new Decimal128((long)existingValue);
                }
                else if (objectType == typeof(double))
                {
                    return new Decimal128((double)existingValue);
                }
                else
                {
                    return new Decimal128(existingValue.ToStringEx().ToDecimal());
                }
            
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Decimal128);
        }

        public static Decimal128JsonConverter Instance => _default;
    }
}
