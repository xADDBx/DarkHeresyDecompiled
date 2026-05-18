using System;
using UnityEngine;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalInfoBoxAttribute : PropertyAttribute, IConditionSourceProvider
{
	public string ConditionSource { get; set; }

	public string Text { get; }

	public ConditionalInfoBoxAttribute(string conditionSource, string text)
	{
		ConditionSource = conditionSource;
		Text = text;
	}
}
