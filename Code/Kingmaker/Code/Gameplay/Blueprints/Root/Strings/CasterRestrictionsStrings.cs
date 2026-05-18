using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Blueprints.Root.Strings;

public class CasterRestrictionsStrings : StringsContainer
{
	public LocalizedString StatRestriction;

	public LocalizedString RequiredFact;

	public LocalizedString IncompatibleFact;

	public LocalizedString NoIncompatibleFact;

	public LocalizedString IncompatibleWeapon;

	public LocalizedString CompatibleWeapon;

	public LocalizedString MeleeWeaponNotEquiped;

	public LocalizedString MeleeWeaponEquiped;

	public LocalizedString InCombat;

	public LocalizedString NotInCombat;

	public LocalizedString HasPriorityTarget;

	public LocalizedString NoPriorityTarget;

	public LocalizedString MovePointsSufficient;

	public LocalizedString MovePointsInsufficient;

	public string GetStatRestrictionText(StatType statType, int value)
	{
		GameLogContext.Text = LocalizedTexts.Instance.Stats.GetText(statType);
		GameLogContext.Count = value;
		return StatRestriction;
	}

	public string GetHasNoFactRestrictionText(BlueprintUnitFact fact, bool hasFact)
	{
		GameLogContext.Text = GenerateLink(fact);
		return hasFact ? IncompatibleFact : NoIncompatibleFact;
	}

	public string GetHasFactRestrictionText(BlueprintUnitFact fact, bool hasFact)
	{
		string text = GenerateLink(fact);
		GameLogContext.Text = text;
		if (!hasFact)
		{
			return RequiredFact;
		}
		return text;
	}

	public string GetCompatibleWeaponText(bool isCompatible)
	{
		return isCompatible ? CompatibleWeapon : IncompatibleWeapon;
	}

	private static string GenerateLink(BlueprintUnitFact fact)
	{
		return "<b><color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.LinkColor) + "><link=\"f:" + fact.AssetGuid + "\">" + fact.LocalizedName.Text + "</link></color></b>";
	}
}
