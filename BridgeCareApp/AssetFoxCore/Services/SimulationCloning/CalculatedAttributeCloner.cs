using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFoxCore.Services
{
    internal class CalculatedAttributeCloner
    {
        internal static CalculatedAttributeDTO Clone(CalculatedAttributeDTO calculatedAttribute, Guid ownerId)
        {
            var cloneEquations =  CalculatedAttributeEquationCriteriaPairCloner.CloneList(calculatedAttribute.Equations, ownerId);
            var clone = new CalculatedAttributeDTO            
            {
                Id = Guid.NewGuid(),
                LibraryId = calculatedAttribute.LibraryId,
                Attribute = calculatedAttribute.Attribute,
                CalculationTiming = calculatedAttribute.CalculationTiming,
                Equations = cloneEquations,
                IsModified = calculatedAttribute.IsModified,
            };
            return clone;
        }
        internal static List<CalculatedAttributeDTO> CloneList(IEnumerable<CalculatedAttributeDTO> calculatedAttributes, Guid ownerId)
        {
            var clone = new List<CalculatedAttributeDTO>();
            foreach (var calculatedAttribute in calculatedAttributes)
            {
                var childClone = Clone(calculatedAttribute, ownerId);
                clone.Add(childClone);
            }
            return clone;

        }
    }
}
