namespace AssetFox.Core.Common
{
    [Serializable]
    public class SimulationDataValidationException : Exception
    {
        public string Location { get; init;  }

        public SimulationDataValidationException(string message, string location) : base(message)
        {
            Location = location;
        }
    }
}
