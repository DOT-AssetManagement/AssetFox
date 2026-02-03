using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore
{
    public static class UserInfoDtos
    {
        public static UserInfoDTO ForUser(UserDTO user)
        {
            var admin = user.HasInventoryAccess;
            var dto = new UserInfoDTO
            {
                HasAdminAccess = admin,
                HasSimulationAccess = admin,
                Sub = user.Username,
            };
            return dto;
        }
    }
}
