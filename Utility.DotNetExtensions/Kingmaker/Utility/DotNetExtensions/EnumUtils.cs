using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine.Serialization;

namespace Kingmaker.Utility.DotNetExtensions;

public static class EnumUtils
{
	private static readonly Dictionary<Type, (object, object[])[]> ValuesWithAttributesCache = new Dictionary<Type, (object, object[])[]>();

	public static IEnumerable<TEnum> GetValues<TEnum>()
	{
		if (!typeof(TEnum).IsEnum)
		{
			return Enumerable.Empty<TEnum>();
		}
		return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
	}

	public static IEnumerable<Enum> GetValues(Type enumType)
	{
		if (!enumType.IsEnum)
		{
			return Enumerable.Empty<Enum>();
		}
		return Enum.GetValues(enumType).Cast<Enum>().OrderBy(Convert.ToInt32);
	}

	public static IEnumerable<Enum> GetValues64(Type enumType)
	{
		if (!enumType.IsEnum)
		{
			return Enumerable.Empty<Enum>();
		}
		return Enum.GetValues(enumType).Cast<Enum>().OrderBy(Convert.ToInt64);
	}

	public static int GetMaxValuePlusOne<TEnum>() where TEnum : Enum
	{
		return Enum.GetValues(typeof(TEnum)).Cast<int>().Max() + 1;
	}

	public static int GetOrdinalNumber<TEnum>(TEnum value) where TEnum : Enum
	{
		return Enum.GetValues(typeof(TEnum)).Cast<Enum>().OrderBy(Convert.ToInt32)
			.FindIndex((Enum e) => object.Equals(e, value));
	}

	public static TEnum GetValueInOrder<TEnum>(int order) where TEnum : Enum
	{
		return (TEnum)Enum.GetValues(typeof(TEnum)).Cast<Enum>().OrderBy(Convert.ToInt32)
			.ElementAt(order);
	}

	public static (object Value, object[] Attributes)[] GetValuesWithAttributes(Type enumType)
	{
		if (!ValuesWithAttributesCache.TryGetValue(enumType, out var value))
		{
			FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
			List<(object, object[])> list = new List<(object, object[])>();
			FieldInfo[] array = fields;
			foreach (FieldInfo obj in array)
			{
				object value2 = obj.GetValue(null);
				object[] customAttributes = obj.GetCustomAttributes(inherit: false);
				list.Add((value2, customAttributes));
			}
			value = (ValuesWithAttributesCache[enumType] = list.ToArray());
		}
		return value;
	}

	[CanBeNull]
	public static object GetValueFormerlySerializedAs(Type enumType, string name)
	{
		(object, object[])[] valuesWithAttributes = GetValuesWithAttributes(enumType);
		for (int i = 0; i < valuesWithAttributes.Length; i++)
		{
			(object, object[]) tuple = valuesWithAttributes[i];
			object[] item = tuple.Item2;
			for (int j = 0; j < item.Length; j++)
			{
				if (item[j] is FormerlySerializedAsAttribute formerlySerializedAsAttribute && formerlySerializedAsAttribute.oldName == name)
				{
					return tuple.Item1;
				}
			}
		}
		return null;
	}
}
