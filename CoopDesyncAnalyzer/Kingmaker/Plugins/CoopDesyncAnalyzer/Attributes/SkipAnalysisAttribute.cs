using System;

namespace Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Interface, Inherited = false)]
public sealed class SkipAnalysisAttribute : Attribute
{
	private const AttributeTargets ValidTargets = AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Interface;
}
