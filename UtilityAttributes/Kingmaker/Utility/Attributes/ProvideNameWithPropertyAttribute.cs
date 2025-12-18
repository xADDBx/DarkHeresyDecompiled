using System;
using UnityEngine;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ProvideNameWithPropertyAttribute : PropertyAttribute
{
	public readonly string PropertyPath;

	public ProvideNameWithPropertyAttribute(string relativePropertyPath)
	{
		PropertyPath = relativePropertyPath;
	}
}
