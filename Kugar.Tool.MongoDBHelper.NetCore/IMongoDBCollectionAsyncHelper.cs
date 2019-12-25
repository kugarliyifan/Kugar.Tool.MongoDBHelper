using System.Collections.Generic;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Kugar.Tool.MongoDBHelper
{
    /// <summary>
    /// 异步collection的接口
    /// </summary>
    public interface IMongoDBCollectionAsyncHelper
    {
        Task<BsonDocument[]> FindAsync(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0,
            int take = 0, Collation collation = null);

        
        Task<BsonDocument> FindOneAsync(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, Collation collation = null);

        Task<ObjectId> FindOneIDAsync(IMongoQuery query, IMongoSortBy order = null);

        /// <summary>
        /// 查找并修改后,返回结果
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sortBy"></param>
        /// <param name="update"></param>
        /// <param name="fields"></param>
        /// <param name="returnNew"></param>
        /// <param name="upsert"></param>
        /// <returns></returns>
        Task<ResultReturn<BsonDocument>> FindOneAndModifyAsync(IMongoQuery query, IMongoUpdate update,
            IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false);

        Task<ResultReturn> InsertAsync(BsonDocument document);

        Task<ResultReturn> InsertBatchAsync(IEnumerable<BsonDocument> document, bool isOrdered = false);


        Task<ResultReturn> DeleteAsync(IMongoQuery query, RemoveFlags flags = RemoveFlags.None);


        Task<bool> ExistsAsync(IMongoQuery query);

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <param name="query"></param>
        /// <param name="updater"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        Task<ResultReturn> UpdateAsync(IMongoQuery query, IMongoUpdate updater, UpdateFlags flags = UpdateFlags.None);


        Task<ObjectId[]> FindIDsAsync(IMongoQuery query, IMongoSortBy orderBy = null);

        Task<ObjectId> FindIDAsync(IMongoQuery query, IMongoSortBy orderBy = null);

        Task<BsonDocument[]> FindByIDsAsync(ObjectId[] ids, IMongoFields fields = null);

        Task<BsonDocument> FindByIDAsync(ObjectId id, IMongoFields fields = null);

        Task<BsonDocument[]> FindAllAsync(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null);


        Task<int> CountAsync(IMongoQuery query);

        /// <summary>
        /// 创建一个索引
        /// </summary>
        /// <param name="index"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<string> CreateIndexAsync(IndexKey key, CreateIndexOptions options = null);

        /// <summary>
        /// 异步创建一个索引
        /// </summary>
        /// <param name="index"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<string> CreateIndexAsync(IndexKeysBuilder index, CreateIndexOptions options = null);

        /// <summary>
        /// 异步创建一个索引
        /// </summary>
        /// <param name="index"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<string> CreateIndexAsync(IndexKeysDefinition<BsonDocument> index, CreateIndexOptions options);

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <param name="fields"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="collation"></param>
        /// <returns></returns>
        Task<VM_PagedList<BsonDocument>> FindWithPagedAsync(IMongoQuery query, IMongoSortBy order = null,
            IMongoFields fields = null, int pageIndex = 1, int pageSize = 20, Collation collation = null);

  
        Task<ResultReturn<BsonDocument[]>> AggregateAsync(params BsonDocument[] pipeline);

        Task<ResultReturn<BsonDocument[]>> AggregateAsync(BsonDocument[] pipeline, AggregateOptions options);

        /// <summary>
        /// 聚合查询,并返回分页后的结果
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IPagedList<BsonDocument>> AggregateWithPagedAsync(AggregateBuilder builder,
            int pageIndex, int pageSize, AggregateOptions options = null);

        Task<BsonDocument[]> DistinctAsync(string field, IMongoQuery query);


        Task<ResultReturn> ReplaceOneAsync(IMongoQuery query, BsonDocument newDocument,
            bool isupset = false);

        Task<ResultReturn<BulkWriteResult<BsonDocument>>> BulkWriteAsync(IEnumerable<WriteModel<BsonDocument>> opts, bool isordered = false);

        Task<ResultReturn<BulkWriteResult<BsonDocument>>> BulkWriteAsync(BulkWriteBuilder opts, bool isordered = false);

        Task<int> NewSequenceIDAsync();
    }

    public interface IMongoDBCollectionAsyncHelper<T> : IMongoDBCollectionAsyncHelper where T : class
    {
        Task<VM_PagedList<T>> FindAsWithPagedAsync(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null,
            int pageIndex = 1,
            int pageSize = 20, Collation collation = null);

        Task<VM_PagedList<TNew>> FindAsWithPagedAsync<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int pageIndex = 1, int pageSize = 20, Collation collation = null) where TNew : class;

        Task<TNew[]> FindAsAsync<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null,
            int skip = 0, int take = 0, Collation collation = null);

        Task<T[]> FindAsAsync(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0,
            int take = 0, Collation collation = null);

        Task<T> FindOneAsAsync(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null);

        Task<TNew> FindOneAsAsync<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null);


        Task<ResultReturn> InsertAsync(T data, bool isValidate = false);

        Task<ResultReturn> InsertBatchAsync(IEnumerable<T> data, bool isOrdered = false, bool isValidate = false);

        Task<T[]> FindAllAsAsync(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null);

        Task<TNew[]> FindAllAsAsync<TNew>(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null);

        Task<ResultReturn<T>> FindOneAndModifyAsAsync<T>(IMongoQuery query, IMongoUpdate update,
            IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false);

        Task<TNew> FindByIDAsAsync<TNew>(ObjectId id, IMongoFields fields = null);

        Task<T> FindByIDAsAsync(ObjectId id, IMongoFields fields = null);

        Task<TNew[]> FindByIDsAsAsync<TNew>(ObjectId[] ids, IMongoFields fields = null);

        Task<T[]> FindByIDsAsAsync(ObjectId[] ids, IMongoFields fields = null);

        Task<T[]> DistinctAsAsyncs(string field, IMongoQuery query);

        Task<TNew[]> DistinctAsAsync<TNew>(string field, IMongoQuery query) where TNew : class;

        Task<IPagedList<T>> AggregateAsWithPagedAsync(AggregateBuilder builder, int pageIndex, int pageSize, AggregateOptions options = null);

        Task<IPagedList<TNew>> AggregateAsWithPagedAsync<TNew>(AggregateBuilder builder, int pageIndex, int pageSize, AggregateOptions options=null) where TNew : class;

    }
}