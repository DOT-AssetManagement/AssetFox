using System.Threading.Tasks;
using AssetFoxCore.Models.DefaultData;

namespace AssetFoxCore.Interfaces.DefaultData
{
    public interface IInvestmentDefaultDataService
    {
        Task<InvestmentDefaultData> GetInvestmentDefaultData();
    }
}
