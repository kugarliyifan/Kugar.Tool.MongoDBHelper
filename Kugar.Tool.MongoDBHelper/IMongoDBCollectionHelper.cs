using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using MongoDB.Bson;
using MongoDB.Driver;
#if NETCOREAPP
using Task = System.Threading.Tasks.ValueTask;

#endif

namespace Kugar.Tool.MongoDBHelper
{
    /// <summary>
    /// 容器处理类
    /// </summary>
    public interface IMongoDBSyncCollectionHelper
    {
        /// <summary>
        /// 按分页方式返回数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <param name="fields"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        VM_PagedList<BsonDocument> FindWithPaged(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int pageIndex = 1, int pageSize = 20, Collation collation = null);

        /// <summary>
        /// 按分页方式返回数据,并且直接回调数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <param name="fields"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        void FindWithPagedAndCallback(IMongoQuery query, Action<BsonDocument[]> callback, IMongoSortBy order = null, IMongoFields fields = null, int pageSize = 20, Collation collation = null);

        /// <summary>
        /// 按指定方式返回查询结果
        /// </summary>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <param name="fields"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        BsonDocument[] Find(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = 0, Collation collation = null);

        /// <summary>
        /// 返回单个查询结果
        /// </summary>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        BsonDocument FindOne(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, Collation collation = null);

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
        ResultReturn<BsonDocument> FindAndModify(IMongoQuery query, IMongoUpdate update, IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false);

        ObjectId FindOneID(IMongoQuery query, IMongoSortBy order = null);


        BsonDocument[] FindAll(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null);

        /// <summary>
        /// 聚合索引查询
        /// </summary>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        ResultReturn<BsonDocument[]> Aggregate(params BsonDocument[] pipeline);

        ResultReturn<BsonDocument[]> Aggregate(AggregateArgs pipeline);
        
        BsonValue Sum(IMongoQuery query, string sumFieldName, BsonValue defaultValue);

        /// <summary>
        /// 插入一个数据
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        ResultReturn Insert(BsonDocument document);

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        ResultReturn InsertBatch(IEnumerable<BsonDocument> document, bool isOrdered = false);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        ResultReturn Delete(IMongoQuery query, RemoveFlags flags = RemoveFlags.None);

        /// <summary>
        /// 测试指定查询是否存在数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        bool Exists(IMongoQuery query);
        
        /// <summary>
        /// 返回指定查询结果集的记录总数
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ResultReturn<int> Count(IMongoQuery query);

        /// <summary>
        /// 计算根据指定条件下查询结果的总页数
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetQueryPageCount(IMongoQuery query, int pageSize);

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <param name="query"></param>
        /// <param name="updater"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        ResultReturn Update(IMongoQuery query, IMongoUpdate updater, UpdateFlags flags = UpdateFlags.None);

        /// <summary>
        /// 更新操作,按_id字段更新
        /// </summary>
        /// <param name="query"></param>
        /// <param name="updater"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        ResultReturn UpdateByID(ObjectId id, IMongoUpdate updater, UpdateFlags flags = UpdateFlags.None);

        /// <summary>
        /// 更新操作,按_id字段更新
        /// </summary>
        /// <param name="query"></param>
        /// <param name="updater"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        Task<ResultReturn> UpdateByIDAsync(ObjectId id, IMongoUpdate updater, UpdateFlags flags = UpdateFlags.None);

        /// <summary>
        /// 返回当前容器的自增量值
        /// </summary>
        /// <returns></returns>
        int NewSequenceID();

        ObjectId[] FindIDs(IMongoQuery query, IMongoSortBy orderBy = null);

        ObjectId FindID(IMongoQuery query, IMongoSortBy orderBy = null);

        BsonDocument[] FindByIDs(ObjectId[] ids, IMongoFields fields = null);

        BsonDocument FindByID(ObjectId id, IMongoFields fields = null);

        /// <summary>
        /// 获取指定查询条件下,第一个符合条件的文档中,指定名称的字段值(获取单个字段值)
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="order">排序方式</param>
        /// <param name="defaultValue">默认返回值</param>
        /// <returns></returns>
        BsonValue FindOneWithSigleField(IMongoQuery query, string fieldName, IMongoSortBy order = null,
            BsonValue defaultValue = null);

        /// <summary>
        /// 获取指定查询条件下,第一个符合条件的文档中,指定名称的字段值(获取单个字段值)
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="order">排序方式</param>
        /// <param name="defaultValue">默认返回值</param>
        /// <returns></returns>
        TValue FindOneWithSigleField<TValue>(IMongoQuery query, string fieldName,
            IMongoSortBy order = null, TValue defaultValue = default(TValue));

        ///// <summary>
        ///// 创建一个索引
        ///// </summary>
        ///// <param name="index"></param>
        ///// <param name="options"></param>
        ///// <returns></returns>
        //string CreateIndex(IndexKey key, CreateIndexOptions options = null);

        BsonDocument[] Distinct(string field, IMongoQuery query);
        
        ResultReturn ReplaceOne(IMongoQuery query, BsonDocument newDocument, bool isupset = false);

        ResultReturn<BulkWriteResult<BsonDocument>> BulkWrite(IEnumerable<WriteModel<BsonDocument>> opts, bool isordered = false);

        ResultReturn<BulkWriteResult<BsonDocument>> BulkWrite(BulkWriteBuilder builder, bool isordered = false);


        IMongoDBClientCollectionExecutor SourceCollection { get; }

        IMongoDBContext Context { get; }
    }


    public interface IMongoDBSyncCollectionHelper<T> : IMongoDBCollectionHelper where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <param name="fields"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        VM_PagedList<T> FindAsWithPaged(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int pageIndex = 1, int pageSize = 20, Collation collation = null);

        VM_PagedList<TNew> FindAsWithPaged<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int pageIndex = 1, int pageSize = 20, Collation collation = null) where TNew : class;

        /// <summary>
        /// 按分页方式返回数据,并且直接回调数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <param name="fields"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        void FindAsWithPagedAndCallback(IMongoQuery query, Action<T[]> callback, IMongoSortBy order = null, IMongoFields fields = null, int pageSize = 20, Collation collation = null);

        T[] FindAs(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = 0, Collation collation = null);

        TNew[] FindAs<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null, int skip = 0, int take = 0, Collation collation = null);

        

        T FindOneAs(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null);

        TNew FindOneAs<TNew>(IMongoQuery query, IMongoSortBy order = null, IMongoFields fields = null);

        
        ResultReturn<T> FindAndModifyAs(IMongoQuery query, IMongoUpdate update, IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false);

        ResultReturn<TNew> FindAndModifyAs<TNew>(IMongoQuery query, IMongoUpdate update, IMongoSortBy sortBy = null, IMongoFields fields = null,
            bool returnNew = true, bool upsert = false);

        ResultReturn Insert(T data,bool isValidate=false);

        ResultReturn InsertBatch(IEnumerable<T> data, bool isOrdered = false, bool isValidate = false);

        T[] FindAllAs(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null);

        TNew[] FindAllAs<TNew>(IMongoSortBy sortBy = null, IMongoFields fields = null, Collation collation = null);

        

        TNew FindByIDAs<TNew>(ObjectId id, IMongoFields fields = null);

        T[] FindByIDsAs(ObjectId[] ids, IMongoFields fields = null);

        TNew[] FindByIDsAs<TNew>(ObjectId[] ids, IMongoFields fields = null);

        

        T FindByIDAs(ObjectId id, IMongoFields fields = null);

        T[] DistinctAs(string field, IMongoQuery query);

        TNew[] DistinctAs<TNew>(string field, IMongoQuery query) where TNew : class;



        
    }

    public interface IMongoDBCollectionHelper : IMongoDBSyncCollectionHelper, IMongoDBCollectionAsyncHelper { }

    public interface IMongoDBCollectionHelper<T> : IMongoDBCollectionHelper, IMongoDBSyncCollectionHelper<T>,
        IMongoDBCollectionAsyncHelper<T> where T : class
    { }
}
