using System;

namespace AssetFox.Core.DTOs.Abstract
{
    /// <summary>
    /// A base DTO class that all DTOs derive from.
    /// </summary>
    public abstract class BaseDTO
    {
        /// <summary>
        /// The ID of the DTO.
        /// </summary>
        public Guid Id { get; set; }
    }
}
