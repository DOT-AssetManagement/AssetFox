using AppliedResearchAssociates.iAM.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BridgeCareCoreTests.Helpers
{
    public static class ActionResultAssertions
    {
        public static object OkObject(IActionResult result)
        {
            Assert.IsType<OkObjectResult>(result);
            var castResult = (OkObjectResult)result;
            return castResult.Value;
        }

        /// <summary>Asserts that the result is an OkObjectResult with a value of type T. Returns its value.</summary> 
        public static T OkObject<T>(IActionResult result)
        {
            var value = OkObject(result);
            var t = (T)value;
            return t;
        }

        public static void Ok(IActionResult result)
        {
            Assert.IsType<OkResult>(result);
        }

        public static void Singleton<T>(T expected, IActionResult result)
        {
            var ok = OkObject(result);
            ObjectAssertions.Singleton(expected, ok);
        }

        public static void BadRequest(IActionResult result)
        {
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
