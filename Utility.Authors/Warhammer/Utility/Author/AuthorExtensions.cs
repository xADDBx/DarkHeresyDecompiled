using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Warhammer.Utility.Author;

public static class AuthorExtensions
{
	public static string GetAuthorEmail(Enum value)
	{
		FieldInfo field = value.GetType().GetField(value.ToString());
		if (field != null && field.GetCustomAttributes(typeof(DescriptionEmail), inherit: false) is DescriptionEmail[] source && source.Any())
		{
			return source.First().Description;
		}
		return value.ToString();
	}

	public static string GetAuthorInspectorName(Enum value)
	{
		FieldInfo field = value.GetType().GetField(value.ToString());
		if (field != null && field.GetCustomAttributes(typeof(InspectorNameAttribute), inherit: false) is InspectorNameAttribute[] source && source.Any())
		{
			return source.First().displayName;
		}
		return value.ToString();
	}
}
