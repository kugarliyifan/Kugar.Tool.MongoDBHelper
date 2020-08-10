using System;
using System.Threading.Tasks;

namespace Kugar.Tool.MongoDBHelper.AspNetCore
{
#if NETCOREAPP3_0 || NETCOREAPP3_1

    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using MongoDB.Bson;


    public class ObjectIDModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var isNullbale = bindingContext.ModelType == typeof(ObjectId?);

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            //if (valueProviderResult == ValueProviderResult.None)
            //{
            //    if (bindingContext.ModelType==typeof(ObjectId?))
            //    {
            //        bindingContext.ModelState.SetModelValue(bindingContext.ModelName,  valueProviderResult.);
            //    }
            //    return ;
            //}

            string firstValue = valueProviderResult.FirstValue;

            ObjectId value;

            if (string.IsNullOrWhiteSpace(firstValue) || valueProviderResult == ValueProviderResult.None)
            {
                value = ObjectId.Empty;

                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                if (isNullbale)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "参数值无效");
                    bindingContext.Result = ModelBindingResult.Success(null);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                }

            }
            else
            {
                if (!ObjectId.TryParse(firstValue, out value))
                {
                    if (!isNullbale)
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, "参数值无效");
                    }

                    value = ObjectId.Empty;

                    bindingContext.Result = ModelBindingResult.Failed();
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(value);
                }
            }


        }
    }

#endif
}
