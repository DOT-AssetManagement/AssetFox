using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Common;
using Moq;
using Moq.Language.Flow;

namespace AssetFox.Core.UnitTestsCore.Extensions
{
    public static class ISetupExtensions
    {
        public static IReturnsResult<T> ReturnsEmptyList<T, U>(this ISetup<T, List<U>> setup)
            where T : class
        {
            return setup.Returns(new List<U>());
        }

        public static IReturnsResult<T> ReturnsList<T, U>(this ISetup<T, List<U>> setup, params U[] listContents)
            where T : class
        {
            var list = new List<U>(listContents);
            return setup.Returns(list);
        }
    }
}
