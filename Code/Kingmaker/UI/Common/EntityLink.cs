using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class EntityLink
{
	public enum Type
	{
		Unit,
		Item,
		ItemBlueprint,
		UnitFact,
		GroupAbility,
		UI,
		Encyclopedia,
		SkillcheckResult,
		SkillcheckDC,
		DialogExchange,
		RelatedDetectiveItems,
		UIProperty,
		Alignment,
		UnitStat,
		Unknown,
		Empty,
		DialogConditions,
		Highlight,
		Detective,
		AbilityTag,
		CloseCase
	}

	private static readonly Dictionary<Type, string> Tags = new Dictionary<Type, string>
	{
		{
			Type.Unit,
			"u"
		},
		{
			Type.Item,
			"i"
		},
		{
			Type.ItemBlueprint,
			"ib"
		},
		{
			Type.UnitFact,
			"f"
		},
		{
			Type.GroupAbility,
			"a"
		},
		{
			Type.UI,
			"ui"
		},
		{
			Type.Encyclopedia,
			"Encyclopedia"
		},
		{
			Type.SkillcheckResult,
			"SkillcheckResult"
		},
		{
			Type.SkillcheckDC,
			"SkillcheckDC"
		},
		{
			Type.DialogExchange,
			"DialogExchange"
		},
		{
			Type.RelatedDetectiveItems,
			"RelatedDetectiveItems"
		},
		{
			Type.UIProperty,
			"uip"
		},
		{
			Type.Alignment,
			"Alignment"
		},
		{
			Type.UnitStat,
			"us"
		},
		{
			Type.Unknown,
			""
		},
		{
			Type.Empty,
			"Empty"
		},
		{
			Type.DialogConditions,
			"DialogConditions"
		},
		{
			Type.Highlight,
			"Highlight"
		},
		{
			Type.Detective,
			"Detective"
		},
		{
			Type.AbilityTag,
			"AbilityTag"
		},
		{
			Type.CloseCase,
			"CloseCase"
		}
	};

	public static string GetTag(Type type)
	{
		if (!Tags.ContainsKey(type))
		{
			Debug.LogError("EntityLinkType " + type.ToString() + " does not have tag");
			return "";
		}
		return Tags[type];
	}

	public static Type GetEntityType(string key)
	{
		Type result = Type.Empty;
		if (Tags.Values.Contains(key))
		{
			result = Tags.FirstOrDefault((KeyValuePair<Type, string> d) => d.Value == key).Key;
		}
		return result;
	}
}
