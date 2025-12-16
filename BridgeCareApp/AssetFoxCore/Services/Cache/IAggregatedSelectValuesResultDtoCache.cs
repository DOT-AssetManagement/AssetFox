using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{
    public interface IAggregatedSelectValuesResultDtoCache
    {
        List<string> AttributesTooBigToCache { get; }

        void ClearInvalid();
        void SaveToCache(AggregatedSelectValuesResultDTO dto);
        AggregatedSelectValuesResultDTO TryGetCachedValue(string attributeName);
    }
}
