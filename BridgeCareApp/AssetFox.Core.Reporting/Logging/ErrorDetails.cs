using System.Text.Json;

namespace AssetFox.Core.Reporting.Logging
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
