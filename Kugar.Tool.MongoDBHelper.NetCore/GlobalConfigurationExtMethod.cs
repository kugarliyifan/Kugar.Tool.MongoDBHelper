using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper
{
    public static class GlobalConfigurationExtMethod
    {
        public static void UseMongoDBHelper(this IServiceCollection services)
        {
            MongoDBContextHelper.RegisterSerializers();
        }
    }
}
