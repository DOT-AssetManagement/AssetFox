using System;

namespace AssetFox.Core.Analysis;

public sealed record ConsequenceApplicator(Attribute Target, Action Change, double? NewValue);
