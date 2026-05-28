using System;

namespace Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
public sealed class HashCommitAttribute : Attribute
{
}
