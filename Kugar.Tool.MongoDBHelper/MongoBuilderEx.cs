using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;

namespace Kugar.Tool.MongoDBHelper
{
    public static class MongoBuilderEx
    {
        public static AggregateBuilder Aggregate=>new AggregateBuilder();

        public static IndexKeysBuilder Index=>new IndexKeysBuilder();
        
        public static ProjectBuilder Project=>new ProjectBuilder();

        public static BulkWriteBuilder BulkWrite => new BulkWriteBuilder();
    }
}
