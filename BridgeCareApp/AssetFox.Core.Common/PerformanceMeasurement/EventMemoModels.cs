using System;

namespace AssetFox.Core.Common.PerformanceMeasurement
{
    public static class EventMemoModels
    {
        public static EventMemoModel Now(string text)
            => new EventMemoModel
            {
                Text = text,
                UtcTime = DateTime.UtcNow
            };
    }
}
