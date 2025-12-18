using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging.DisplayStats;

internal sealed class ExtendedDebugDisplayStatMarker
{
	public ProfilingSampler Sampler { get; }

	public WaaaghProfileCategory Category { get; }

	private ExtendedDebugDisplayStatMarker(ProfilingSampler sampler, WaaaghProfileCategory category)
	{
		Category = category;
		Sampler = sampler;
	}

	public static List<ExtendedDebugDisplayStatMarker> CreateMany<TProfileId>() where TProfileId : struct, Enum
	{
		List<ExtendedDebugDisplayStatMarker> list = new List<ExtendedDebugDisplayStatMarker>();
		Type type = typeof(TProfileId);
		foreach (object value in Enum.GetValues(type))
		{
			MemberInfo element = type.GetMember(value.ToString()).First((MemberInfo m) => m.DeclaringType == type);
			if (Attribute.GetCustomAttribute(element, typeof(HideInDebugUIAttribute)) == null)
			{
				ProfilingSampler sampler = ProfilingSamplerStorage<TProfileId>.Get(null, (TProfileId)value);
				WaaaghProfileCategory category = ((Attribute.GetCustomAttribute(element, typeof(WaaaghProfileCategoryAttribute)) is WaaaghProfileCategoryAttribute waaaghProfileCategoryAttribute) ? waaaghProfileCategoryAttribute.Category : WaaaghProfileCategory.Misc);
				list.Add(new ExtendedDebugDisplayStatMarker(sampler, category));
			}
		}
		return list;
	}
}
