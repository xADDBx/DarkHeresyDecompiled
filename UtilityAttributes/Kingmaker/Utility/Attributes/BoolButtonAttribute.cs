using System;
using UnityEngine;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class BoolButtonAttribute : PropertyAttribute
{
	public readonly string MethodName;

	public readonly string Label;

	public BoolButtonAttribute(string methodName, string label = null)
	{
		MethodName = methodName;
		Label = label ?? methodName;
	}
}
