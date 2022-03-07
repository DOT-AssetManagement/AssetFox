namespace BridgeCareCore.Security
{
    public static class SecurityConstants
    {
        public static class SecurityTypes
        {
            public const string Esec = "ESEC";
            public const string B2C = "B2C";
        }

        public static class Policy
        {
            public const string Admin = "UserIsAdmin";
            public const string AdminOrDistrictEngineer = "UserIsAdminOrDistrictEngineer";
        }

        public static class Role
        {
            public const string BAMSAdmin = "PD-BAMS-Administrator";
            public const string BAMSCWOPA = "PD-BAMS-CWOPA";
            public const string BAMSPlanningPartner = "PD-BAMS-PlanningPartner";
            public const string BAMSDBEngineer = "PD-BAMS-DBEngineer";
        }
    }
}
