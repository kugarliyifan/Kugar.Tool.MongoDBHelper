using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Kugar.Tool.MongoDBHelper
{
    public class DigitModelBinder: DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var value = (string)result.ConvertTo(typeof(string));

            if (string.IsNullOrWhiteSpace(value))
            {
                return new Digit(0);
            }
            else
            {
                return (Digit) value;
            }
        }
    }
}
