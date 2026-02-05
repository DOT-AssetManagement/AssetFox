using AssetFox.Validation;
using System;

namespace AssetFoxCore.Models.Validation
{
    public sealed class PreChecksValidationResult
    {
        public PreChecksValidationResult(
            ValidationStatus status,
            string message,
            string location)
        {
            Status = status;

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Description is blank.", nameof(message));
            }

            Message = message;
            Location = location;
        }

        public string Message { get; }

        public ValidationStatus Status { get; }

        // TODO and add from UI too where some msgs getting added to pre-check results...
        public string Location { get; }        
    }
}
