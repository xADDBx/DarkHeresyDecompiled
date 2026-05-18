using System;
using System.Collections.Generic;
using System.Reflection;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.EntitySystem.Stats.Components;

public sealed class ModifiableStatsFilterAttribute : EnumOrderAttribute
{
	private static Enum[] _cachedOrder;

	public override Enum[] Order => _cachedOrder ?? (_cachedOrder = BuildOrder());

	private static Enum[] BuildOrder()
	{
		List<Enum> list = new List<Enum>();
		FieldInfo[] fields = typeof(StatType).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetCustomAttribute<NonModifiableStatAttribute>() == null && fieldInfo.GetCustomAttribute<ObsoleteAttribute>() == null)
			{
				list.Add((StatType)fieldInfo.GetValue(null));
			}
		}
		return list.ToArray();
	}
}
