using System.Collections.Generic;

namespace AssetFoxCore.Models
{
    public class UserInfo
    {
        /// <summary>
        ///     The stable, unique identifier used as the user's DB Username. For Entra this is the
        ///     email (with preferred_username / oid fallbacks) rather than the display name, so that
        ///     two people who share a display name (e.g. two "Jeff"s) are kept as distinct accounts.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The human-readable display name from the token's "name" claim. Not unique, so it is
        ///     never used as an identity key; it is retained only for display and for the one-time
        ///     migration of legacy rows that were keyed on the display name.
        /// </summary>
        public string DisplayName { get; set; }

        public bool HasAdminAccess { get; set; }

        // Below is for SimulationPowerUser
        public bool HasSimulationAccess { get; set; }

        public string Email { get; set; }
    }
}
