using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.ModelBinding;
using MongoDB.Bson;
using MongoDB.Driver;
using ModelBinderProvider = System.Web.Http.ModelBinding.ModelBinderProvider;
using ModelBindingContext = System.Web.Http.ModelBinding.ModelBindingContext;
using SimpleModelBinderProvider = System.Web.Http.ModelBinding.Binders.SimpleModelBinderProvider;

namespace Kugar.Tool.MongoDBHelper
{
    public class WebApiBsondocumentModelBinder: System.Web.Http.ModelBinding.IModelBinder
    {
        private Type _binderType = null;

        public WebApiBsondocumentModelBinder(Type type)
        {
            _binderType = type;
        }
        

        public static void Register(ServicesContainer servicesContainer)
        {
            insertToContainer(servicesContainer, typeof(IMongoQuery));
            insertToContainer(servicesContainer, typeof(IMongoUpdate));
            insertToContainer(servicesContainer, typeof(IMongoFields));
            insertToContainer(servicesContainer, typeof(IMongoSortBy));
            insertToContainer(servicesContainer, typeof(BsonDocument));
            insertToContainer(servicesContainer, typeof(AggregateArgs));
        }

        private static void insertToContainer(ServicesContainer servicesContainer, Type type)
        {
            var provider2 = new SimpleModelBinderProvider(type, new WebApiBsondocumentModelBinder(type));
            servicesContainer.Insert(typeof(ModelBinderProvider), 0, provider2);
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);


            if (result == null)
            {
                bindingContext.Model = null;

                return true;
            }

            if (_binderType == typeof(IMongoQuery))
            {
                var value = (QueryDocument)result.ConvertTo(typeof(QueryDocument));

                if (value == null || !value.HasData())
                {
                    bindingContext.Model = null;

                    return true;
                }

                bindingContext.Model = (IMongoQuery)value;

                return true;
            }
            else if (_binderType == typeof(IMongoUpdate))
            {
                var value = (UpdateDocument)result.ConvertTo(typeof(UpdateDocument));

                if (value == null || !value.HasData())
                {
                    bindingContext.Model = null;

                    return true;
                }

                bindingContext.Model = (IMongoUpdate)value;

                return true;
            }
            else if (_binderType == typeof(IMongoFields))
            {
                var value = (FieldsDocument)result.ConvertTo(typeof(FieldsDocument));

                if (value == null || !value.HasData())
                {
                    bindingContext.Model = null;

                    return true;
                }

                bindingContext.Model = (IMongoFields)value;

                return true;
            }
            else if (_binderType == typeof(IMongoSortBy))
            {
                var value = (SortByDocument)result.ConvertTo(typeof(SortByDocument));

                if (value == null || !value.HasData())
                {
                    bindingContext.Model = null;

                    return true;
                }

                bindingContext.Model = (IMongoSortBy)value;

                return true;
            }
            else if (_binderType == typeof(BsonDocument))
            {
                var value = (BsonDocument)result.ConvertTo(typeof(BsonDocument));

                if (value == null || !value.HasData())
                {
                    bindingContext.Model = null;

                    return true;
                }

                bindingContext.Model = value;

                return true;
            }
            else if (_binderType == typeof(AggregateArgs))
            {
                var value = (AggregateArgs)result.ConvertTo(typeof(AggregateArgs));

                if (value == null)
                {
                    bindingContext.Model = null;

                    return true;
                }

                bindingContext.Model = value;

                return true;
            }

            return false;
        }
    }
}
