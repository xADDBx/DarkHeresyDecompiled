using System;
using Kingmaker.Blueprints.Base;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class UIIcons
{
	[Header("DefaultIcons")]
	public Sprite DefaultItemIcon;

	public Sprite DefaultAbilityIcon;

	public Sprite DefaultAbilityModifierIcon;

	public Sprite DefaultColonyProjectIcon;

	[Header("Tooltips")]
	public TooltipIcons TooltipIcons;

	[Header("Tooltip Inspect")]
	public TooltipInspectIcons TooltipInspectIcons;

	[Header("CombatMessage")]
	public Sprite CultAmbush;

	public Sprite CombatMessageMorale;

	public Sprite HPDamageBonus;

	public Sprite ArmorDamageBonus;

	public Sprite Success;

	public Sprite Fail;

	public Sprite CantAct;

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

	public Sprite GetGenderIcon(Gender gender)
	{
		return gender switch
		{
			Gender.Male => Male, 
			Gender.Female => Female, 
			_ => null, 
		};
	}
}
