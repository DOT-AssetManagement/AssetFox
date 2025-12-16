using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.UnitTestsCore.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace AssetFoxCoreTests.Helpers
{
    public static class HttpContextSetup
    {
        public static void AddAuthorizationHeader(DefaultHttpContext context) =>
            context.Request.Headers.Add("Authorization", "Bearer abc123");

        public static DefaultHttpContext WithAuthorizationHeader(Dictionary<string, StringValues> queryStore = null)
        {
            var context = new DefaultHttpContext();
            AddAuthorizationHeader(context);
            if (queryStore != null)
            {
                context.Request.Query = new QueryCollection(queryStore);
                context.Request.ContentLength = 2;
                string[] body = new string[] { TestAttributeNames.BrKey, "10" };
                var serializeBody = JsonConvert.SerializeObject(body);
                var bytes = Encoding.UTF8.GetBytes(serializeBody);
                context.Request.Body = new MemoryStream(bytes);
            }
            return context;
        }

        public static DefaultHttpContext WithFormCollection(IFormCollection requestFormCollection)
        {
            var context = new DefaultHttpContext();
            context.Request.Form = requestFormCollection;
            return context;
        }
    }
}
