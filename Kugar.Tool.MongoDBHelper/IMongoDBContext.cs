using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kugar.Tool.MongoDBHelper
{
    public interface IMongoDBContext
    {
        /// <summary>
        /// 获取一个强类型的容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">容器名称</param>
        /// <param name="isForceNew">是否强制创建一个新的容器对象</param>
        /// <param name="settings">创建容器的其他设置项</param>
        /// <returns></returns>
        IMongoDBCollectionHelper<T> GetCollection<T>(string name="",bool isForceNew = false, MongoCollectionSettings settings = null) where T : class;

        /// <summary>
        /// 获取一个容器对象
        /// </summary>
        /// <param name="name">容器名称</param>
        /// <param name="isForceNew">是否强制创建一个新的容器对象</param>
        /// <param name="isJournal"></param>
        /// <returns></returns>
        IMongoDBCollectionHelper GetCollection(string name, bool isForceNew = false, bool isJournal = false, int wlevel = 1);


        /// <summary>
        /// 在数据库中添加一个临时数据,可设置过期时间
        /// </summary>
        /// <param name="templateDataKey">临时数据key,如果出现key重复,则只获取最新一条该key的数据</param>
        /// <param name="data">临时数据</param>
        /// <param name="expireDt">过期时间,为空则不过期</param>
        void CreateTemplateData(string templateDataKey, BsonValue data, DateTime? expireDt = null);

        /// <summary>
        /// 在数据库中添加一个临时数据,可设置过期时间,并且强制key必须为唯一值,如果重复,则抛出错误
        /// </summary>
        /// <param name="templateDataKey">临时数据key,如果出现key重复,则将抛出错误</param>
        /// <param name="data">临时数据</param>
        /// <param name="expireDt">过期时间,为空则不过期</param>
        void CreateUnqieTemplateKeyData(string templateDataKey, BsonValue data, DateTime? expireDt = null);

        /// <summary>
        /// 获取临时数据
        /// </summary>
        /// <param name="templateDataKey"></param>
        /// <returns></returns>
        BsonValue GetTemplateKeyData(string templateDataKey);

        int NewSequenceID(string ModuleID);

        Version ServerVersion { get; }
    }
}
