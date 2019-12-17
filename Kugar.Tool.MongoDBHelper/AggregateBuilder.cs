using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kugar.Tool.MongoDBHelper
{
    [Obsolete("请使用MongoBuilderEx类")]
    public static class Aggregate
    {
        public static AggregateBuilder Match(IMongoQuery query)
        {
            return new AggregateBuilder().Match(query);
        }

        public static AggregateBuilder Project(BsonDocument project) => new AggregateBuilder().Project(project);

        public static AggregateBuilder Group(BsonDocument group) => new AggregateBuilder().Group(group);

        public static AggregateBuilder Skip(int skip) => new AggregateBuilder().Skip(skip);

        public static AggregateBuilder Limit(int limit) => new AggregateBuilder().Limit(limit);

        public static AggregateBuilder Sort(IMongoSortBy sort) => new AggregateBuilder().Sort(sort);

        public static AggregateBuilder Unwind(BsonValue unwind) => new AggregateBuilder().Unwind(unwind);

        public static AggregateBuilder Lookup(string from, string localField, string foreignField, string @as) => new AggregateBuilder().Lookup(from, localField, foreignField, @as);

        public static AggregateBuilder Lookup(string from, BsonDocument let, AggregateBuilder pipeline, string @as) => new AggregateBuilder().Lookup(from, let, pipeline, @as);

        public static AggregateBuilder AddFields(BsonDocument addFields) => new AggregateBuilder().AddFields(addFields);

        public static AggregateBuilder SortByCount(string field) => new AggregateBuilder().SortByCount(field);

        public static AggregateBuilder Count(string outputFieldName) => new AggregateBuilder().Count(outputFieldName);

        public static AggregateBuilder Out(string outputCollectionName) => new AggregateBuilder().Out(outputCollectionName);

        public static AggregateBuilder Other(BsonDocument opt) => new AggregateBuilder().Other(opt);
    }

    public class AggregateBuilder
    {
        private List<BsonDocument> _pipline = null;

        internal AggregateBuilder()
        {
            _pipline = new List<BsonDocument>();
        }

        internal AggregateBuilder(BsonDocument doc) : this()
        {
            _pipline.Add(doc);
        }

        internal AggregateBuilder(IEnumerable<BsonDocument> pipline)
        {
            _pipline = new List<BsonDocument>(pipline);
        }

        /// <summary>
        /// 添加一个查询块
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isExpr">是否为表达式,在使用lookup的pipline的时候,设置为true</param>
        /// <returns></returns>
        public AggregateBuilder Match(IMongoQuery query, bool isExpr = false)
        {

            return Match(query.ToBsonDocumentEx(), isExpr);
        }

        /// <summary>
        /// 添加一个查询块
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isExpr">是否为表达式,在使用lookup的pipline的时候,设置为true,并且会把eq变成 $eq:[x,y] 的方式</param>
        /// <returns></returns>
        public AggregateBuilder Match(BsonDocument query, bool isExpr = false)
        {
            BsonDocument q = null;

            if (query != null)
            {



                if (isExpr)
                {
                    var doc = new BsonArray();

                    foreach (var item in query)
                    {
                        if (item.Name.StartsWith("$"))
                        {
                            doc.Add(new BsonDocument()
                            {
                                [item.Name] = item.Value
                            });
                        }
                        else if (!item.Name.StartsWith('$'))
                        {
                            if (item.Value.IsBsonDocument)
                            {
                                var valueDoc = item.Value.AsBsonDocument;

                                foreach (var pair in valueDoc)
                                {
                                    doc.Add(new BsonDocument()
                                    {
                                        [pair.Name] = new BsonArray()
                                        {
                                            "$" + item.Name,
                                            pair.Value
                                        }
                                    });
                                }
                            }
                            else
                            {
                                doc.Add(new BsonDocument()
                                {
                                    ["$eq"] = new BsonArray()
                                    {
                                        "$" +item.Name, item.Value
                                    }
                                });
                            }
                        }
                    }

                    q = new BsonDocument()
                    {
                        ["$match"] = new BsonDocument()
                        {
                            ["$expr"] = new BsonDocument()
                            {

                                ["$and"] = doc
                            }
                        }
                    };


                }
                else
                {
                    q = new BsonDocument()
                    {
                        ["$match"] = query
                    };
                }

                _pipline.Add(q);
            }

            return this;
        }

        public AggregateBuilder Project(BsonDocument project)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$project"] = project
            });

            return this;
        }

        public AggregateBuilder Project(ProjectBuilder projectBuilder)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$project"] = (BsonDocument)projectBuilder
            });

            return this;
        }

        public AggregateBuilder Group(BsonDocument group)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$group"] = group
            });

            return this;
        }

        /// <summary>
        /// $group 关键字,
        /// </summary>
        /// <param name="idField">group的_id字段,可以直接传单个string,也可以传入document作为多key,如果需要空,则传null或者空字符串</param>
        /// <param name="groupData"></param>
        /// <returns></returns>
        public AggregateBuilder Group(BsonValue idField, BsonDocument groupData)
        {
            var bson = new BsonDocument();

            if (idField == null || idField == BsonNull.Value || (idField.IsString && string.IsNullOrEmpty(idField.AsString)))
            {
                bson.Add("_id", BsonNull.Value);
            }
            else
            {
                bson.Add("_id", idField);
            }

            foreach (var item in groupData)
            {
                bson.Add(item);
            }

            _pipline.Add(new BsonDocument()
            {
                ["$group"] = bson
            });

            return this;
        }


        public AggregateBuilder Skip(int skip)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$skip"] = skip
            });

            return this;
        }

        public AggregateBuilder Limit(int limit)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$limit"] = limit
            });

            return this;
        }

        public AggregateBuilder Sort(IMongoSortBy sort)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$sort"] = sort.ToBsonDocumentEx()
            });

            return this;
        }

        public AggregateBuilder Unwind(BsonValue unwind)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$unwind"] = unwind
            });

            return this;
        }

        public AggregateBuilder Lookup(string from, string localField, string foreignField, string @as)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$lookup"] = new BsonDocument()
                {
                    ["from"] = from,
                    ["localField"] = localField,
                    ["foreignField"] = foreignField,
                    ["as"] = @as
                }
            });

            return this;
        }

        public AggregateBuilder Lookup(string from, BsonDocument let, AggregateBuilder pipeline, string @as)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$lookup"] = new BsonDocument()
                {
                    ["from"] = from,
                    ["let"] = let,
                    ["pipeline"] = new BsonArray(pipeline._pipline),
                    ["as"] = @as
                }
            });

            return this;
        }

        public AggregateBuilder AddFields(BsonDocument addFields)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$addFields"] = addFields
            });

            return this;
        }

#if NETCOREAPP
        public AggregateBuilder AddFields(params (string newFieldName, string fieldValueFrom)[] fields)
        {
            AddFields(
                new BsonDocument(fields.Select(x => new BsonElement(x.newFieldName,
                    x.fieldValueFrom.StartsWith('$') ? x.fieldValueFrom : "$" + x.fieldValueFrom)))
            );

            return this;
        }
#endif


        /// <summary>
        /// 使用新的对象替换当前数据行 ($replaceRoot 操作)
        /// </summary>
        /// <param name="newRootFieldName">新的对象的字段名</param>
        /// <returns></returns>
        public AggregateBuilder ReplaceRoot(string newRootFieldName)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$replaceRoot"] = new BsonDocument()
                {
                    ["newRoot"] =newRootFieldName.StartsWith('$')?newRootFieldName:"$"+ newRootFieldName
                }
            });

            return this;
        }

        public AggregateBuilder SortByCount(string field)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$sortByCount"] = field[0] == '$' ? field : ('$' + field)
            });

            return this;
        }

        public AggregateBuilder Count(string outputFieldName)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$count"] = outputFieldName
            });

            return this;
        }

        public AggregateBuilder Unset(params string[] fieldNames)
        {
            if (fieldNames.HasData())
            {
                _pipline.Add(new BsonDocument()
                {
                    ["$unset"] = new BsonArray(fieldNames)
                });
            }

            return this;
        }

        public AggregateBuilder Out(string outputCollectionName)
        {
            _pipline.Add(new BsonDocument()
            {
                ["$out"] = outputCollectionName
            });

            return this;
        }

        /// <summary>
        /// 其他操作,如果类中定义的操作,未包含所需,,则直接使用该函数
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public AggregateBuilder Other(BsonDocument opt)
        {
            _pipline.Add(opt);

            return this;
        }

        /// <summary>
        /// 当前查询,复制一份
        /// </summary>
        /// <returns></returns>
        public AggregateBuilder Copy()
        {
            return new AggregateBuilder(_pipline);
        }

        /// <summary>
        /// 原builder
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static implicit operator BsonDocument[](AggregateBuilder source)
        {
            return source._pipline.ToArray();
        }

        public override string ToString()
        {
            return _pipline.ToBsonArray().ToStringEx();
        }

        //public static implicit operator AggregateArgs (AggregateBuilder source)
        //{
        //    var args = new AggregateArgs();

        //    args.AllowDiskUse = true;
        //    args.Pipeline = source._pipline;

        //    return args;
        //}
    }

    public static class Project
    {
        public static ProjectBuilder Include(string fieldName)
        {
            return new ProjectBuilder().Include(fieldName);
        }

        public static ProjectBuilder Include(params string[] fieldNames)
        {
            return new ProjectBuilder().Include(fieldNames);
        }

        public static ProjectBuilder Exclude(string fieldName)
        {
            return new ProjectBuilder().Exclude(fieldName);
        }

        public static ProjectBuilder Exclude(params string[] fieldNames)
        {
            return new ProjectBuilder().Exclude(fieldNames);
        }

        /// <summary>
        /// 是否排除_id字段
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static ProjectBuilder ExcludeID(bool isExclude = true)
        {
            return new ProjectBuilder().ExcludeID(isExclude);
        }

        public static ProjectBuilder Expression(string fieldName, BsonDocument expression)
        {
            return new ProjectBuilder().Expression(fieldName, expression);
        }

        public static ProjectBuilder NewArray(string fieldName, params string[] sourceFields)
        {
            return new ProjectBuilder().NewArray(fieldName, sourceFields);
        }

        public static ProjectBuilder Cond(string fieldName, IMongoQuery @if, BsonValue @then, BsonValue @else)
        {
            return new ProjectBuilder().Cond(fieldName, @if, @then, @else);
        }
    }

    public class ProjectBuilder
    {
        private BsonDocument _lst = new BsonDocument();

        public ProjectBuilder Include(string fieldName)
        {
            _lst.Add(new BsonElement(fieldName, true));

            return this;
        }

        public ProjectBuilder Include(params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                _lst.Add(new BsonElement(fieldName, true));
            }

            return this;
        }

        public ProjectBuilder Exclude(string fieldName)
        {
            _lst.Add(new BsonElement(fieldName, false));

            return this;
        }

        public ProjectBuilder Exclude(params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                _lst.Add(new BsonElement(fieldName, false));
            }

            return this;
        }

        /// <summary>
        /// 是否排除_id字段
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public ProjectBuilder ExcludeID(bool isExclude = true)
        {
            _lst.Add(new BsonElement("_id", false));

            return this;
        }

        public ProjectBuilder Expression(string fieldName, BsonDocument expression)
        {
            _lst.Add(new BsonElement(fieldName, expression));

            return this;
        }

        public ProjectBuilder NewArray(string fieldName, params string[] sourceFields)
        {
            _lst.Add(new BsonElement(fieldName, new BsonArray(sourceFields)));
            return this;
        }

        public ProjectBuilder Cond(string fieldName, IMongoQuery @if, BsonValue @then, BsonValue @else)
        {
            _lst.Add(new BsonElement(fieldName, new BsonDocument()
            {
                ["if"] = @if.ToBsonDocument(),
                ["then"] = @then,
                ["else"] = @else
            }));

            return this;
        }

        public static implicit operator BsonDocument(ProjectBuilder source)
        {
            return source._lst;
        }
    }

    public class GroupBuilder
    {
        private BsonDocument _doc = new BsonDocument();

        public GroupBuilder _id(string fieldName)
        {
            _doc["_id"] = fieldName;

            return this;
        }

        public GroupBuilder _id(params BsonElement[] elements)
        {
            _doc["_id"] = new BsonDocument(elements);

            return this;
        }

        public GroupBuilder Sum(string outFieldName, BsonValue sumValueOrField)
        {
            _doc[outFieldName] = new BsonDocument()
            {
                ["$sum"] = sumValueOrField
            };

            return this;
        }

        public GroupBuilder Sum(string outFieldName, params BsonElement[] exp)
        {
            _doc[outFieldName] = new BsonDocument()
            {
                ["$sum"] = new BsonDocument(exp)
            };

            return this;
        }

        public GroupBuilder AddField(string outFieldName, params BsonElement[] expressions)
        {
            _doc[outFieldName] = new BsonDocument(expressions);
            return this;
        }
    }

    public class CalcOperatorBuilder
    {

        public BsonElement Multiply(params BsonValue[] fieldNames)
        {
            return new BsonElement("$multiply", new BsonArray(fieldNames));
        }

        public BsonElement Pow(params BsonValue[] fieldNames)
        {
            return new BsonElement("$pow", new BsonArray(fieldNames));
        }

        public BsonElement Abs(BsonValue fieldNameOrValue)
        {
            return new BsonElement("$abs", fieldNameOrValue);
        }

        public BsonElement Avg(BsonValue fieldNameOrValue)
        {
            return new BsonElement("$abs", fieldNameOrValue);
        }

        public BsonElement Avg(params BsonElement[] fieldNameOrValue)
        {
            return new BsonElement("$abs", new BsonArray(fieldNameOrValue));
        }

        public BsonElement First(string fieldName)
        {
            return new BsonElement("$first", fieldName);
        }

        public BsonElement Last(string fieldName)
        {
            return new BsonElement("$last", fieldName);
        }
    }
}
