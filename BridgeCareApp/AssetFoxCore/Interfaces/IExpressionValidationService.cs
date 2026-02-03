using System;
using AssetFox.Core.DTOs;
using AssetFoxCore.Models.Validation;

namespace AssetFoxCore.Interfaces
{
    public interface IExpressionValidationService
    {
        ValidationResult ValidateEquation(EquationValidationParameters model);

        CriterionValidationResult ValidateCriterion(string mergedCriteriaExpression,
            UserCriteriaDTO currentUserCriteriaFilter, Guid networkId);

        CriterionValidationResult ValidateCriterionWithoutResults(string mergedCriteriaExpression,
            UserCriteriaDTO currentUserCriteriaFilter);
    }
}
