using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class UnitOfWorkExtensionsTests
    {
        [Fact]
        public void NestedAsTransaction_Throws()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                TestHelper.UnitOfWork.AsTransaction(() =>
                {
                    TestHelper.UnitOfWork.AsTransaction(() => { });
                }));

            Assert.Equal(UnitOfDataPersistenceWorkExtensions.CannotStartTransactionWhileAnotherTransactionIsInProgress, exception.Message);
        }
    }
}
