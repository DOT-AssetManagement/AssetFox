using System;

namespace AssetFox.Core.Analysis.Engine;

public sealed record RollForwardEventDetail(int Year, Guid AssetId, string AssetName, string CommittedProject);
