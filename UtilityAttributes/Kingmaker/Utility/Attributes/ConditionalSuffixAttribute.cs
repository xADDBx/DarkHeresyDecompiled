using System;
using UnityEngine;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalSuffixAttribute : PropertyAttribute, IConditionSourceProvider
{
	public string Suffix { get; }

	public string ConditionSource { get; }

	public ConditionalSuffixAttribute(string suffix, string conditionSource)
	{
		Suffix = suffix;
		ConditionSource = conditionSource;
	}
}
