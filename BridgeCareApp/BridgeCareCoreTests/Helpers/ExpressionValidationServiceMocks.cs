using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models.Validation;
using Moq;

namespace AssetFoxCoreTests.Helpers
{
    public static class ExpressionValidationServiceMocks
    {
        public static Mock<IExpressionValidationService> New()
        {
            var mock = new Mock<IExpressionValidationService>();
            return mock;
        }

        public static void SetupValidateAnyCriterionWithoutResults(this Mock<IExpressionValidationService> mock, bool isValid)
        {
            mock.Setup(m => m.ValidateCriterionWithoutResults(It.IsAny<string>(), It.IsAny<UserCriteriaDTO>())).Returns(new CriterionValidationResult { IsValid = isValid });
        }

        public static void SetupValidateAnyEquation(this Mock<IExpressionValidationService> mock, bool valid)
        {
            mock.Setup(m => m.ValidateEquation(It.IsAny<EquationValidationParameters>())).Returns(new ValidationResult { IsValid = valid });
        }

        public static Mock<IExpressionValidationService> EverythingIsValid()
        {
            var mock = New();
            mock.SetupValidateAnyCriterionWithoutResults(true);
            mock.SetupValidateAnyEquation(true);
            return mock;
        }
    }
}
