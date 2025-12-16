using AssetFox.Core.Analysis.Engine;
using Newtonsoft.Json;

namespace AssetFox.Core.Analysis.Testing;

public class SimulationOutputTests
{
    [Fact]
    public void Deserialization() => _ = JsonConvert.DeserializeObject<SimulationOutput>(Properties.Resources.Network_13___Simulation_1181_json);
}
