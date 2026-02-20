using System;
using System.Collections.Generic;

namespace AssetFox.Validation
{
    public sealed class ValidationResult
    {
        public ValidationResult(
            ValidationTarget target,
            ValidationStatus status,
            string message,
            string location
            )
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
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

        public ValidationTarget Target { get; }

        public override bool Equals(object obj) => obj is ValidationResult result && EqualityComparer<ValidationTarget>.Default.Equals(Target, result.Target) && Status == result.Status && Message == result.Message && Location == result.Location;

        public override int GetHashCode() => HashCode.Combine(Target, Status, Message);
    }
}
