using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace AssetFoxCoreTests.Tests.Integration
{
    public static class FormCollectionMockExtensions
    {
        public static void SetupGetValue(this Mock<IFormCollection> mock, string key, string value)
        {
            //https://stackoverflow.com/questions/1068095/assigning-out-ref-parameters-in-moq
            mock.Setup(m => m.TryGetValue(key, out It.Ref<StringValues>.IsAny))
                .Returns((string idString, ref StringValues output)
                =>
                {
                    output = new StringValues(value);
                    return true;
                });
        }
    }
}
