using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Mvc;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using IModelBinder = System.Web.Http.ModelBinding.IModelBinder;
using ModelBindingContext = System.Web.Http.ModelBinding.ModelBindingContext;

namespace Kugar.Tool.MongoDBHelper
{
    public class WebApiObjectIdModelBinder : IModelBinder
    {
        public static void Register(HttpConfiguration config)
        {
            var provider = new System.Web.Http.ModelBinding.Binders.SimpleModelBinderProvider(typeof(ObjectId), new WebApiObjectIdModelBinder());
            config.Services.Insert(typeof(System.Web.Http.ModelBinding.ModelBinderProvider), 0, provider);
            
            var provider2 = new System.Web.Http.ModelBinding.Binders.SimpleModelBinderProvider(typeof(ObjectId?), new WebApiObjectIdModelBinder());
            config.Services.Insert(typeof(ModelBinderProvider), 0, provider2);

        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (string.IsNullOrEmpty(bindingContext.ModelName))
            {
                return false;
            }

            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (result == null)
            {
                bindingContext.Model = ObjectId.Empty;
                return true;
                //return ObjectId.Empty;
            }

            var value = (string)result.ConvertTo(typeof(string));

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Model = ObjectId.Empty;
                return true;
            }

            ObjectId o;

            if (ObjectId.TryParse(value, out o))
            {
                bindingContext.Model = o;
                return true;
            }
            else
            {
                bindingContext.Model = ObjectId.Empty;
                return true;
            }
        }
    }

    public class WebApiObjectIdArrayModelBinder : IModelBinder
    {
        private static ObjectId[] _defaultValue=new ObjectId[0];

        public static void Register(HttpConfiguration config)
        {
            var provider1 = new System.Web.Http.ModelBinding.Binders.SimpleModelBinderProvider(typeof(ObjectId[]), new WebApiObjectIdArrayModelBinder());
            config.Services.Insert(typeof(ModelBinderProvider), 0, provider1);

        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (string.IsNullOrEmpty(bindingContext.ModelName))
            {
                return false;
            }

            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            IEnumerable<string> value = null;

            if (result.RawValue == null)
            {
                bindingContext.Model = _defaultValue;
                return true;
            }

            if (result.RawValue is string)
            {
                var tmp = (string)result.ConvertTo(typeof(string));

                value = tmp.Split(',');
            }
            else if (result.RawValue is IEnumerable<string>)
            {
                value = (string[])result.ConvertTo(typeof(string[]));
            }
            else if (result.RawValue is JArray)
            {
                var tmp = (JArray)result.RawValue;

                value = tmp.Select(x => x.ToStringEx()).ToArrayEx();
            }

            if (!value.HasData())
            {
                bindingContext.Model = _defaultValue;
                return true;
            }

            var lst = new List<ObjectId>(5);

            foreach (var s1 in value)
            {
                var o1 = s1.ToObjectId(ObjectId.Empty);

                if (!o1.IsEmpty())
                {
                    lst.Add(o1);
                }
            }

            bindingContext.Model = lst.ToArray();

            return true;
        }
    }


}
