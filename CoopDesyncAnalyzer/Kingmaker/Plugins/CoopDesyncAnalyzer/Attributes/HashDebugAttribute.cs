using System;

namespace Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
public sealed class HashDebugAttribute : Attribute
{
	public readonly string Name;

	public readonly bool ShouldProduceCommit;

	public HashDebugAttribute(string name, bool shouldProduceCommit = true)
	{
		Name = name;
		ShouldProduceCommit = shouldProduceCommit;
	}
}
