using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using BridgeCare.Interfaces;
using BridgeCare.Models;
using BridgeCare.Properties;
using DatabaseManager;

namespace BridgeCare.DataAccessLayer
{
    public class RunRollupDAL : IRunRollup
    {
        public void SetLastRunDate(int networkId, BridgeCareContext db)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Creates/starts a rollup segmentation
        /// </summary>
        /// <param name="model">SimulationModel</param>
        /// <returns>string task</returns>
        public Task<string> RunRollup(SimulationModel model)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["BridgeCareContext"].ConnectionString;
                DBMgr.NativeConnectionParameters = new ConnectionParameters(connectionString, false, "MSSQL");

                var mongoConnection = "";
#if DEBUG
                mongoConnection = ConfigurationManager.ConnectionStrings["MongoDBDevConnectionString"].ConnectionString;
#else
                mongoConnection = ConfigurationManager.ConnectionStrings["MongoDBProdConnectionString"].ConnectionString;
#endif
                var rollupSegmentation = new RollupSegmentation.RollupSegmentation(model.networkName,
                    model.networkId.ToString(), true, mongoConnection)
                { strNetwork = model.networkName };

                var rollupAndSimulation = new Thread(rollupSegmentation.DoRollup);
                rollupAndSimulation.Start();

                return Task.FromResult("Rolling up...");
            }
            catch (Exception ex)
            {
                DBMgr.CloseConnection();
                return Task.FromResult($"Rollup failed::{ex.Message}");
            }
        }
    }
}
