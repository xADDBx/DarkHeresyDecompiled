using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.ParryChance;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a9480d21aeee4a940a478882bf1736fa")]
public abstract class WarhammerParryChanceModifier : MechanicEntityFactComponentDelegate
{
	[Flags]
	public enum PropertyType
	{
		ParryChance = 1,
		AttackerWeaponSkillBonus = 2,
		DefenderWeaponSkillBonus = 4
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsDropdown]
	public PropertyType Properties = PropertyType.ParryChance;

	[ShowIf("ModifyParryChance")]
	public ContextValue ParryChance;

	[ShowIf("ModifyAttackerWeaponSkillBonus")]
	public ContextValue AttackerWeaponSkillBonus;

	[ShowIf("ModifyDefenderWeaponSkillBonus")]
	public ContextValue DefenderWeaponSkillBonus;

	private bool ModifyParryChance => (Properties & PropertyType.ParryChance) != 0;

	private bool ModifyAttackerWeaponSkillBonus => (Properties & PropertyType.AttackerWeaponSkillBonus) != 0;

	private bool ModifyDefenderWeaponSkillBonus => (Properties & PropertyType.DefenderWeaponSkillBonus) != 0;
}
