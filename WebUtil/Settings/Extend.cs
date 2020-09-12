using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebUtil.Settings
{
    public static class Extend
    {
        public static MongoDbSettings GetMongoDbSettings(this IConfiguration configuration)
        {
            var databaseSection = configuration.GetSection("MongoDb");
            return databaseSection.Get<MongoDbSettings>();
        }
    }
}
