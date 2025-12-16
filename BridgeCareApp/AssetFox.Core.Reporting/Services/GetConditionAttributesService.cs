using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;

namespace AssetFoxCore.Services
{
    public class GetConditionAttributesService
    {
        private static IUnitOfWork _unitOfWork;

        public GetConditionAttributesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        }
        public static List<string> GetConditionAttributes(Guid simulationId, IUnitOfWork unitOfWork)
        {
            var performanceCurveRepo = unitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var conditionAttributes = new List<string>();

            foreach (var curve in performanceCurveRepo)
            {
                // Get each attribute
                var attributeValue = curve.Attribute;

                // Add each attribute to the conditionAttributes list
                conditionAttributes.Add(attributeValue);
            }
            return conditionAttributes;
        }
    }
}
