using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kugar.Tool.MongoDBHelper
{
    public interface IMongoDBClientCollectionExecutor //: IMongoCollection<BsonDocument>
    {
        IEnumerable<BsonDocument> Aggregate(PipelineDefinition<BsonDocument, BsonDocument> pipeline,
            AggregateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<IEnumerable<BsonDocument>> AggregateAsync(PipelineDefinition<BsonDocument, BsonDocument> pipeline,
            AggregateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        BulkWriteResult<BsonDocument> BulkWrite(IEnumerable<WriteModel<BsonDocument>> requests,
            BulkWriteOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<BulkWriteResult<BsonDocument>> BulkWriteAsync(IEnumerable<WriteModel<BsonDocument>> requests,
            BulkWriteOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        long Count(FilterDefinition<BsonDocument> filter, CountOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());


        Task<long> CountAsync(FilterDefinition<BsonDocument> filter, CountOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        DeleteResult DeleteMany(FilterDefinition<BsonDocument> filter, DeleteOptions options,
            CancellationToken cancellationToken = new CancellationToken());

        Task<DeleteResult> DeleteManyAsync(FilterDefinition<BsonDocument> filter, DeleteOptions options,
            CancellationToken cancellationToken = new CancellationToken());
        
        DeleteResult DeleteOne(FilterDefinition<BsonDocument> filter, DeleteOptions options,
            CancellationToken cancellationToken = new CancellationToken());
        
        Task<DeleteResult> DeleteOneAsync(FilterDefinition<BsonDocument> filter, DeleteOptions options,
            CancellationToken cancellationToken = new CancellationToken());

        IEnumerable<TField> Distinct<TField>(FieldDefinition<BsonDocument, TField> field,
            FilterDefinition<BsonDocument> filter, DistinctOptions options = null,
            CancellationToken cancellationToken = new CancellationToken()) where TField : class
          ;

        Task<IEnumerable<TField>> DistinctAsync<TField>(FieldDefinition<BsonDocument, TField> field,
            FilterDefinition<BsonDocument> filter, DistinctOptions options = null,
            CancellationToken cancellationToken = new CancellationToken()) where TField : class;

        IEnumerable<TProjection> FindSync<TProjection>(FilterDefinition<BsonDocument> filter,
            FindOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<IEnumerable<TProjection>> FindAsync<TProjection>(FilterDefinition<BsonDocument> filter,
            FindOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());

        TProjection FindOneAndDelete<TProjection>(FilterDefinition<BsonDocument> filter,
            FindOneAndDeleteOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<BsonDocument> filter,
            FindOneAndDeleteOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());


        TProjection FindOneAndReplace<TProjection>(FilterDefinition<BsonDocument> filter, BsonDocument replacement,
            FindOneAndReplaceOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<BsonDocument> filter,
            BsonDocument replacement,
            FindOneAndReplaceOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());

        TProjection FindOneAndUpdate<TProjection>(FilterDefinition<BsonDocument> filter,
            UpdateDefinition<BsonDocument> update,
            FindOneAndUpdateOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<BsonDocument> filter,
            UpdateDefinition<BsonDocument> update,
            FindOneAndUpdateOptions<BsonDocument, TProjection> options = null,
            CancellationToken cancellationToken = new CancellationToken());

        void InsertOne(BsonDocument document, InsertOneOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task InsertOneAsync(BsonDocument document, InsertOneOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        void InsertMany(IEnumerable<BsonDocument> documents, InsertManyOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task InsertManyAsync(IEnumerable<BsonDocument> documents, InsertManyOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        ReplaceOneResult ReplaceOne(FilterDefinition<BsonDocument> filter, BsonDocument replacement,
            UpdateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<BsonDocument> filter, BsonDocument replacement,
            UpdateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        UpdateResult UpdateMany(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<UpdateResult> UpdateManyAsync(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        UpdateResult UpdateOne(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<UpdateResult> UpdateOneAsync(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<string> CreateIndexAsync(IndexKeysDefinition<BsonDocument> index, CreateIndexOptions options);
        
        
        IMongoCollection<BsonDocument> SourceCollection { get; }
    }
}
