using System;

namespace AssetFox.Core.Analysis.Engine;

public class SimulationLogEventArgs: EventArgs
{
    public SimulationLogEventArgs(SimulationLogMessageBuilder messageBuilder)
    {
        MessageBuilder = messageBuilder;
    }
    public SimulationLogMessageBuilder MessageBuilder { get; set; }
}
