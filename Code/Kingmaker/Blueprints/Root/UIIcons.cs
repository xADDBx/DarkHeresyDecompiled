using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class UIIcons
{
	[Header("DefaultIcons")]
	public DefaultIcons DefaultIcons;

	[Header("Tooltips")]
	public TooltipIcons TooltipIcons;

	[Header("Tooltip Inspect")]
	public TooltipInspectIcons TooltipInspectIcons;

	[Header("Chargen")]
	public ChargenIcons ChargenIcons;

	[Header("CombatMessage")]
	public Sprite CultAmbush;

	public Sprite CombatMessageMorale;

	public Sprite HPDamageBonus;

	public Sprite ArmorDamageBonus;

	public Sprite Success;

	public Sprite Fail;

	public Sprite CantAct;

	public Sprite Heal;

	public Sprite ArmorRepair;

	public Sprite VitalDamageBonus;

	public Sprite CriticalApplied;

	[Header("Cargo")]
	public CargoIcons CargoIcons;

	public CargoTooltipIcons CargoTooltipIcons;

	[Header("SoulMarks")]
	public SoulMarkIcons SoulMarkIcons;

	[Header("CharScores")]
	public Sprite HP;

	public Sprite Damage;

	public Sprite Crit;

	public Sprite RateOfFireMelee;

	public Sprite Attack;

	public Sprite Melee;

	public Color32 MeleeColor;

	public Sprite Range;

	public Color32 RangeColor;

	public Sprite Speed;

	public Sprite Bab;

	public Color32 BabColor;

	public Sprite Penetration;

	public Sprite StatBackground;

	public Sprite Recoil;

	[Header("DamageForms")]
	public Sprite Slashing;

	public Color32 SlashingColor;

	[Header("AbilityTarget")]
	public Sprite TargetPersonal;

	public Sprite TargetAnyLine;

	public Sprite TargetCharge;

	public Sprite TargetAnyOne;

	public Sprite TargetAnyAll;

	public Sprite TargetAllyOne;

	public Sprite TargetAllyAll;

	public Sprite TargetEnemyOne;

	public Sprite TargetEnemyAll;

	public Sprite SpellTargetPoint;

	[Header("SpellTimer")]
	public Sprite TimeIcon;

	[Header("Ability Placeholder Icon")]
	public Sprite EmptyAbilityIcon;

	public Sprite[] AbilityPlaceholderIcon;

	public Sprite DiceD100;

	[Header("Other")]
	public Sprite Male;

	public Sprite Female;

	public Sprite Recommended;

	public Sprite NotRecommended;

	public Sprite Yes;

	public Sprite No;

	public Sprite Attention;

	[Header("ContextMenu")]
	public Sprite Check;

	public Sprite NotCheck;

	[Header("QuestTypes")]
	public QuestTypeIcons QuestTypesIcons;

	[Header("Entity Info")]
	public Sprite IconDOT;

	[Header("Buff Groups")]
	public Sprite CriticalEffects;

	public Sprite StatusEffects;

	public Sprite DotEffects;

	public Sprite NegativeEffects;

	public Sprite PositiveEffects;

	public Sprite GetGenderIcon(Gender gender)
	{
		return gender switch
		{
			Gender.Male => Male, 
			Gender.Female => Female, 
			_ => null, 
		};
	}

	public Sprite GetAppearanceIcon(CharGenAppearancePageType type)
	{
		return type switch
		{
			CharGenAppearancePageType.General => ChargenIcons.AppearanceGeneral, 
			CharGenAppearancePageType.Hair => ChargenIcons.AppearanceHair, 
			CharGenAppearancePageType.Tattoo => ChargenIcons.AppearanceTattoo, 
			CharGenAppearancePageType.Implants => ChargenIcons.AppearanceImplants, 
			_ => null, 
		};
	}
}
