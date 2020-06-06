using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Microsoft.AspNetCore.Http; 

using MongoDB.Bson;

namespace Kugar.Tool.MongoDBHelper
{
    public static  class RequestBsonValueExtMethod
    {
        public static ObjectId GetObjectId(this HttpRequest request, string key)
        {
            return GetObjectId(request, key, ObjectId.Empty);
        }

        public static ObjectId GetObjectId(this HttpRequest request, string key, ObjectId defaultValue)
        {
            var str = request.GetString(key,"",false);

            ObjectId v;

            if (ObjectId.TryParse(str, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        public static ObjectId? GetObjectIdNullale(this HttpRequest request, string key, ObjectId? defaultValue=null)
        {
            var str = request.GetString(key, "", false);

            ObjectId v;

            if (ObjectId.TryParse(str, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        private static string GetString(this HttpRequest request, string name, string defaultValue, bool autoDecode)
        {
            try
            {
                string value;

                if (request.Method == "GET")
                {
                    value = request.Query[name];
                }
                else
                {
                    if (request.Form.ContainsKey(name))
                    {
                        value = request.Form[name];
                    }
                    else
                    {
                        value = request.Query[name];
                    }
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    return defaultValue;
                }
                else
                {
                    if (autoDecode)
                    {
                        return HttpUtility.UrlDecode(value);
                    }
                    else
                    {
                        return value;
                    }
                }
            }
            catch (Exception)
            {
                return defaultValue;
            }


        }

    }
}
