using System;
using System.Collections.Generic;
using System.Reflection;

namespace Owlcat.UI;

public static class IBindableExtensions
{
	private static readonly Dictionary<(Type, Type), bool> sIsBindableForCache = new Dictionary<(Type, Type), bool>();

	private static readonly Dictionary<(Type, Type), (bool result, MethodInfo methodInfo)> sCanBindToCache = new Dictionary<(Type, Type), (bool, MethodInfo)>();

	public static bool IsBindableFrom(this Type viewType, Type dataType)
	{
		(Type, Type) key = (viewType, dataType);
		if (sIsBindableForCache.TryGetValue(key, out var value))
		{
			return value;
		}
		return sIsBindableForCache[key] = IsBindableFromImpl(viewType, dataType);
	}

	private static bool IsBindableFromImpl(Type type, Type source)
	{
		Type[] interfaces = type.GetInterfaces();
		foreach (Type type2 in interfaces)
		{
			if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IBindable<>) && source.IsAssignableFrom(type2.GetGenericArguments()[0]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanBindTo(this Type viewType, Type dataType, out MethodInfo bindMethodInfo)
	{
		(Type, Type) key = (viewType, dataType);
		if (sCanBindToCache.TryGetValue(key, out (bool, MethodInfo) value))
		{
			bindMethodInfo = value.Item2;
			return value.Item1;
		}
		sCanBindToCache[key] = (CanBindToImpl(viewType, dataType, out bindMethodInfo), bindMethodInfo);
		return sCanBindToCache[key].result;
	}

	private static bool CanBindToImpl(Type type, Type source, out MethodInfo bindMethodInfo)
	{
		Type[] interfaces = type.GetInterfaces();
		foreach (Type type2 in interfaces)
		{
			if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IBindable<>) && type2.GetGenericArguments()[0].IsAssignableFrom(source))
			{
				bindMethodInfo = type2.GetMethod("Bind");
				return true;
			}
		}
		bindMethodInfo = null;
		return false;
	}

	public static void BindDynamic(this IBindable bindable, object data)
	{
		if (bindable == null)
		{
			throw new ArgumentNullException("bindable");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Type type = bindable.GetType();
		if (!type.CanBindTo(data.GetType(), out var bindMethodInfo))
		{
			throw new InvalidOperationException($"Cannot bind {type} to {data.GetType()}");
		}
		bindMethodInfo.Invoke(bindable, new object[1] { data });
	}
}
