using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kingmaker.Utility.DotNetExtensions;

public static class TypesCache
{
	private const BindingFlags MethodFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static ReadonlyList<Type>? _allTypes;

	private static ReadonlyList<MethodInfo>? _allMethods;

	private static Dictionary<Type, ReadonlyList<Type>> _attributeToTypes = new Dictionary<Type, ReadonlyList<Type>>();

	private static Dictionary<Type, ReadonlyList<Type>> _inheritedAttributeToTypes = new Dictionary<Type, ReadonlyList<Type>>();

	private static Dictionary<Type, ReadonlyList<MethodInfo>> _attributeToMethods = new Dictionary<Type, ReadonlyList<MethodInfo>>();

	public static ReadonlyList<Type> GetAllTypes()
	{
		ReadonlyList<Type> valueOrDefault = _allTypes.GetValueOrDefault();
		if (!_allTypes.HasValue)
		{
			valueOrDefault = AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly i) => i.GetTypes()).ToArray();
			_allTypes = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public static ReadonlyList<MethodInfo> GetAllMethods()
	{
		ReadonlyList<MethodInfo> valueOrDefault = _allMethods.GetValueOrDefault();
		if (!_allMethods.HasValue)
		{
			valueOrDefault = GetAllTypes().SelectMany((Type i) => i.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToArray();
			_allMethods = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public static ReadonlyList<Type> GetTypesWithAttribute<T>(bool inherit)
	{
		return GetTypesWithAttribute(typeof(T), inherit);
	}

	public static ReadonlyList<Type> GetTypesWithAttribute(Type attributeType, bool inherit)
	{
		Dictionary<Type, ReadonlyList<Type>> dictionary = (inherit ? _inheritedAttributeToTypes : _attributeToTypes);
		if (!dictionary.TryGetValue(attributeType, out var value))
		{
			value = (dictionary[attributeType] = (from i in GetAllTypes()
				where i.IsDefined(attributeType, inherit: false)
				select i).ToArray());
		}
		return value;
	}

	public static ReadonlyList<MethodInfo> GetMethodsWithAttribute<T>()
	{
		return GetMethodsWithAttribute(typeof(T));
	}

	public static ReadonlyList<MethodInfo> GetMethodsWithAttribute(Type attributeType)
	{
		if (!_attributeToMethods.TryGetValue(attributeType, out var value))
		{
			value = (_attributeToMethods[attributeType] = (from i in GetAllMethods()
				where i.IsDefined(attributeType)
				select i).ToArray());
		}
		return value;
	}

	public static string GetTypeName(Type type)
	{
		return type.FullName;
	}
}
