using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper
{


    public partial class MongoDBContextHelper: IMongoDBContext
    {
        private MongoServer _server = null;
        private IMongoDatabase _mongoDb = null;
        //private MongoDatabase _oldMongoDb = null;
        private MongoClient _client = null;
        private Lazy<IMongoDBCollectionHelper> _moduleIDCollection = null;
        private ConcurrentDictionary<string, IMongoDBCollectionHelper> _collectionCache = null;
        private ConcurrentDictionary<string, IMongoDBCollectionHelper> _collectionCache1 = null;
        

        static MongoDBContextHelper()
        {
            BsonSerializer.RegisterSerializer(
                typeof(DateTime),
                new DateTimeSerializerEx());

            BsonSerializer.RegisterSerializer(typeof(Digit), new MongoDB_DigitSerializer());

            var cp = new ConventionPack();
            cp.Add(new IgnoreExtraElementsConvention(true));  //忽略多余字段

            ConventionRegistry.Register("ApplicationConventions", cp, t => true);

            //try
            //{
            //    BsonSerializer.RegisterSerializer(typeof(decimal),new MongoDB_DecimalSerializer(this));
            //}
            //catch (Exception e)
            //{
            //}
        }

        //public MongoDBContextHelper() : this("", "") { }

        public MongoDBContextHelper(string connectStr, string databaseName="",bool autoCheckConn=true)
        {
            if (string.IsNullOrWhiteSpace(connectStr))
            {
                throw new ArgumentOutOfRangeException(nameof(connectStr));
            }

            //if (string.IsNullOrWhiteSpace(connectStr))
            //{
            //    connectStr = CustomConfigManager.Default.AppSettings.GetValueByName<string>("MongoDBConnection");
            //}

            //if (string.IsNullOrWhiteSpace(databaseName))
            //{
            //    databaseName = CustomConfigManager.Default.AppSettings.GetValueByName<string>("MongoDBName");
            //}

            _collectionCache=new ConcurrentDictionary<string, IMongoDBCollectionHelper>();
            _collectionCache1=new ConcurrentDictionary<string, IMongoDBCollectionHelper>();

            try
            {
                

                _client = new MongoClient(connectStr);
                _server = _client.GetServer(); ;

                
            }
            catch (Exception)
            {
                throw;
            }
            
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                var uri=new MongoUrl(connectStr);

                databaseName = uri.DatabaseName;
            }

            //_oldMongoDb = _server.GetDatabase(databaseName);

            

            _mongoDb = _client.GetDatabase(databaseName);

            _moduleIDCollection =new Lazy<IMongoDBCollectionHelper>(()=> new MongoCollectionHelper("sys_ModuleID", new CollectionExecutor(_mongoDb.GetCollection<BsonDocument>("sys_ModuleID")), this));

            //try
            //{
            //    var version = _mongoDb.RunCommand<dynamic>(new BsonDocument("serverStatus", 1)).version;

            //    ServerVersion = new Version(version);
            //}
            //catch (Exception e)
            //{
            //    ServerVersion = new Version(3, 6);
            //}


            //if (autoCheckConn)
            //{
            //    TimerPool.Default.Add(30000, autoReconn);
            //}
        }


        public static void RegisterSerializers()
        {
            try
            {
                BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            }
            catch (Exception e)
            {
                
            }

            try
            {
                BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

            }
            catch (Exception e)
            {
            }
            


            if (JsonConvert.DefaultSettings==null)
            {
                var jsonDefaultSetting=new JsonSerializerSettings();

                jsonDefaultSetting.Converters.Add(new ObjectIDJsonConverter());

                JsonConvert.DefaultSettings = () => jsonDefaultSetting;
            }
            else
            {
                var tmp = JsonConvert.DefaultSettings();

                if (!tmp.Converters.Any(x=>x is ObjectIDJsonConverter))
                {
                    tmp.Converters.Add(new ObjectIDJsonConverter());
                }
                
            }
        }
        
        /// <summary>
        /// 返回一个泛型容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">容器名称</param>
        /// <param name="isForceNew">是否强制返回新创建的容器对象,如果为false,则返回缓存中的容器对象</param>
        /// <returns></returns>
        public IMongoDBCollectionHelper<T> GetCollection<T>(string name="", bool isForceNew = false,MongoCollectionSettings settings=null) where T : class
        {
            //if (string.IsNullOrWhiteSpace(name))
            //{
            //    throw new ArgumentOutOfRangeException("name");
            //}

            if (string.IsNullOrWhiteSpace(name))
            {
                name = typeof(T).Name;
            }
            
            if (isForceNew)
            {
                return new MongoCollectionHelper<T>(name, new CollectionExecutor(_mongoDb.GetCollection<BsonDocument>(name, settings)), this , ServerVersion);
            }
            else
            {
                return (IMongoDBCollectionHelper<T>) _collectionCache.GetOrAdd(name,
                    x => new MongoCollectionHelper<T>(x, new CollectionExecutor(_mongoDb.GetCollection<BsonDocument>(name,settings)), this ,
                        ServerVersion));

                //IMongoDBCollectionHelper col = null;

                //if (_collectionCache.TryGetValue(name,out col))
                //{
                //    return (IMongoDBCollectionHelper<T>) col;
                //}
                //else
                //{
                //   var temp= new MongoCollectionHelper<T>(name, _mongoDb, _oldMongoDb,this, isJournal, wlevel, ServerVersion);

                //    _collectionCache.AddOrUpdate(name, temp);

                //    return temp;
                //}
            }
            
        }

        public IMongoDBCollectionHelper<T> GetCollection<T>( bool isForceNew = false) where T : class
        {
            return GetCollection<T>(typeof (T).Name, isForceNew);

        }

        /// <summary>
        /// 返回一个BsonDocument类型的容器
        /// </summary>
        /// <param name="name">容器名称</param>
        /// <param name="isForceNew">是否强制返回新创建的容器对象,如果为false,则返回缓存中的容器对象</param>
        /// <returns></returns>
        public IMongoDBCollectionHelper GetCollection(string name, bool isForceNew = false, bool isJournal = false, int wlevel = 1)
        {
            

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException("name");
            }

            if (isForceNew)
            {
                return new MongoCollectionHelper(name, new CollectionExecutor(_mongoDb.GetCollection<BsonDocument>(name)), this ,serverVersion: ServerVersion);
            }
            else
            {
                IMongoDBCollectionHelper col = null;

                return _collectionCache1.GetOrAdd(name,
                    x => new MongoCollectionHelper(x, new CollectionExecutor(_mongoDb.GetCollection<BsonDocument>(name)), this , ServerVersion));

                //if (_collectionCache1.TryGetValue(name, out col))
                //{
                //    return col;
                //}
                //else
                //{
                //    var temp = new MongoCollectionHelper(name, _mongoDb, _oldMongoDb,this, isJournal, wlevel,ServerVersion);

                //    _collectionCache1.AddOrUpdate(name, temp);

                //    return temp;
                //}
            }
        }


        /// <summary>
        /// 返回一个BsonDocument类型的容器
        /// </summary>
        /// <param name="name">容器名称</param>
        /// <param name="isForceNew">是否强制返回新创建的容器对象,如果为false,则返回缓存中的容器对象</param>
        /// <returns></returns>
        public IMongoDBCollectionAsyncHelper GetAsyncCollection(string name, bool isForceNew = false, bool isJournal = false, int wlevel = 1)
        {


            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException("name");
            }

            if (isForceNew)
            {
                return new MongoCollectionHelper(name, new CollectionExecutor(_mongoDb.GetCollection<BsonDocument>(name)), this, serverVersion: ServerVersion);
            }
            else
            {
                IMongoDBCollectionHelper col = null;

                return (IMongoDBCollectionAsyncHelper)_collectionCache1.GetOrAdd(name,
                    x => new MongoCollectionHelper(x, new CollectionExecutor(_mongoDb.GetCollection<BsonDocument>(name)), this, ServerVersion));

                //if (_collectionCache1.TryGetValue(name, out col))
                //{
                //    return col;
                //}
                //else
                //{
                //    var temp = new MongoCollectionHelper(name, _mongoDb, _oldMongoDb,this, isJournal, wlevel,ServerVersion);

                //    _collectionCache1.AddOrUpdate(name, temp);

                //    return temp;
                //}
            }
        }

        public BsonDocument RunCommand(params BsonElement[] cmd)
        {
            var command = new CommandDocument(cmd);

            return _mongoDb.RunCommand<BsonDocument>(command);
        }

        public BsonDocument RunCommand(BsonDocument cmd)
        {
            var command = new CommandDocument(cmd);

            return _mongoDb.RunCommand<BsonDocument>(command);
        }

        public T RunCommand<T>(params BsonElement[] cmd)
        {
            var command = new CommandDocument(cmd);

            return _mongoDb.RunCommand<T>(command);
        }

        public T RunCommand<T>(BsonDocument cmd)
        {
            var command = new CommandDocument(cmd);

            return _mongoDb.RunCommand<T>(command);
        }

        public int NewSequenceID(string ModuleID)
        {
            var ret = _moduleIDCollection.Value.FindAndModify(Query.EQ("ModuleID", ModuleID),Update.Inc("CurrentValue", 1),returnNew:true,upsert:true);

            return ret.ReturnData.GetValue("CurrentValue").AsInt32;
        }

        /// <summary>
        /// 在数据库中添加一个临时数据,可设置过期时间
        /// </summary>
        /// <param name="templateDataKey">临时数据key,如果出现key重复,则只获取最新一条该key的数据</param>
        /// <param name="data">临时数据</param>
        /// <param name="expireDt">过期时间,为空则不过期</param>
        public void CreateTemplateData(string templateDataKey, BsonValue data, DateTime? expireDt = null)
        {

            var coll = GetCollection("sys_TemplateData_1");

            //GetCollection("sys_TemplateData_1").Insert(new BsonDocument()
            //                                           {
            //                                               ["Key"] = templateDataKey,
            //                                               ["Data"] = data
            //                                           });

            var bsonDoc = new BsonDocument()
                          {
                              ["Key"] = templateDataKey,
                              ["Data"] = data
                          };

            if (expireDt.HasValue)
            {
                bsonDoc.Add("ExpireDt", expireDt.Value);
            }

            coll.Insert(bsonDoc);
        }

        /// <summary>
        /// 在数据库中添加一个临时数据,可设置过期时间,并且强制key必须为唯一值,如果重复,则抛出错误
        /// </summary>
        /// <param name="templateDataKey">临时数据key,如果出现key重复,则将抛出错误</param>
        /// <param name="data">临时数据</param>
        /// <param name="expireDt">过期时间,为空则不过期</param>
        public void CreateUnqieTemplateKeyData(string templateDataKey, BsonValue data, DateTime? expireDt = null)
        {
            var coll = GetCollection("sys_TemplateData_1");

            if (coll.Exists(Query.EQ("Key", templateDataKey)))
            {
                throw new ArgumentOutOfRangeException($"参数templateDataKey指定的值重复");
            }

            var bsonDoc = new BsonDocument()
                          {
                              ["Key"] = templateDataKey,
                              ["Data"] = data
                          };

            if (expireDt.HasValue)
            {
                bsonDoc.Add("ExpireDt", expireDt.Value);
            }

            coll.Insert(bsonDoc);
            
        }

        /// <summary>
        /// 获取临时数据
        /// </summary>
        /// <param name="templateDataKey"></param>
        /// <returns></returns>
        public BsonValue GetTemplateKeyData(string templateDataKey)
        {
            var data = GetCollection("sys_TemplateData_1").FindOne(Query.EQ("Key", templateDataKey),
                SortBy.Descending("_id"), fields: Fields.Include("Data"));
            
            return data.GetValue("Data");
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="serviceHost">服务器地址/IP地址,不需要带端口号</param>
        /// <param name="databaseName">数据库名称</param>
        /// <param name="port">端口号,默认为27017</param>
        /// <param name="userName">连接用户名</param>
        /// <param name="password">连接密码</param>
        /// <param name="useSSL">是否为SSL连接</param>
        public MongoDBContextHelper Create(string serviceHost,
            string databaseName,
            int port = 27017,
            string userName = "",
            string password = "",
            bool useSSL = false,
            bool autoCheckConn = true)
        {
            return new MongoDBContextHelper($"mongodb://{(!string.IsNullOrWhiteSpace(userName) ? $"{userName}:{password}@" : "")}{serviceHost}:{port}?ssl={useSSL}", databaseName, autoCheckConn);
        }

        public Version ServerVersion { set; get; }

        private void autoReconn(object state)
        {
            var reconnQty = 0;

            while (_server.State == MongoServerState.Disconnected)
            {
                Thread.Sleep(1000);
                try
                {
                    _server.Reconnect();
                    reconnQty = 0;
                }
                catch (Exception ex)
                {
                    reconnQty++;
                }

                if (reconnQty>10)
                {
                    break;
                }
            }
            
        }

        private class CollectionExecutor : IMongoDBClientCollectionExecutor
        {
            private IMongoCollection<BsonDocument> _collection = null;

            public CollectionExecutor(IMongoCollection<BsonDocument> collection)
            {
                _collection = collection;
            }
            
            public IEnumerable<BsonDocument> Aggregate(PipelineDefinition<BsonDocument, BsonDocument> pipeline, AggregateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return tryInvoke(() =>
                {
                    return new AsyncCursorToEnumerable<BsonDocument>(_collection.Aggregate(pipeline, options, cancellationToken));
                }, typeof(MongoConnectionException), 3);
                
            }

            public async Task<IEnumerable<BsonDocument>> AggregateAsync(PipelineDefinition<BsonDocument, BsonDocument> pipeline, AggregateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await tryInvokeAsync(async () =>
                {
                    return new AsyncCursorToEnumerable<BsonDocument>(await _collection.AggregateAsync(pipeline, options, cancellationToken));
                }, typeof(MongoConnectionException), 3);

                
            }

            public BulkWriteResult<BsonDocument> BulkWrite(IEnumerable<WriteModel<BsonDocument>> requests, BulkWriteOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.BulkWrite(requests, options, cancellationToken);
            }

            public async Task<BulkWriteResult<BsonDocument>> BulkWriteAsync(IEnumerable<WriteModel<BsonDocument>> requests, BulkWriteOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.BulkWriteAsync(requests, options, cancellationToken);
            }

            public long Count(FilterDefinition<BsonDocument> filter, CountOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return tryInvoke(() =>
                {
                    return _collection.Count(filter, options, cancellationToken);
                }, typeof(MongoConnectionException), 3);

                //return _collection.Count(filter, options, cancellationToken);
            }

            public async Task<long> CountAsync(FilterDefinition<BsonDocument> filter, CountOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await tryInvokeAsync(async () =>
                    {
                        return await _collection.CountAsync(filter, options, cancellationToken);
                    }, typeof(MongoConnectionException), 3);

                //long ret=0l;

                //tryInvoke(async () =>
                //{
                //    ret = await _collection.CountAsync(filter, options, cancellationToken);
                //}, typeof(MongoConnectionException), 3);

                //return ret;
            }

            public DeleteResult DeleteMany(FilterDefinition<BsonDocument> filter, DeleteOptions options,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.DeleteMany(filter, options, cancellationToken);
            }

            public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<BsonDocument> filter, DeleteOptions options,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.DeleteManyAsync(filter, options, cancellationToken);
            }
            
            public DeleteResult DeleteOne(FilterDefinition<BsonDocument> filter, DeleteOptions options,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.DeleteOne(filter, options, cancellationToken);
            }

            public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<BsonDocument> filter, DeleteOptions options,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.DeleteOneAsync(filter, options, cancellationToken);
            }

            public IEnumerable<TField> Distinct<TField>(FieldDefinition<BsonDocument, TField> field, FilterDefinition<BsonDocument> filter, DistinctOptions options = null,
            CancellationToken cancellationToken = new CancellationToken()) where TField : class
            {
                return tryInvoke(() =>
                {
                    return new AsyncCursorToEnumerable<TField>(_collection.Distinct(field, filter, options, cancellationToken));
                }, typeof(MongoConnectionException), 3);
                
            }

            public async Task<IEnumerable<TField>> DistinctAsync<TField>(FieldDefinition<BsonDocument, TField> field, FilterDefinition<BsonDocument> filter, DistinctOptions options = null,
                CancellationToken cancellationToken = new CancellationToken()) where TField : class
            {
                return await tryInvokeAsync(async () =>
                {
                    return new AsyncCursorToEnumerable<TField>(await _collection.DistinctAsync(field, filter, options, cancellationToken));
                }, typeof(MongoConnectionException), 3);
            }

            public IEnumerable<TProjection> FindSync<TProjection>(FilterDefinition<BsonDocument> filter, FindOptions<BsonDocument, TProjection> options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                //AsyncCursorToEnumerable<TProjection> ret = null;

                return tryInvoke(() =>
                {
                   return new AsyncCursorToEnumerable<TProjection>(_collection.FindSync(filter, options, cancellationToken));
                }, typeof(MongoConnectionException), 3);

                //return ret;
            }

            public async Task<IEnumerable<TProjection>> FindAsync<TProjection>(FilterDefinition<BsonDocument> filter, FindOptions<BsonDocument, TProjection> options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await tryInvokeAsync(async () =>
                {
                    return new AsyncCursorToEnumerable<TProjection>(await _collection.FindAsync(filter, options, cancellationToken));
                }, typeof(MongoConnectionException), 3);

            }

            public TProjection FindOneAndDelete<TProjection>(FilterDefinition<BsonDocument> filter, FindOneAndDeleteOptions<BsonDocument, TProjection> options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.FindOneAndDelete(filter, options, cancellationToken);
            }

            public async Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<BsonDocument> filter, FindOneAndDeleteOptions<BsonDocument, TProjection> options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.FindOneAndDeleteAsync(filter, options, cancellationToken);
            }

            public TProjection FindOneAndReplace<TProjection>(FilterDefinition<BsonDocument> filter, BsonDocument replacement,
                FindOneAndReplaceOptions<BsonDocument, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.FindOneAndReplace(filter, replacement ,options, cancellationToken);
            }

            public async Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<BsonDocument> filter, BsonDocument replacement,
                FindOneAndReplaceOptions<BsonDocument, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.FindOneAndReplaceAsync(filter, replacement, options, cancellationToken);
            }

            public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update,
                FindOneAndUpdateOptions<BsonDocument, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.FindOneAndUpdate(filter, update, options, cancellationToken);
            }

            public async Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update,
                FindOneAndUpdateOptions<BsonDocument, TProjection> options = null, CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
            }

            public void InsertOne(BsonDocument document, InsertOneOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                _collection.InsertOne(document,options,cancellationToken);
            }


            public async Task InsertOneAsync(BsonDocument document, InsertOneOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                await _collection.InsertOneAsync(document, options, cancellationToken);
            }

            public void InsertMany(IEnumerable<BsonDocument> documents, InsertManyOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                _collection.InsertMany(documents, options, cancellationToken);
            }

            public async Task InsertManyAsync(IEnumerable<BsonDocument> documents, InsertManyOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                await _collection.InsertManyAsync(documents, options, cancellationToken);
            }

            public ReplaceOneResult ReplaceOne(FilterDefinition<BsonDocument> filter, BsonDocument replacement, UpdateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.ReplaceOne(filter, replacement, options, cancellationToken);
            }

            public async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<BsonDocument> filter, BsonDocument replacement, UpdateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken);
            }

            public UpdateResult UpdateMany(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, UpdateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.UpdateMany(filter, update, options, cancellationToken);
            }

            public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, UpdateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.UpdateManyAsync(filter, update, options, cancellationToken);
            }

            public UpdateResult UpdateOne(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, UpdateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return _collection.UpdateOne(filter, update, options, cancellationToken);
            }

            public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, UpdateOptions options = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                return await _collection.UpdateOneAsync(filter, update, options, cancellationToken);
            }

            public async Task<string> CreateIndexAsync(IndexKeysDefinition<BsonDocument> index, CreateIndexOptions options)
            {
                return await _collection.Indexes.CreateOneAsync(index, options);
            }

            public IMongoCollection<BsonDocument> SourceCollection => _collection;

            private T tryInvoke<T>(Func<T> action,Type catchException, int count,int waitForMSec=300)
            {
                label1:
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    if (e.GetType()== catchException)
                    {
                        Debug.WriteLine("数据库连接超时错误");

                        count--;

                        if (count < 0)
                        {
                            throw;
                        }
                        else
                        {
                            if (waitForMSec>0)
                            {
                                Thread.Sleep(waitForMSec);
                            }
                            goto label1;
                        }
                    }
                    else
                    {
                        throw;
                    }
                    
                }
            }

            private async Task<T> tryInvokeAsync<T>(Func<Task<T>> action, Type catchException, int count, int waitForMSec = 300)
            {
                label1:
                try
                {
                    //return await Task<T>.Factory.StartNew(() =>
                    //{
                        return await action();
                    //});
                    
                }
                catch (Exception e)
                {
                    if (e.GetType() == catchException)
                    {
                        Debug.WriteLine("数据库连接超时错误");
                        count--;

                        if (count < 0)
                        {
                            throw;
                        }
                        else
                        {
                            if (waitForMSec > 0)
                            {
                                await Task.Delay(waitForMSec);
                                //Thread.Sleep(waitForMSec);
                            }

                            goto label1;
                        }
                    }
                    else
                    {
                        throw;
                    }

                }
            }
        }

        private class AsyncCursorToEnumerable<TProjection> : IEnumerable<TProjection>
        {
            private IAsyncCursor<TProjection> _cursor = null;

            public AsyncCursorToEnumerable(IAsyncCursor<TProjection> cursor)
            {
                _cursor = cursor;
            }

            public IEnumerator<TProjection> GetEnumerator()
            {
                while (_cursor.MoveNext())
                {
                    foreach (var item in _cursor.Current)
                    {
                        yield return item;
                    }

                };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }
}
