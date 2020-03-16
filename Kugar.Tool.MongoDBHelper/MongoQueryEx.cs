using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using BSON=MongoDB.Bson.BsonDocument;

namespace Kugar.Tool.MongoDBHelper
{
    public static class MongoQueryEx
    {
        public static IMongoQuery Like(string propertyName, string keyword, bool isIgoneCause = true)
        {
            if (isIgoneCause)
            {
                //var regex = BsonRegularExpression.Create(new Regex(string.Format("^*{0}*&", keyword), RegexOptions.IgnoreCase));
             
                return Query.Matches(propertyName, string.Format("/{0}/i", keyword));
            }
            else
            {
                return Query.Matches(propertyName, string.Format("/{0}/", keyword));
            }
        }


        /// <summary>
        /// 用于聚合时正则表达式匹配使用
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="regex"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IMongoQuery RegexMatch(string propertyName, string regex, string options="i")
        {
            return Query.Create(new BsonDocument()
            {
                ["$regexMatch"] = new BsonDocument()
                {
                    ["input"] = propertyName[0] == '$' ? propertyName : $"${propertyName}",
                    ["regex"] = new BsonRegularExpression(regex),
                    ["options"] = options
                }
            });
        }

        public static IMongoQuery IsEqual(string propertyName, string value, bool isIgoneCause = true)
        {
            if (isIgoneCause)
            {
                return Query.Matches(propertyName, string.Format("/^{0}$/i", value));
            }
            else
            {
                return Query.EQ(propertyName, value);
            }
        }

        public static IMongoQuery StartWith(string propertyName, string value, bool isIgoneCause = true)
        {
            if (isIgoneCause)
            {
                return Query.EQ(propertyName, string.Format("/^{0}/i", value));
            }
            else
            {
                return Query.EQ(propertyName, string.Format("/^{0}/", value));
            }
        }

        public static IMongoQuery EndWith(string propertyName, string value, bool isIgoneCause = true)
        {
            if (isIgoneCause)
            {
                return Query.EQ(propertyName, string.Format("/{0}$/i", value));
            }
            else
            {
                return Query.EQ(propertyName, string.Format("/{0}$/", value));
            }
        }

        public static IMongoQuery Between(string propertyName, int? start=null, int? end=null)
        {
            if (start.HasValue || end.HasValue)
            {
                IMongoQuery q = null;

                if (start.HasValue)
                {
                    q = Query.GTE(propertyName, start.Value);
                }

                if (end.HasValue)
                {
                    if (start.HasValue)
                    {
                        q = Query.And(
                            q,
                            Query.LTE(propertyName, end.Value)
                        );
                    }
                    else
                    {
                        q = Query.LTE(propertyName, end.Value);
                    }
                }

                return q;
            }
            else
            {
                return Query.Null;
            }
        }

        public static IMongoQuery Between(string propertyName, DateTime? start=null, DateTime? end=null)
        {
            if (start.HasValue || end.HasValue)
            {
                IMongoQuery q = null;

                if (start.HasValue)
                {
                    q = Query.GTE(propertyName, start.Value);
                }

                if (end.HasValue)
                {
                    if (start.HasValue)
                    {
                        q = Query.And(
                            q,
                            Query.LTE(propertyName, end.Value)
                        );
                    }
                    else
                    {
                        q = Query.LTE(propertyName, end.Value);
                    }
                }

                return q;
            }
            else
            {
                return Query.Null;
            }
        }

        public static IMongoQuery Between(string propertyName, decimal? start = null, decimal? end = null)
        {
            if (start.HasValue || end.HasValue)
            {
                IMongoQuery q = null;

                if (start.HasValue)
                {
                    q = Query.GTE(propertyName, start.Value);
                }

                if (end.HasValue)
                {
                    if (start.HasValue)
                    {
                        q = Query.And(
                            q,
                            Query.LTE(propertyName, end.Value)
                        );
                    }
                    else
                    {
                        q = Query.LTE(propertyName, end.Value);
                    }
                }

                return q;
            }
            else
            {
                return Query.Null;
            }
        }

        public static IMongoQuery Between(string propertyName, double? start = null, double? end = null)
        {
            if (start.HasValue || end.HasValue)
            {
                IMongoQuery q = null;

                if (start.HasValue)
                {
                    q = Query.GTE(propertyName, start.Value);
                }

                if (end.HasValue)
                {
                    if (start.HasValue)
                    {
                        q = Query.And(
                            q,
                            Query.LTE(propertyName, end.Value)
                        );
                    }
                    else
                    {
                        q = Query.LTE(propertyName, end.Value);
                    }
                }

                return q;
            }
            else
            {
                return Query.Null;
            }
        }
    }

    public class MongoGroupBuilder
    {
        private BsonDocument _document=new BsonDocument();

        public MongoGroupBuilder ID(params string[] keys)
        {
            if (!keys.HasData())
            {
                _document.Add("_id", BsonNull.Value);
            }
            else
            {
                if (keys.Length==1)
                {
                
                }
                else
                {
                    _document.Add("_id", new BSON()
                        
                    );
                }                
            }

            return this;

        }
    }

    public static class MongoGroupOpt
    {
        public static KeyValuePair<string, BsonValue> Month(string fieldName)
        {
            return new KeyValuePair<string,BsonValue>("$month", formatFieldName(fieldName));
        }

        public static KeyValuePair<string, BsonValue> Day(string fieldName)
        {
            return new KeyValuePair<string, BsonValue>("$dayOfMonth", formatFieldName(fieldName));
        }

        public static KeyValuePair<string, BsonValue> Year(string fieldName)
        {
            return new KeyValuePair<string, BsonValue>("$year", formatFieldName(fieldName));
        }

        public static KeyValuePair<string, BsonValue> Sum(string[] fieldNames)
        {
            return new KeyValuePair<string, BsonValue>("$sum",new BsonArray(fieldNames));
        }

        public static KeyValuePair<string, BsonValue> Avg(string[] fieldNames)
        {
            return new KeyValuePair<string, BsonValue>("$avg", new BsonArray(fieldNames));
        }

        public static KeyValuePair<string, BsonValue> First(string fieldName)
        {
            return new KeyValuePair<string, BsonValue>("$first", formatFieldName(fieldName));
        }
        public static KeyValuePair<string, BsonValue> Last(string fieldName)
        {
            return new KeyValuePair<string, BsonValue>("$last", formatFieldName(fieldName));
        }
        public static KeyValuePair<string, BsonValue> Max(string[] fieldNames)
        {
            return new KeyValuePair<string, BsonValue>("$max", formatFieldName(fieldNames));
        }

        public static KeyValuePair<string, BsonValue> Push(string fieldName)
        {
            return new KeyValuePair<string, BsonValue>("$push", formatFieldName(fieldName));
        }

        public static MongoDBExp GT(string fieldName, BsonValue value)
        {
            return new MongoDBExp("$gt",new BsonArray()
            {
                formatFieldName(fieldName),
                value
            });
        }

        public static MongoDBExp GTE(string fieldName, BsonValue value)
        {
            return new MongoDBExp("$gte", new BsonArray()
            {
                formatFieldName(fieldName),
                value
            });
        }

        public static MongoDBExp LT(string fieldName, BsonValue value)
        {
            return new MongoDBExp("$lt", new BsonArray()
            {
                formatFieldName(fieldName),
                value
            });
        }

        public static MongoDBExp LTE(string fieldName, BsonValue value)
        {
            return new MongoDBExp("$lte", new BsonArray()
            {
                formatFieldName(fieldName),
                value
            });
        }

        public static MongoDBExp EQ(string fieldName, BsonValue value)
        {
            return new MongoDBExp("$eq", new BsonArray()
            {
                formatFieldName(fieldName),
                value
            });
        }

        public static MongoDBExp NE(string fieldName, BsonValue value)
        {
            return new MongoDBExp("$ne", new BsonArray()
            {
                formatFieldName(fieldName),
                value
            });
        }

        public static MongoDBExp And(params MongoDBExp[] exp)
        {
            return new MongoDBExp("$and",new BsonArray(exp.Select(x=>(BsonValue)x)));
        }

        public static MongoDBExp Or(params MongoDBExp[] exp)
        {
            return new MongoDBExp("$or", new BsonArray(exp.Select(x => (BsonValue)x)));
        }

        public static MongoDBExp Not(params MongoDBExp[] exp)
        {
            return new MongoDBExp("$not", new BsonArray(exp.Select(x => (BsonValue)x)));
        }

        private static BsonValue formatFieldName(string fieldName)
        {
            return fieldName[0] == '$' ? $"${fieldName}" : fieldName;
        }

        private static BsonValue formatFieldName(string[] fieldNames)
        {
            if (fieldNames.Length == 1)
            {
                return formatFieldName(fieldNames[0]);
            }

            return new BsonArray(fieldNames.Select(x=>formatFieldName(x)));
        }
    }

    public class MongoDBExp
    {
        public MongoDBExp() { }

        public MongoDBExp(string key, BsonValue value)
        {
            Key = key;
            Value = value;
        }

        public string Key { set; get; }

        public BsonValue Value { set; get; }

        public static implicit operator BsonValue(MongoDBExp d)
        {
            return new FieldsDocument(d.Key,d.Value);
        }
    }
}
