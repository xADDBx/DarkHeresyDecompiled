using System;
using System.Collections.Generic;
using System.Reflection;

namespace Owlcat.UI;

public class ViewFactoryPolicy : Attribute
{
	public static readonly ViewFactoryPolicy Default = new ViewFactoryPolicy();

	private static readonly Dictionary<Type, ViewFactoryPolicy> sCache = new Dictionary<Type, ViewFactoryPolicy>();

	public ViewFactoryPolicyFlag Flags { get; }

	public string Path { get; }

	public static ViewFactoryPolicy GetCustomAttribute(Type type)
	{
		if (!sCache.TryGetValue(type, out var value))
		{
			Dictionary<Type, ViewFactoryPolicy> dictionary = sCache;
			ViewFactoryPolicy obj = type.GetCustomAttribute<ViewFactoryPolicy>() ?? Default;
			value = obj;
			dictionary[type] = obj;
		}
		return value;
	}

	public ViewFactoryPolicy(ViewFactoryPolicyFlag flags = ViewFactoryPolicyFlag.None, string path = null)
	{
		Flags = flags;
		Path = path;
	}
}
