using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Kugar.Tool.MongoDBHelper
{
    //同步接口
    public partial class MongoCollectionHelper : IMongoDBCollectionHelper
    {
        private string _collectionName = "";
        protected IMongoDBClientCollectionExecutor _collection = null;
        //protected Lazy<MongoCollection<BsonDocument>> _oldCollection = null;
        private Lazy<IMongoDBCollectionHelper> _moduleIDCollection = null;
        //private IMongoDatabase _context = null;
        private MongoDatabase _oldcontext = null;
        //private bool _isJournal;
        //private int _wlevel = 1;
        private IMongoDBContext _helper = null;

        private Version _serverVersion = null;
        //private MongoClient _client = null;

        public MongoCollectionHelper(string name, IMongoDBClientCollectionExecutor orgCollection, IMongoDBContext helper, Version serverVersion = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            _serverVersion = serverVersion;

            _collectionName = name;

            //_context = context;

            //_isJournal = isJournal;
            //_wlevel = wLevel;

            _collection = orgCollection; // _context.GetCollection<BsonDocument>(_collectionName, new MongoCollectionSettings() { WriteConcern = new WriteConcern() });

            //_oldcontext = oldContext;
            //_oldCollection =new Lazy<MongoCollection<BsonDocument>>(getOldContextCollection); //_oldcontext.GetCollection<BsonDocument>(_collectionName, new WriteConcern());

            _moduleIDCollection = new Lazy<IMongoDBCollectionHelper>(() => _helper.GetCollection("sys_ModuleID"));


            _helper = helper;
        }

        /// <summary>
        /// 替换一个符合查询的文档
        /// </summary>
        /// <param name="query"></param>
        /// <param name="newDocument"></param>                   
        /// <param name="isupset"></param>
        /// <returns></returns>
        public ResultReturn ReplaceOne(IMongoQuery query, BsonDocument newDocument, bool isupset = false)
        {
            var ret = _collection.ReplaceOne(query.ToFilterDefinition(),
                newDocument, new UpdateOptions()
                {
                    IsUpsert = isupset,
                });



            return toResultReturn(ret);
        }

        public ResultReturn<BulkWriteResult<BsonDocument>> BulkWrite(IEnumerable<WriteModel<BsonDocument>> opts, bool isordered = false)
        {
            try
            {
                var ret = _collection.BulkWrite(opts, new BulkWriteOptions() { IsOrdered = isordered });
                return new SuccessResultReturn<BulkWriteResult<BsonDocument>>(ret);
            }
            catch (Exception e)
            {
                return new FailResultReturn<BulkWriteResult<BsonDocument>>(e);
            }
        }

        public ResultReturn<BulkWriteResult<BsonDocument>> BulkWrite(BulkWriteBuilder builder, bool isordered = false)
        {
            return BulkWrite(builder.Operations, isordered);
        }

        private ResultReturn toResultReturn(ReplaceOneResult ret)
        {
            if (ret.IsAcknowledged)
            {
                if (ret.IsModifiedCountAvailable && ret.ModifiedCount > 0)
                {
                    return SuccessResultReturn.Default;
                }
                else if (ret.UpsertedId.IsObjectId)
                {
                    return new SuccessResultReturn(ret.UpsertedId.AsObjectId);
                }

                return new FailResultReturn("替换失败");
            }
            else
            {
                return new SuccessResultReturn();
            }

        }


        #region Count

        public ResultReturn<int> Count(IMongoQuery query)
        {
            try
            {
                var c = _collection.Count(new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocument()));
                //var c = _oldCollection.Value.Count(query);

                return new ResultReturn<int>(true, (int)c);
            }
            catch (Exception ex)
            {
                return new FailResultReturn<int>(ex);
            }
        }

        public int GetQueryPageCount(IMongoQuery query, int pageSize)
        {
            var count = Count(query).ReturnData;

            if (count <= 0)
            {
                return 0;
            }
            else
            {
                if (count % pageSize > 0)
                {
                    return (count / pageSize) + 1;
                }
                else
                {
                    return count / pageSize;
                }
            }
        }

        #endregion

        #region  FindOneWithSigleField

        public BsonValue FindOneWithSigleField(IMongoQuery query, string fieldName, IMongoSortBy order = null, BsonValue defaultValue = null)
        {
            try
            {
                var doc = FindOne(query, order, Fields.Include(fieldName));

                if (doc == null)
                {
                    return defaultValue ?? BsonNull.Value;
                }
                else
                {
                    var value = doc.GetValue(fieldName, BsonNull.Value);

                    return value;
                }
            }
            catch (Exception e)
            {
                return defaultValue ?? BsonNull.Value;
            }
        }

        public TValue FindOneWithSigleField<TValue>(IMongoQuery query, string fieldName, IMongoSortBy order = null, TValue defaultValue = default(TValue))
        {
            var type = typeof(TValue);

            try
            {
                var doc = FindOne(query, order, Fields.Include(fieldName));

                if (doc == null)
                {
                    return defaultValue;
                }
                else
                {
                    var value = doc.GetValue(fieldName, BsonNull.Value);

                    TValue tmpValue;

                    if (value.IsBsonDocument)
                    {
                        if (type.IsPrimitive)
                        {
                            return defaultValue;

                            //return new FailResultReturn<T>("类型错误,读取的数据是Bsondocument,T是元类型,无法转换");
                        }
                        else
                        {
                            tmpValue = BsonSerializer.Deserialize<TValue>(value.AsBsonDocument);
                        }
                    }
                    else if (value.IsBsonArray)
                    {
                        return defaultValue;

                    }
                    else
                    {
                        tmpValue = (TValue)value.ToPrimitiveValue();
                    }


                    return tmpValue;
                }
            }
            catch (Exception e)
            {
                return defaultValue;
                //return new FailResultReturn<T>(e);
            }
        }


        #endregion



        #region FindWithPaged
        public VM_PagedList<BsonDocument> FindWithPaged(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int pageIndex = 1, int pageSize = 20, Collation collation = null)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            var count = Count(query).ReturnData;

            if (count <= 0)
            {
                return new VM_PagedList<BsonDocument>(new BsonDocument[0], pageIndex: pageIndex, pageSize: pageSize, totalCount: 0);
            }
            else
            {
                BsonDocument[] data = null;

                if (pageIndex <= 0)
                {
                    pageIndex = 1;
                }

                if (pageIndex == 1)
                {
                    var ret = Find(query, order, fields, 0, pageSize, collation: collation);

                    data = ret;
                }
                else
                {
                    var skip = pageSize * (pageIndex - 1);

                    var ret = Find(query, order, fields, skip, pageSize, collation: collation);
                    data = ret;
                }

                return new VM_PagedList<BsonDocument>(data, pageIndex, pageSize, count);
            }

        }


        public void FindWithPagedAndCallback(IMongoQuery query, Action<BsonDocument[]> callback, IMongoSortBy order = null, IMongoFields fields = null, int pageSize = 20, Collation collation = null)
        {
            var pageCount = GetQueryPageCount(query, pageSize);

            if (pageCount <= 0)
            {
                return;
            }

            for (int i = 1; i <= pageCount; i++)
            {
                var data = FindWithPaged(query, order, fields, i, pageSize, collation: collation);

                if (!data.HasData())
                {
                    break;
                }
                else
                {
                    callback?.Invoke(data.Data.ToArrayEx());
                }

            }
        }

        #endregion

        #region Find
        private static BsonDocument[] _emptyBsonDocument = new BsonDocument[0];
        public BsonDocument[] Find(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = -1, Collation collation = null)
        {
            try
            {
                var opt = new FindOptions<BsonDocument, BsonDocument>();

                if (take > 0)
                {
                    opt.BatchSize = take;
                }

                if (order != null)
                {
                    opt.Sort = order.ToSortDefinition();
                }

                if (fields != null)
                {
                    opt.Projection = fields.ToProjectionDefinition();
                }

                if (skip > 0)
                {
                    opt.Skip = skip;
                }

                if (take > 0)
                {
                    opt.Limit = take;
                }

                if (collation != null)
                {
                    opt.Collation = collation;
                }

                var temp = _collection.FindSync(query.ToFilterDefinition(), opt);

                //if (order != null)
                //{
                //    temp = temp.Sort(order.ToSortDefinition());
                //}

                //if (fields != null)
                //{
                //    temp = temp.Project(fields.ToProjectionDefinition());
                //}

                //if (skip > 0)
                //{
                //    temp = temp.Skip(skip);
                //}

                //if (take > 0)
                //{
                //    temp = temp.Limit(take);
                //}

                //temp.SetFlags(QueryFlags.Exhaust);

                //temp = temp.SetFlags(QueryFlags.TailableCursor);

                var data = temp.ToList();

                return data.HasData() ? data.ToArray() : _emptyBsonDocument;

                //return new ResultReturn<BsonDocument[]>(true, data);
            }
            catch (Exception ex)
            {
                throw;
                //return new FailResultReturn<BsonDocument[]>(ex);
            }
        }

        #endregion

        #region FindOne

        public BsonDocument FindOne(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, Collation collation = null)
        {
            try
            {
                var data = Find(
                    query,
                    order,
                    fields
                    , 0, 1, collation: collation);

                return data.FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ObjectId FindOneID(IMongoQuery query, IMongoSortBy order = null)
        {
            var data = FindOne(query, order, Fields.Include("_id"));

            if (data.HasData())
            {
                return data.GetObjectId("_id");
            }
            else
            {
                return ObjectId.Empty;
            }
        }

        #endregion

        #region Update

        public ResultReturn Update(IMongoQuery query, IMongoUpdate updater, UpdateFlags flags = UpdateFlags.None)
        {
            if (updater == null)
            {
                throw new ArgumentNullException("updater");
            }

            try
            {
                var concern = WriteConcern.WMajority;

                //if (_isJournal)
                //{
                //    concern.
                //    concern.Journal = true;
                //    concern.W = 1;
                //}

                UpdateResult result = null;

                if (flags == UpdateFlags.Multi)
                {
                    result = _collection.UpdateMany(query.ToBsonDocument(), updater.ToBsonDocument(), new UpdateOptions() { IsUpsert = false });
                }
                else
                {
                    result = _collection.UpdateOne(query.ToBsonDocument(), updater.ToBsonDocument(), new UpdateOptions() { IsUpsert = flags == UpdateFlags.Upsert });
                }


                if (!result.IsAcknowledged || result.ModifiedCount > 0 || result.MatchedCount > 0)
                {
                    return new SuccessResultReturn(result.IsAcknowledged ? result.ModifiedCount : 0);
                }
                else
                {
                    return new FailResultReturn("更新失败");
                }

                //var result = _oldCollection.Value.Update(query, updater, flags, concern);

                //if (!result.HasLastErrorMessage)
                //{
                //    return new SuccessResultReturn(result.DocumentsAffected);
                //}
                //else
                //{
                //    return new FailResultReturn(result.LastErrorMessage);
                //}
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, null, error: ex);
            }
        }

        public ResultReturn UpdateByID(ObjectId id, IMongoUpdate updater, UpdateFlags flags = UpdateFlags.None)
        {
            if (updater == null)
            {
                throw new ArgumentNullException("updater");
            }

            if (id.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            try
            {
                return Update(Query.EQ("_id", id), updater, flags);
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, null, error: ex);
            }
        }


        //public ResultReturn  Update<TDocument>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateFlags flags = UpdateFlags.None)
        //{

        //}

        #endregion

        #region FindAndModify

        public ResultReturn<BsonDocument> FindAndModify(IMongoQuery query, IMongoUpdate update, IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false)
        {
            if (update == null)
            {
                throw new ArgumentNullException("update");
            }

            sortBy = sortBy ?? SortBy.Null;


            try
            {
                var result = _collection.FindOneAndUpdate(query.ToBsonDocument(),
                    update.ToBsonDocument(),
                    new FindOneAndUpdateOptions<BsonDocument>()
                    {
                        IsUpsert = upsert,
                        ReturnDocument = returnNew ? ReturnDocument.After : ReturnDocument.Before,
                        Sort = (sortBy ?? SortBy.Null).ToSortDefinition(),
                        Projection = (fields ?? Fields.Null).ToProjectionDefinition(),
                    }
                );

                return new SuccessResultReturn<BsonDocument>(result);

                //var result = _oldCollection.Value.FindAndModify(args);

                if (result.HasData())
                {
                    return new ResultReturn<BsonDocument>(true, result);
                }
                else
                {
                    return new ResultReturn<BsonDocument>(true, null);
                }
            }
            catch (Exception ex)
            {
                return new FailResultReturn<BsonDocument>(ex);
            }
        }

        #endregion


        #region FindAll

        public BsonDocument[] FindAll(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null)
        {
            //var q = _oldCollection.Value.FindAll();

            var opt = new FindOptions<BsonDocument, BsonDocument>();

            if (sortBy != null)
            {
                opt.Sort = sortBy.ToSortDefinition();
            }

            if (fields != null)
            {
                opt.Projection = fields.ToProjectionDefinition();
            }

            if (collation != null)
            {
                opt.Collation = collation;
            }

            var q = _collection.FindSync(Query.Empty.ToFilterDefinition(), opt);

            //if (sortBy != null)
            //{
            //    q = q.Sort(sortBy.ToSortDefinition());//.SetSortOrder(sortBy);
            //}

            //if (fields != null)
            //{
            //    q = q.Project(fields.ToProjectionDefinition());
            //   // q = q.SetFields(fields);
            //}

            return q.ToList().ToArrayEx();
        }

        #endregion

        #region Aggregate

        public ResultReturn<BsonDocument[]> Aggregate(params BsonDocument[] pipeline)
        {
            if (!pipeline.HasData())
            {
                return new FailResultReturn<BsonDocument[]>(new ArgumentNullException("pipeline"));
            }

            try
            {

                var pipelineDef = new PipelineStagePipelineDefinition<BsonDocument, BsonDocument>(pipeline.Select(x => new BsonDocumentPipelineStageDefinition<BsonDocument, BsonDocument>(x)));

                var opt = new AggregateOptions();
                opt.AllowDiskUse = true;

                //if (_serverVersion.Major >= 3 && _serverVersion.Minor >= 5)
                {
                    opt.UseCursor = true;
                }

                var r = _collection.Aggregate(pipelineDef, opt);

                return new ResultReturn<BsonDocument[]>(true, r.ToArrayEx());


            }
            catch (Exception ex)
            {
                return new FailResultReturn<BsonDocument[]>(ex);
            }
        }

        public ResultReturn<BsonDocument[]> Aggregate(AggregateArgs args)
        {
            if (!args.Pipeline.HasData())
            {
                return new FailResultReturn<BsonDocument[]>(new ArgumentNullException("pipeline"));
            }

            try
            {
                if (_serverVersion.Major >= 3 && _serverVersion.Minor >= 5)
                {
                    args.OutputMode = AggregateOutputMode.Cursor;
                }

                //var result = _oldCollection.Aggregate(args);

                var pipeline = new PipelineStagePipelineDefinition<BsonDocument, BsonDocument>(args.Pipeline.Select(x => new BsonDocumentPipelineStageDefinition<BsonDocument, BsonDocument>(x)));

                var opt = new AggregateOptions();
                opt.AllowDiskUse = args.AllowDiskUse;
                opt.BatchSize = args.BatchSize;
                opt.UseCursor = args.OutputMode == AggregateOutputMode.Cursor;

                var r = _collection.Aggregate(pipeline, opt);

                return new ResultReturn<BsonDocument[]>(true, r.ToList().ToArrayEx());


            }
            catch (Exception ex)
            {
                return new FailResultReturn<BsonDocument[]>(ex);
            }
        }

        public BsonValue Sum(IMongoQuery query, string sumFieldName, BsonValue defaultValue)
        {
            var result = this.Aggregate(MongoBuilderEx.Aggregate.Match(query).Group(new BsonDocument()
            {
                ["_id"] = BsonNull.Value,
                ["value"] = new BsonDocument()
                {
                    ["$sum"] = "$" + sumFieldName
                }
            }));

            if (result.IsSuccess)
            {
                return defaultValue;
            }
            else
            {
                return result.ReturnData?.FirstOrDefault()?.GetValue("value") ?? defaultValue;
            }
        }

        #endregion

        #region Insert
        public ResultReturn Insert(BsonDocument document)
        {
            //var concern = WriteConcern.WMajority;

            try
            {
                _collection.InsertOne(document);

                //var result = _oldCollection.Value.Insert(document, concern);

                return SuccessResultReturn.Default;
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, error: ex);
            }
        }

        public ResultReturn InsertBatch(IEnumerable<BsonDocument> document, bool isOrdered = false)
        {
            try
            {
                _collection.InsertMany(document, new InsertManyOptions() { IsOrdered = isOrdered });

                //return new ResultReturn(result.Any(x => !x.HasLastErrorMessage), null, message: result.HasData() ? result.FirstOrDefault().LastErrorMessage : "");
                return SuccessResultReturn.Default;
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, error: ex);
            }
        }

        #endregion

        #region Delete

        public ResultReturn Delete(IMongoQuery query, RemoveFlags flags = RemoveFlags.None)
        {
            try
            {
                DeleteResult result = null;

                if (flags == RemoveFlags.None)
                {
                    result = _collection.DeleteMany(query.ToFilterDefinition(), new DeleteOptions());
                }
                else
                {
                    result = _collection.DeleteOne(query.ToFilterDefinition(), new DeleteOptions());
                }

                return new SuccessResultReturn(result.DeletedCount);

                //var result = _oldCollection.Remove(query, flags, new WriteConcern());

                //return new ResultReturn(!result.HasLastErrorMessage, null, message: result.LastErrorMessage);
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, error: ex);
            }


        }

        #endregion

        #region FindID

        public ObjectId FindID(IMongoQuery query, IMongoSortBy orderBy = null)
        {
            var s = FindOne(query, orderBy, Fields.Include("_id"));

            if (s != null)
            {
                return s.GetValue("_id").AsObjectId;
            }
            else
                return ObjectId.Empty;

        }

        public ObjectId[] FindIDs(IMongoQuery query, IMongoSortBy orderBy = null)
        {
            var s = Find(query, orderBy, Fields.Include("_id"));

            if (s != null)
            {
                if (s.HasData())
                {
                    return s.Select(x => x.GetValue("_id").AsObjectId).ToArray();
                }
                else
                {
                    return new ObjectId[0];
                }
            }
            else
                return new ObjectId[0];
        }

        #endregion

        #region FindByID

        public BsonDocument FindByID(ObjectId id, IMongoFields fields = null)
        {
            return FindOne(Query.EQ("_id", id), fields: fields);
        }

        public BsonDocument[] FindByIDs(ObjectId[] ids, IMongoFields fields = null)
        {
            return Find(Query.In("_id", ids.ToBsonArray()), fields: fields);
        }

        #endregion


        #region Exists

        public bool Exists(IMongoQuery query)
        {
            try
            {
                return FindOne(query, fields: Fields.Include("_id")).HasData();

                //var ret = _oldCollection.Value.Find(query).SetFields("_id").SetLimit(1).Any();

                //return ret;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion


        public BsonDocument[] Distinct(string field, IMongoQuery query)
        {
            return _collection.Distinct<BsonDocument>(field,
                new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx())).ToList().ToArrayEx();
        }


        public IMongoDBClientCollectionExecutor SourceCollection { get { return _collection; } }

        public IMongoDBContext Context => _helper;

        //public IMongoCollection<BsonDocument> Collection
        //{
        //    get { return _collection; }
        //}

        public int NewSequenceID()
        {
            var args = new FindAndModifyArgs();
            args.Query = Query.EQ("ModuleID", _collectionName);
            args.SortBy = null;
            args.Update = MongoDB.Driver.Builders.Update.Inc("CurrentValue", 1);
            args.Upsert = true;
            args.VersionReturned = FindAndModifyDocumentVersion.Modified;

            var ret = _moduleIDCollection.Value.FindAndModify(Query.EQ("ModuleID", _collectionName),
                MongoDB.Driver.Builders.Update.Inc("CurrentValue", 1), upsert: true, returnNew: true
                );

            return ret.ReturnData.GetValue("CurrentValue").AsInt32;
        }

        //private IMongoCollection<BsonDocument> getNewContextCollection()
        //{
        //    return _context.GetCollection<BsonDocument>(_collectionName, new MongoCollectionSettings() { WriteConcern = new WriteConcern(journal:_isJournal,w: _wlevel) });
        //}

        //private MongoCollection<BsonDocument> getOldContextCollection()
        //{
        //    return _oldcontext.GetCollection<BsonDocument>(_collectionName, new WriteConcern(journal: _isJournal, w: _wlevel));
        //}

    }

    //异步接口
    public partial class MongoCollectionHelper 
    {
        /// <summary>
        /// 替换一个符合指定查询的文档
        /// </summary>
        /// <param name="query"></param>
        /// <param name="newDocument"></param>
        /// <param name="isupset"></param>
        /// <returns></returns>
        public async Task<ResultReturn> ReplaceOneAsync(IMongoQuery query, BsonDocument newDocument,
            bool isupset = false)
        {
            var ret = await _collection.ReplaceOneAsync(query.ToFilterDefinition(),
                newDocument, new UpdateOptions()
                {
                    IsUpsert = isupset,
                });


            return toResultReturn(ret);
        }

        public async Task<ResultReturn<BulkWriteResult<BsonDocument>>> BulkWriteAsync(
            IEnumerable<WriteModel<BsonDocument>> opts, bool isordered = false)
        {
            try
            {
                var ret = await _collection.BulkWriteAsync(opts, new BulkWriteOptions() { IsOrdered = isordered });
                return new SuccessResultReturn<BulkWriteResult<BsonDocument>>(ret);
            }
            catch (Exception e)
            {
                return new FailResultReturn<BulkWriteResult<BsonDocument>>(e);
            }
        }

        public async Task<ResultReturn<BulkWriteResult<BsonDocument>>> BulkWriteAsync(BulkWriteBuilder builder,
            bool isordered = false)
        {
            return await BulkWriteAsync(builder.Operations, isordered);
        }

        public async Task<int> CountAsync(IMongoQuery query)
        {
            try
            {
                var c = await _collection.CountAsync(query.ToFilterDefinition());

                return (int)c;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<VM_PagedList<BsonDocument>> FindWithPagedAsync(IMongoQuery query, IMongoSortBy order = null,
            IMongoFields fields = null, int pageIndex = 1, int pageSize = 20, Collation collation = null)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            var count = await CountAsync(query);

            if (count <= 0)
            {
                return new VM_PagedList<BsonDocument>(new BsonDocument[0], pageIndex: pageIndex, pageSize: pageSize,
                    totalCount: 0);
            }
            else
            {
                BsonDocument[] data = null;

                if (pageIndex <= 0)
                {
                    pageIndex = 1;
                }

                if (pageIndex == 1)
                {
                    var ret = await FindAsync(query, order, fields, 0, pageSize, collation: collation);

                    data = ret;
                }
                else
                {
                    var skip = pageSize * (pageIndex - 1);
                    var ret = await FindAsync(query, order, fields, skip, pageSize, collation: collation);
                    data = ret;
                }

                return new VM_PagedList<BsonDocument>(data, pageIndex, pageSize, count);
            }
        }

        public async Task<BsonDocument[]> FindAsync(IMongoQuery query, IMongoSortBy order = null,
            IMongoFields fields = null, int skip = 0, int take = -1, Collation collation = null)
        {
            try
            {
                var opt = new FindOptions<BsonDocument, BsonDocument>();

                if (take > 0)
                {
                    opt.BatchSize = take;
                }

                if (order != null)
                {
                    opt.Sort = order.ToSortDefinition();
                }

                if (fields != null)
                {
                    opt.Projection = fields.ToProjectionDefinition();
                }

                if (skip > 0)
                {
                    opt.Skip = skip;
                }

                if (take > 0)
                {
                    opt.Limit = take;
                }

                if (opt.Collation != null)
                {
                    opt.Collation = collation;
                }

                var temp = await _collection.FindAsync(query.ToFilterDefinition(), opt);

                var data = temp.ToArrayEx();

                return data.HasData() ? data.ToArray() : _emptyBsonDocument;
            }
            catch (Exception ex)
            {
                throw;
                //return new FailResultReturn<BsonDocument[]>(ex);
            }
        }

        public async Task<BsonDocument> FindOneAsync(IMongoQuery query, IMongoSortBy order = null,
            IMongoFields fields = null, Collation collation = null)
        {
            try
            {
                var data = await FindAsync(
                    query,
                    order,
                    fields
                    , 0, 1, collation: collation);

                return data.FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObjectId> FindOneIDAsync(IMongoQuery query, IMongoSortBy order = null)
        {
            var data = await FindOneAsync(query, order, Fields.Include("_id"));

            if (data.HasData())
            {
                return data.GetObjectId("_id");
            }
            else
            {
                return ObjectId.Empty;
            }
        }

        public async Task<ResultReturn> UpdateAsync(IMongoQuery query, IMongoUpdate updater,
            UpdateFlags flags = UpdateFlags.None)
        {
            if (updater == null)
            {
                throw new ArgumentNullException("updater");
            }

            try
            {
                //var concern = WriteConcern.WMajority;

                //if (_isJournal)
                //{
                //    concern.
                //    concern.Journal = true;
                //    concern.W = 1;
                //}
                UpdateResult result = null;

                if (flags == UpdateFlags.Multi)
                {
                    result = await _collection.UpdateManyAsync(query.ToBsonDocument(), updater.ToBsonDocument(),
                        new UpdateOptions() { IsUpsert = false });
                }
                else
                {
                    result = await _collection.UpdateOneAsync(query.ToBsonDocument(), updater.ToBsonDocument(),
                        new UpdateOptions() { IsUpsert = flags == UpdateFlags.Upsert });
                }

                if (!result.IsAcknowledged || result.ModifiedCount > 0 || result.MatchedCount > 0)
                {
                    return new SuccessResultReturn(result.IsAcknowledged ? result.ModifiedCount : 0);
                }
                else
                {
                    return new FailResultReturn("更新失败");
                }
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, null, error: ex);
            }
        }

        public async Task<ResultReturn> UpdateByIDAsync(ObjectId id, IMongoUpdate updater,
            UpdateFlags flags = UpdateFlags.None)
        {
            if (updater == null)
            {
                throw new ArgumentNullException("updater");
            }

            if (id.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            try
            {
                return await UpdateAsync(Query.EQ("_id", id), updater, flags);
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, null, error: ex);
            }
        }

        public async Task<ResultReturn<BsonDocument>> FindOneAndModifyAsync(IMongoQuery query, IMongoUpdate update,
            IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false)
        {
            if (update == null)
            {
                throw new ArgumentNullException("update");
            }

            //sortBy = sortBy ?? SortBy.Null;

            var opt = new FindOneAndUpdateOptions<BsonDocument>();

            opt.IsUpsert = upsert;
            opt.Sort = (sortBy ?? SortBy.Null).ToSortDefinition();
            opt.Projection = (fields ?? Fields.Null).ToProjectionDefinition();
            opt.ReturnDocument = returnNew ? ReturnDocument.After : ReturnDocument.Before;

            try
            {
                var result =
                    await _collection.FindOneAndUpdateAsync(query.ToBsonDocument(), update.ToBsonDocument(), opt);

                return new SuccessResultReturn<BsonDocument>(result);

                if (result.HasData())
                {
                    return new SuccessResultReturn<BsonDocument>(result);
                }
                else
                {
                    return new FailResultReturn<BsonDocument>("更新失败");
                }
            }
            catch (Exception ex)
            {
                return new FailResultReturn<BsonDocument>(ex);
            }
        }

        public async Task<BsonDocument[]> FindAllAsync(IMongoSortBy sortBy = null, IMongoFields fields = null,
            Collation collation = null)
        {
            var opt = new FindOptions<BsonDocument, BsonDocument>();

            if (sortBy != null)
            {
                opt.Sort = sortBy.ToSortDefinition();
            }

            if (fields != null)
            {
                opt.Projection = fields.ToProjectionDefinition();
            }

            if (collation != null)
            {
                opt.Collation = collation;
            }

            var q = await _collection.FindAsync(Query.Empty.ToFilterDefinition(), opt);

            var result = q.ToArrayEx();

            return result;
        }

        public async Task<ResultReturn<BsonDocument[]>> AggregateAsync(params BsonDocument[] pipeline)
        {
            return await AggregateAsync(pipeline, null);
        }

        public async Task<ResultReturn<BsonDocument[]>> AggregateAsync(BsonDocument[] pipeline,
            AggregateOptions options)
        {
            if (!pipeline.HasData())
            {
                return new FailResultReturn<BsonDocument[]>(new ArgumentNullException("pipeline"));
            }

            try
            {
                //var opt = new PipelineDefinition.crea;

                var opt = PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline);

                var result = await _collection.AggregateAsync(opt, options);

                var data = result.ToArrayEx();

                return new SuccessResultReturn<BsonDocument[]>(data);
            }
            catch (Exception ex)
            {
                return new FailResultReturn<BsonDocument[]>(ex);
            }
        }

        public async Task<IPagedList<BsonDocument>> AggregateWithPagedAsync(AggregateBuilder builder, int pageIndex,
            int pageSize,AggregateOptions options=null)
        {
            options =options?? new AggregateOptions() {AllowDiskUse = true};

            var countRet = await this.AggregateAsync(builder.Copy().Count("Count"), options);

            var count = countRet.ReturnData.FirstOrDefault()?.GetInt32("Count") ?? 0;

            if (count <= 0)
            {
                return VM_PagedList<BsonDocument>.Empty(pageIndex, pageSize);
            }

            var data = await this.AggregateAsync(builder.Skip(pageSize * (pageIndex - 1)).Limit(pageSize), options);

            return new VM_PagedList<BsonDocument>(data.ReturnData.ToArrayEx(), pageIndex, pageSize, count);
        }

        public async Task<ResultReturn> InsertAsync(BsonDocument document)
        {
            //var concern = WriteConcern.WMajority;

            try
            {
                await _collection.InsertOneAsync(document);

                return SuccessResultReturn.Default;
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, error: ex);
            }
        }

        public async Task<ResultReturn> InsertBatchAsync(IEnumerable<BsonDocument> document, bool isOrdered = false)
        {
            try
            {
                await _collection.InsertManyAsync(document, new InsertManyOptions() { IsOrdered = isOrdered });

                return SuccessResultReturn.Default;

                //return new ResultReturn(result.Any(x => !x.Ok), null, message: result.HasData() ? result.FirstOrDefault().ErrorMessage : "");
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, error: ex);
            }
        }

        public async Task<ResultReturn> DeleteAsync(IMongoQuery query, RemoveFlags flags = RemoveFlags.None)
        {
            try
            {
                if (flags == RemoveFlags.Single)
                {
                    var ret = await _collection.DeleteOneAsync(query.ToFilterDefinition(), new DeleteOptions());

                    return new SuccessResultReturn(ret.DeletedCount);
                }
                else
                {
                    var ret = await _collection.DeleteManyAsync(query.ToFilterDefinition(), new DeleteOptions());

                    return new SuccessResultReturn(ret.DeletedCount);
                }

                //var result = _collection.DeleteManyAsync(query, flags, new WriteConcern());

                //return new ResultReturn(result.Ok, null, message: result.ErrorMessage, returnCode: result.Code.GetValueOrDefault());
            }
            catch (Exception ex)
            {
                return new ResultReturn(false, error: ex);
            }
        }



        public async Task<ObjectId> FindIDAsync(IMongoQuery query, IMongoSortBy orderBy = null)
        {
            var s = await FindOneAsync(query, orderBy, Fields.Include("_id"));

            if (s != null)
            {
                return s.GetValue("_id").AsObjectId;
            }
            else
                return ObjectId.Empty;
        }

        public async Task<ObjectId[]> FindIDsAsync(IMongoQuery query, IMongoSortBy orderBy = null)
        {
            var s = await FindAsync(query, orderBy, Fields.Include("_id"));

            if (s != null)
            {
                if (s.HasData())
                {
                    return s.Select(x => x.GetValue("_id").AsObjectId).ToArray();
                }
                else
                {
                    return new ObjectId[0];
                }
            }
            else
                return new ObjectId[0];
        }


        public async Task<BsonDocument> FindByIDAsync(ObjectId id, IMongoFields fields = null)
        {
            return await FindOneAsync(Query.EQ("_id", id), fields: fields);
        }

        public async Task<BsonDocument[]> FindByIDsAsync(ObjectId[] ids, IMongoFields fields = null)
        {
            return await FindAsync(Query.In("_id", ids.ToBsonArray()), fields: fields);
        }

        public async Task<bool> ExistsAsync(IMongoQuery query)
        {
            try
            {
                var ret = (await FindAsync(query, fields: Fields.Include("_id"))).HasData();

                return ret;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<BsonDocument[]> DistinctAsync(string field, IMongoQuery query)
        {
            return (await _collection.DistinctAsync<BsonDocument>(field,
                new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx()))).ToArrayEx();
        }

        public async Task<string> CreateIndexAsync(IndexKey key, CreateIndexOptions options = null)
        {
            var builder = Builders<BsonDocument>.IndexKeys;
            IndexKeysDefinition<BsonDocument> index = null;
            switch (key.Type)
            {
                case IndexKeyType.Ascending:
                    index = builder.Ascending(key.Key);
                    break;
                case IndexKeyType.Descending:
                    index = builder.Descending(key.Key);
                    break;
                case IndexKeyType.Geo2D:
                    index = builder.Geo2D(key.Key);
                    break;
                case IndexKeyType.Geo2DSphere:
                    index = builder.Geo2DSphere(key.Key);
                    break;
                case IndexKeyType.GeoHaystack:
                    index = builder.GeoHaystack(key.Key);
                    break;
                case IndexKeyType.Hashed:
                    index = builder.Hashed(key.Key);
                    break;
                case IndexKeyType.Text:
                    index = builder.Text(key.Key);
                    break;
            }

            return await _collection.CreateIndexAsync(index, options ?? new CreateIndexOptions());
        }

        public async Task<string> CreateIndexAsync(IndexKeysBuilder keys, CreateIndexOptions options = null)
        {
            return await _collection.CreateIndexAsync(
                new BsonDocumentIndexKeysDefinition<BsonDocument>(keys.ToBsonDocument()),
                options ?? new CreateIndexOptions());

            //return await _collection.Indexes.CreateOneAsync(new BsonDocumentIndexKeysDefinition<BsonDocument>(keys.ToBsonDocument()), options ?? new CreateIndexOptions());
        }

        public async Task<string> CreateIndexAsync(IndexKeysDefinition<BsonDocument> index, CreateIndexOptions options)
        {
            return await _collection.CreateIndexAsync(index, options ?? new CreateIndexOptions());

            //return await _collection.Indexes.CreateOneAsync(index, options);
        }

        public async Task<int> NewSequenceIDAsync()
        {
            var args = new FindAndModifyArgs();
            args.Query = Query.EQ("ModuleID", _collectionName);
            args.SortBy = null;
            args.Update = MongoDB.Driver.Builders.Update.Inc("CurrentValue", 1);
            args.Upsert = true;
            args.VersionReturned = FindAndModifyDocumentVersion.Modified;

            var ret =await _moduleIDCollection.Value.SourceCollection.FindOneAndUpdateAsync(
                new BsonDocumentFilterDefinition<BsonDocument>(Query.EQ("ModuleID", _collectionName).ToBsonDocumentEx()),
                new BsonDocumentUpdateDefinition<BsonDocument>(MongoDB.Driver.Builders.Update.Inc("CurrentValue", 1).ToBsonDocumentEx()),
                new FindOneAndUpdateOptions<BsonDocument, BsonDocument>(){IsUpsert = true,ReturnDocument= ReturnDocument.After}
            );

            return ret.GetValue("CurrentValue").AsInt32;
        }
    }

    public class IndexKey
    {
        public string Key { set; get; }

        public IndexKeyType Type { set; get; }
    }

    public enum IndexKeyType
    {
        Ascending,

        Descending,

        Geo2D,

        Geo2DSphere,

        GeoHaystack,

        Hashed,

        Text
    }

    internal class MongoCollectionHelper<T> : MongoCollectionHelper, IMongoDBCollectionHelper<T> where T : class
    {
        private static ConcurrentDictionary<Type, IMongoFields> _typeFields = null;

        static MongoCollectionHelper()
        {
            _typeFields = new ConcurrentDictionary<Type, IMongoFields>();
        }

        internal MongoCollectionHelper(string name, IMongoDBClientCollectionExecutor orgCollection, IMongoDBContext helper, Version serverVersion = null) : base(name, orgCollection, helper, serverVersion)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            if (typeof(T) == typeof(BsonDocument))
            {
                throw new Exception("T不允许是BsonDocument");
            }
        }

        #region FindAsWithPaged

        public VM_PagedList<T> FindAsWithPaged(IMongoQuery query, IMongoSortBy order, IMongoFields fields, int pageIndex, int pageSize, Collation collation = null)
        {
            return this.FindAsWithPaged<T>(query, order, fields, pageIndex, pageSize, collation: collation);
        }

        public async Task<VM_PagedList<T>> FindAsWithPagedAsync(IMongoQuery query, IMongoSortBy order, IMongoFields fields, int pageIndex,
            int pageSize, Collation collation = null)
        {
            return await this.FindAsWithPagedAsync<T>(query, order, fields, pageIndex, pageSize, collation: collation);
        }


        public void FindAsWithPagedAndCallback(IMongoQuery query, Action<T[]> callback, IMongoSortBy order = null, IMongoFields fields = null,
            int pageSize = 20, Collation collation = null)
        {
            var pageCount = GetQueryPageCount(query, pageSize);

            if (pageCount <= 0)
            {
                return;
            }

            for (int i = 1; i <= pageCount; i++)
            {
                var data = FindAsWithPaged(query, order, fields ?? getTypeProperties(typeof(T)), i, pageSize, collation: collation);

                if (!data.HasData())
                {
                    break;
                }
                else
                {
                    callback?.Invoke(data.Data.ToArrayEx());
                }

            }
        }


        public VM_PagedList<TNew> FindAsWithPaged<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null,
            int pageIndex = 1, int pageSize = 20, Collation collation = null) where TNew : class
        {
            var docList = FindWithPaged(query, order, fields ?? getTypeProperties(typeof(TNew)), pageIndex, pageSize, collation: collation);

            if (!docList.Data.HasData())
            {
                return new VM_PagedList<TNew>(new TNew[0], pageIndex, pageSize);
            }

            var lst = new List<TNew>(docList.Data.Count());

            foreach (var d in docList.Data)
            {
                var data = BsonSerializer.Deserialize<TNew>(d);

                lst.Add(data);
            }

            return new VM_PagedList<TNew>(lst, pageIndex, pageSize, docList.TotalCount);
        }

        public async Task<VM_PagedList<TNew>> FindAsWithPagedAsync<TNew>(IMongoQuery query, IMongoSortBy order = null,
            IMongoFields fields = null,
            int pageIndex = 1, int pageSize = 20, Collation collation = null) where TNew : class
        {
            var docList = await FindWithPagedAsync(query, order, fields ?? getFields(typeof(TNew)), pageIndex, pageSize, collation: collation);

            if (!docList.Data.HasData())
            {
                return new VM_PagedList<TNew>(new TNew[0], pageIndex, pageSize);
            }

            var lst = new List<TNew>(docList.Data.Count());

            foreach (var d in docList.Data)
            {
                var data = BsonSerializer.Deserialize<TNew>(d);

                lst.Add(data);
            }

            return new VM_PagedList<TNew>(lst, pageIndex, pageSize, docList.TotalCount);
        }

        #endregion

        #region FindAs

        public T[] FindAs(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = 0, Collation collation = null)
        {
            return this.FindAs<T>(query, order, fields, skip, take, collation: collation);
        }

        public async Task<T[]> FindAsAsync(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = 0, Collation collation = null)
        {
            return await this.FindAsAsync<T>(query, order, fields, skip, take, collation: collation);
        }


        public TNew[] FindAs<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = 0, Collation collation = null)
        {
            if (order == null)
            {
                order = SortBy.Null;
            }

            if (fields == null)
            {
                fields = getFields(typeof(TNew));
            }

            var docList = Find(query, order, fields, skip, take, collation: collation);

            if (!docList.HasData())
            {
                return new TNew[0];
            }

            var lst = new List<TNew>(docList.Length);

            for (int i = 0; i < docList.Length; i++)
            {
                var item = docList[i];

                var data = BsonSerializer.Deserialize<TNew>(item);

                lst.Add(data);
            }

            return lst.ToArray();
        }


        public async Task<TNew[]> FindAsAsync<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = 0, Collation collation = null)
        {
            if (order == null)
            {
                order = SortBy.Null;
            }

            if (fields == null)
            {
                fields = getFields(typeof(TNew));
            }

            var docList = await FindAsync(query, order, fields, collation: collation);

            if (!docList.HasData())
            {
                return new TNew[0];
            }

            var lst = new List<TNew>(docList.Length);

            for (int i = 0; i < docList.Length; i++)
            {
                var item = docList[i];

                var data = BsonSerializer.Deserialize<TNew>(item);

                lst.Add(data);
            }

            return lst.ToArray();
        }


        #endregion

        #region FindOneAs

        public T FindOneAs(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null)
        {
            return this.FindOneAs<T>(query, order, fields);
        }

        public async Task<T> FindOneAsAsync(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null)
        {
            return await this.FindOneAsAsync<T>(query, order, fields);
        }


        public TNew FindOneAs<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null)
        {
            if (order == null)
            {
                order = SortBy.Null;
            }

            if (fields == null)
            {
                fields = getFields(typeof(TNew));
            }

            var item = FindOne(query, order, fields);

            if (item == null)
            {
                return default(TNew);
            }
            else
            {
                var data = BsonSerializer.Deserialize<TNew>(item);

                return data;
            }
        }

        public async Task<TNew> FindOneAsAsync<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null)
        {
            if (order == null)
            {
                order = SortBy.Null;
            }

            if (fields == null)
            {
                fields = getFields(typeof(TNew));
            }

            var item = await FindOneAsync(query, order, fields);

            if (item == null)
            {
                return default(TNew);
            }
            else
            {
                var data = BsonSerializer.Deserialize<TNew>(item);

                return data;
            }
        }


        #endregion

        #region FindAndModifyAs

        public ResultReturn<T> FindAndModifyAs(IMongoQuery query, IMongoUpdate update, IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false)
        {
            return this.FindAndModifyAs<T>(query, update, sortBy, fields ?? getTypeProperties(typeof(T)), returnNew, upsert);
        }

        public ResultReturn<TNew> FindAndModifyAs<TNew>(IMongoQuery query, IMongoUpdate update, IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false)
        {
            sortBy = sortBy ?? SortBy.Null;

            var docList = FindAndModify(query, update, sortBy, fields ?? getFields(typeof(TNew)), returnNew, upsert);

            if (!docList.IsSuccess)
            {
                return new FailResultReturn<TNew>(docList.Message);
            }

            if (docList.ReturnData.HasData())
            {
                //var array = docList.ReturnData;

                //var lst = new List<TNew>(array.Length);

                //for (int i = 0; i < array.Length; i++)
                //{
                //    var item = docList.ReturnData[i];

                //    var data = BsonSerializer.Deserialize<TNew>(item);

                //    lst.Add(data);
                //}

                return new ResultReturn<TNew>(true, BsonSerializer.Deserialize<TNew>(docList.ReturnData));
            }
            else
            {
                return new ResultReturn<TNew>(true, default(TNew));
            }

        }

        public async Task<ResultReturn<T>> FindOneAndModifyAsAsync<T>(IMongoQuery query, IMongoUpdate update, IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false)
        {
            var docList = await FindOneAndModifyAsync(query, update, sortBy, fields ?? getFields(typeof(T)), returnNew, upsert);

            if (!docList.IsSuccess)
            {
                return new FailResultReturn<T>(docList.Message);
            }

            if (docList.ReturnData.HasData())
            {
                var data = BsonSerializer.Deserialize<T>(docList.ReturnData);

                return new ResultReturn<T>(true, data);

                //var array = docList.ReturnData;

                //var lst = new List<T>(array.Length);

                //for (int i = 0; i < array.Length; i++)
                //{
                //    var item = docList.ReturnData[i];

                //    var data = BsonSerializer.Deserialize<T>(item);

                //    lst.Add(data);
                //}

                //return new ResultReturn<T[]>(true, lst.ToArray());
            }
            else
            {
                return new ResultReturn<T>(true);

                //return new ResultReturn<T[]>(true, new T[0]);
            }
        }

        #endregion

        #region Insert

        public ResultReturn Insert(T data)
        {
            var json = data.ToBsonDocument();

            return Insert(json);

            return InsertBatch(new[] { data });
        }

        public async Task<ResultReturn> InsertAsync(T data)
        {
            var json = data.ToBsonDocument();

            return await InsertAsync(json);

        }


        public ResultReturn InsertBatch(IEnumerable<T> data, bool isOrdered = false)
        {
            return InsertBatch(data.Select(x => x.ToBsonDocument()), isOrdered);
        }

        public async Task<ResultReturn> InsertBatchAsync(IEnumerable<T> data, bool isOrdered = false)
        {
            return await base.InsertBatchAsync(data.Select(x => x.ToBsonDocument()), isOrdered);
        }
        #endregion

        #region FindAllAs

        public T[] FindAllAs(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null)
        {
            return this.FindAllAs<T>(sortBy, fields, collation: collation);

            //if (sortBy == null)
            //{
            //    sortBy = SortBy.Null;
            //}

            //if (fields == null)
            //{
            //    fields = Fields.Null;
            //}


            //return Collection.FindAllAs<T>().SetSortOrder(sortBy).SetFields(fields).SetFlags(QueryFlags.TailableCursor).ToArray();
        }

        public async Task<T[]> FindAllAsAsync(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null)
        {
            return await this.FindAllAsAsync<T>(sortBy, fields, collation: collation);
        }


        public TNew[] FindAllAs<TNew>(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null)
        {
            if (sortBy == null)
            {
                sortBy = SortBy.Null;
            }

            if (fields == null)
            {
                fields = getFields(typeof(TNew));
            }

            var data = base.FindAll(sortBy, fields, collation: collation);

            if (data.HasData())
            {
                return data.Select(x => BsonSerializer.Deserialize<TNew>(x)).ToArray();
            }
            else
            {
                return new TNew[0];
            }

            //return Collection.FindAllAs<TNew>().SetSortOrder(sortBy).SetFields(fields).SetFlags(QueryFlags.TailableCursor).ToArray();
        }

        public async Task<TNew[]> FindAllAsAsync<TNew>(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null)
        {
            if (sortBy == null)
            {
                sortBy = SortBy.Null;
            }

            if (fields == null)
            {
                fields = getFields(typeof(TNew));
            }

            var data = await base.FindAllAsync(sortBy, fields, collation: collation);

            if (data.HasData())
            {
                return data.Select(x => BsonSerializer.Deserialize<TNew>(x)).ToArray();
            }
            else
            {
                return new TNew[0];
            }

            //return Collection.FindAllAs<TNew>().SetSortOrder(sortBy).SetFields(fields).SetFlags(QueryFlags.TailableCursor).ToArray();
        }

        #endregion

        #region FindByIDAs

        public T FindByIDAs(ObjectId id, IMongoFields fields = null)
        {
            return FindByIDAs<T>(id, fields);
        }

        public T[] FindByIDsAs(ObjectId[] ids, IMongoFields fields = null)
        {
            return this.FindByIDsAs<T>(ids, fields);
        }

        public async Task<T> FindByIDAsAsync(ObjectId id, IMongoFields fields = null)
        {
            return await this.FindByIDAsAsync<T>(id, fields);
        }


        public async Task<T[]> FindByIDsAsAsync(ObjectId[] ids, IMongoFields fields = null)
        {
            return await this.FindByIDsAsAsync<T>(ids, fields);
        }

        public async Task<TNew> FindByIDAsAsync<TNew>(ObjectId id, IMongoFields fields = null)
        {
            return await FindOneAsAsync<TNew>(Query.EQ("_id", id), fields: fields ?? getFields(typeof(TNew)));
        }

        public async Task<TNew[]> FindByIDsAsAsync<TNew>(ObjectId[] ids, IMongoFields fields = null)
        {
            return await FindAsAsync<TNew>(Query.In("_id", ids.ToBsonArray()), fields: fields ?? getTypeProperties(typeof(TNew)));
        }

        public TNew FindByIDAs<TNew>(ObjectId id, IMongoFields fields = null)
        {
            return FindOneAs<TNew>(Query.EQ("_id", id), fields: fields ?? getFields(typeof(TNew)));
        }

        public TNew[] FindByIDsAs<TNew>(ObjectId[] ids, IMongoFields fields = null)
        {
            return FindAs<TNew>(Query.In("_id", ids.ToBsonArray()), fields: fields ?? getFields(typeof(TNew)));
        }

        #endregion


        public T[] DistinctAs(string field, IMongoQuery query)
        {
            return this.DistinctAs<T>(field, query);
        }

        public async Task<T[]> DistinctAsAsyncs(string field, IMongoQuery query)
        {
            return await this.DistinctAsAsync<T>(field, query);
        }

        public TNew[] DistinctAs<TNew>(string field, IMongoQuery query) where TNew : class
        {
            return SourceCollection.Distinct<TNew>(field, query.ToBsonDocumentEx()).ToList().ToArrayEx();
        }

        public async Task<IPagedList<T>> AggregateAsWithPagedAsync(AggregateBuilder builder, int pageIndex, int pageSize, AggregateOptions options = null)
        {
            var data = await base.AggregateWithPagedAsync(builder, pageIndex, pageSize, options);

            return data.Cast(doc => doc.ToObject<T>());
        }

        public async Task<IPagedList<TNew>> AggregateAsWithPagedAsync<TNew>(AggregateBuilder builder, int pageIndex, int pageSize, AggregateOptions options = null) where TNew : class
        {
            var data = await base.AggregateWithPagedAsync(builder, pageIndex, pageSize, options);

            return data.Cast(doc => doc.ToObject<TNew>());
        }

        public async Task<TNew[]> DistinctAsAsync<TNew>(string field, IMongoQuery query) where TNew : class
        {
            return (await SourceCollection.DistinctAsync<TNew>(field, query.ToBsonDocumentEx())).ToArrayEx();
        }



        private static IMongoFields getFields(Type type)
        {
            if (BsonSerializer.LookupSerializer(type) != null)
            {
                return Fields.Null;
            }

            return _typeFields.GetOrAdd(type, getTypeProperties(type));
        }

        private static IMongoFields getTypeProperties(Type type)
        {
            var names = type.GetProperties().Select(x => x.Name).ToArrayEx();

            return Fields.Include(names);
        }
    }

}



