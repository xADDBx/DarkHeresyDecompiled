using System;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.GameConst;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICombatTexts
{
	public LocalizedString CombatLogShowHide;

	public LocalizedString CombatLogEventsFilter;

	public LocalizedString CombatLogDialogueFilter;

	public LocalizedString CombatLogCombatFilter;

	public LocalizedString AttackOfOpportunity;

	public LocalizedString ThrowSave;

	public LocalizedString Avoid;

	public LocalizedString Morale;

	public LocalizedString CriticallyInjured;

	public LocalizedString Miss;

	public LocalizedString Defended;

	public LocalizedString Cover;

	public LocalizedString CasterCriticallyInjured;

	public LocalizedString HPUninjured;

	public LocalizedString HPBarelyInjured;

	public LocalizedString HPInjured;

	public LocalizedString HPBadlyInjured;

	public LocalizedString HPNearDeath;

	public LocalizedString HPDead;

	public LocalizedString CultAmbush;

	public LocalizedString ConcentrationBroken;

	public string GetAvoidText(AttackResult result, bool isCasterCriticallyInjured)
	{
		return result switch
		{
			AttackResult.Defended => Defended, 
			AttackResult.CoverHit => Cover, 
			AttackResult.Miss => GetMissText(isCasterCriticallyInjured), 
			_ => Avoid, 
		};
	}

	private string GetMissText(bool isCasterCriticallyInjured)
	{
		if (!isCasterCriticallyInjured)
		{
			return Miss.Text;
		}
		return Miss.Text + ": " + CasterCriticallyInjured.Text;
	}

	public LocalizedString GetHPText(float hp)
	{
		if (hp >= UIConsts.HPUninjured)
		{
			return UIStrings.Instance.CombatTexts.HPUninjured;
		}
		if (hp >= UIConsts.HPBarelyInjured)
		{
			return UIStrings.Instance.CombatTexts.HPBarelyInjured;
		}
		if (hp >= UIConsts.HPInjured)
		{
			return UIStrings.Instance.CombatTexts.HPInjured;
		}
		if (hp >= UIConsts.HPBadlyInjured)
		{
			return UIStrings.Instance.CombatTexts.HPBadlyInjured;
		}
		if (hp >= UIConsts.HPNearDeath)
		{
			return UIStrings.Instance.CombatTexts.HPNearDeath;
		}
		return UIStrings.Instance.CombatTexts.HPDead;
	}

	public string GetTbmCombatText(string text, int roll, int dc)
	{
		if (roll <= 0)
		{
			return text;
		}
		return $"{text}   ({roll} vs {dc})";
	}
}
