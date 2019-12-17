using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Bson;

namespace Kugar.Tool.MongoDBHelper
{
    public static class MongoDBRequestExtMethod
    {
        public static ObjectId GetObjectID(this HttpRequestBase request, string key)
        {
            return GetObjectID(request, key, ObjectId.Empty);
        }

        public static ObjectId GetObjectID(this HttpRequestBase request, string key, ObjectId defaultValue)
        {
            try
            {
                var s = request[key];

                if (string.IsNullOrWhiteSpace(s))
                {
                    return defaultValue;
                }
                else
                {
                    return HttpUtility.UrlDecode(s).ToObjectId(defaultValue);
                }
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static ObjectId GetObjectID(this HttpRequest request, string key)
        {
            return GetObjectID(request, key, ObjectId.Empty);
        }

        public static ObjectId GetObjectID(this HttpRequest request, string key, ObjectId defaultValue)
        {
            try
            {
                var s = request[key];

                if (string.IsNullOrWhiteSpace(s))
                {
                    return defaultValue;
                }
                else
                {
                    return HttpUtility.UrlDecode(s).ToObjectId(defaultValue);
                }
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
