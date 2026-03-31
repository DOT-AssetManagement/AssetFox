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
                
        public string Location { get; }        
    }
}
