using System;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models.Validation;
using AssetFoxCore.Security.Interfaces;
using AssetFox.Core.Hubs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AssetFox.CalculateEvaluate;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpressionValidationController : AssetFoxCoreBaseController
    {
        public const string ExpressionValidationError = "Expression Validation Error";

        private readonly IExpressionValidationService _expressionValidationService;

        public ExpressionValidationController(IExpressionValidationService expressionValidationService,
            IEsecSecurity esecSecurity, IUnitOfWork unitOfWork, IHubService hubService,
            IHttpContextAccessor httpContextAccessor) : base(esecSecurity, unitOfWork, hubService, httpContextAccessor) =>
            _expressionValidationService = expressionValidationService ??
                                           throw new ArgumentNullException(nameof(expressionValidationService));

        [HttpPost]
        [Route("GetEquationValidationResult")]
        [Authorize]
        public async Task<IActionResult> GetEquationValidationResult([FromBody] EquationValidationParameters model)
        {
            try
            {
                var result = await Task.Factory.StartNew(() => _expressionValidationService.ValidateEquation(model));
                return Ok(result);
            }
            catch (Exception e)
            {
                if (e is CalculateEvaluateException)
                {
                    HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{ExpressionValidationError}::GetEquationValidationResult - {e.Message}", e);
                } else
                {
                    HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{ExpressionValidationError}::GetEquationValidationResult - {e.Message}", e);
                }
            }
            return Ok();
        }

        [HttpPost]
        [Route("GetCriterionValidationResult")]
        [Authorize]
        public async Task<IActionResult> GetCriterionValidationResult([FromBody] ValidationParameter model)
        {
            try
            {
                var result = await Task.Factory.StartNew(() =>
                    _expressionValidationService.ValidateCriterion(model.Expression, model.CurrentUserCriteriaFilter, model.NetworkId));
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{ExpressionValidationError}::GetCriterionValidationResult - {e.Message}", e);
            }
            return Ok();
        }

        [HttpPost]
        [Route("GetCriterionValidationResultNoCount")]
        [Authorize]
        public async Task<IActionResult> GetCriterionValidationResultNoCount([FromBody] ValidationParameter model)
        {
            try
            {
                var result = await Task.Factory.StartNew(() =>
                    _expressionValidationService.ValidateCriterionWithoutResults(model.Expression, model.CurrentUserCriteriaFilter));
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{ExpressionValidationError}::GetCriterionValidationResultNoCount - {e.Message}", e);
            }
            return Ok();
        }
    }
}
