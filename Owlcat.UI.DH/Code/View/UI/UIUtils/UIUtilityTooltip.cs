using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Interaction;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.Mechanics.Interactions.Restrictions;
using UnityEngine;

namespace Code.View.UI.UIUtils;

public static class UIUtilityTooltip
{
	private static readonly List<Vector2> DefaultPivots = new List<Vector2>
	{
		new Vector2(0.5f, 1f),
		new Vector2(0.5f, 0f),
		new Vector2(0f, 0.5f),
		new Vector2(1f, 0.5f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	private static readonly HashSet<WeaponTagProperty> HeaderOnlyTags = new HashSet<WeaponTagProperty>
	{
		WeaponTagProperty.Brutal,
		WeaponTagProperty.Destructive,
		WeaponTagProperty.Vital
	};

	private static readonly HashSet<WeaponTagProperty> BodyIgnoreTags = new HashSet<WeaponTagProperty>
	{
		WeaponTagProperty.Brutal,
		WeaponTagProperty.Destructive,
		WeaponTagProperty.Vital,
		WeaponTagProperty.Expensive
	};

	public static bool IsHeaderOnlyTag(this WeaponTagUISettings tagUISettings)
	{
		return HeaderOnlyTags.Contains(tagUISettings.Tag);
	}

	public static bool IsBodyIgnoreTag(this WeaponTagUISettings tagUISettings)
	{
		return BodyIgnoreTags.Contains(tagUISettings.Tag);
	}

	public static string GetTooltipElementLabel(TooltipElement type)
	{
		return UIStrings.Instance.TooltipsElementLabels.GetLabel(type);
	}

	public static bool IsSkillRestriction(IInteractionVariantActor actor)
	{
		if (!(actor is SkillUseWithoutToolRestrictionPart) && !(actor is LoreXenosRestrictionPart))
		{
			return actor is SleightOfHandRestrictionPart;
		}
		return true;
	}

	public static PortraitType GetPortraitType(BaseUnitEntity unit)
	{
		if (unit.IsPlayerEnemy)
		{
			return PortraitType.Enemy;
		}
		if (!unit.IsInPlayerParty && !unit.IsPlayerEnemy)
		{
			return PortraitType.Friend;
		}
		return PortraitType.Default;
	}

	public static void SetPivots(List<Vector2> pivots, List<Vector2> priorityPivots)
	{
		if (priorityPivots != null && priorityPivots.Count >= 0)
		{
			pivots.AddRange(priorityPivots);
		}
		if (pivots.Count <= 0)
		{
			pivots.AddRange(DefaultPivots);
		}
		float num = 10f;
		for (float num2 = 0f; num2 <= 1f; num2 += 1f)
		{
			for (float num3 = 0f; num3 <= num; num3 += 1f)
			{
				Vector2 item = new Vector2(num2, num3 / num);
				if (!pivots.Contains(item))
				{
					pivots.Add(new Vector2(num2, num3 / num));
				}
			}
		}
		for (float num4 = 0f; num4 <= 1f; num4 += 1f)
		{
			for (float num5 = 0f; num5 <= num; num5 += 1f)
			{
				Vector2 item2 = new Vector2(num5 / num, num4);
				if (!pivots.Contains(item2))
				{
					pivots.Add(new Vector2(num5 / num, num4));
				}
			}
		}
	}
}
