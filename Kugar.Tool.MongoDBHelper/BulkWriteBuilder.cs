using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kugar.Tool.MongoDBHelper
{
    public class BulkWriteBuilder
    {
        private List<WriteModel<BsonDocument>> _lst=new List<WriteModel<BsonDocument>>();

        public IEnumerable<WriteModel<BsonDocument>> Operations => _lst;

        public BulkWriteBuilder UpdateOne(IMongoQuery query, IMongoUpdate updater, bool isupset=false)
        {
            var q = new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx());
            var u = new BsonDocumentUpdateDefinition<BsonDocument>(updater.ToBsonDocumentEx());

            _lst.Add(new UpdateOneModel<BsonDocument>(q,u)
                     { IsUpsert = isupset });

            return this;
        }

        

        public BulkWriteBuilder UpdateMany(IMongoQuery query, IMongoUpdate updater, bool isupset=false)
        {
            var q = new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx());
            var u = new BsonDocumentUpdateDefinition<BsonDocument>(updater.ToBsonDocumentEx());

            _lst.Add(new UpdateManyModel<BsonDocument>(q, u)
                     { IsUpsert = isupset });

            return this;
        }

        public BulkWriteBuilder ReplaceOne(IMongoQuery query, BsonDocument newItem, bool isupset=false)
        {
            var q = new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx());

            _lst.Add(new ReplaceOneModel<BsonDocument>(q,newItem)
                     {
                         IsUpsert = isupset
                     });

            return this;
        }

        public BulkWriteBuilder InsertOne(BsonDocument newItem)
        {
            _lst.Add(new InsertOneModel<BsonDocument>(newItem));

            return this;
        }

        public BulkWriteBuilder DeleteOne(IMongoQuery query)
        {
            var q = new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx());

            _lst.Add(new DeleteOneModel<BsonDocument>(q));

            return this;
        }

        public BulkWriteBuilder DeleteMany(IMongoQuery query)
        {
            var q = new BsonDocumentFilterDefinition<BsonDocument>(query.ToBsonDocumentEx());

            _lst.Add(new DeleteManyModel<BsonDocument>(q));

            return this;
        }

        public static implicit operator WriteModel<BsonDocument>[] (BulkWriteBuilder source)
        {
            return source._lst.ToArray();
        }
    }
}