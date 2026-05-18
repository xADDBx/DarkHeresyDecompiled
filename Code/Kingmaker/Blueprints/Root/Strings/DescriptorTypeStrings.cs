using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

public class DescriptorTypeStrings : StringsContainer
{
	public LocalizedString None;

	public LocalizedString Racial;

	public LocalizedString Difficulty;

	public LocalizedString BaseStatBonus;

	public LocalizedString Polymorph;

	public LocalizedString UniqueItem;

	public LocalizedString RighteousFury;

	public LocalizedString ArmorDamageReduction;

	public LocalizedString AreaOfEffectAbilityMiss;

	public LocalizedString DegreeOfSuccess;

	public LocalizedString ScatterShot;

	public LocalizedString ArmorPenalty;

	public LocalizedString AttackerPerception;

	public LocalizedString WeaponSkillDifference;

	public LocalizedString BurstFirePenalty;

	public LocalizedString Weapon;

	public LocalizedString AttackerWeaponSkill;

	public LocalizedString OriginAdvancement;

	public LocalizedString CareerAdvancement;

	public LocalizedString OtherAdvancement;

	public LocalizedString Immunity;

	public LocalizedString HitChanceOverkill;

	public LocalizedString DistanceToTarget;

	public LocalizedString BaseValue;

	public LocalizedString UntypedStackable;

	public LocalizedString UntypedUnstackable;

	public LocalizedString Wounds;

	public LocalizedString Trauma;

	public LocalizedString AttackerAgility;

	public LocalizedString PreciseAttack;

	public LocalizedString Morale;

	public LocalizedString BodyPartVital;

	public LocalizedString Recoil;

	public LocalizedString AllyLeaderDead;

	public LocalizedString EnemyLeaderDead;

	public LocalizedString Cheat;

	public LocalizedString MoraleChangeLocked;

	public LocalizedString CanNotGainMorale;

	public LocalizedString EnemyCombatVeterancy;

	public string GetText(ModifierDescriptor type)
	{
		return type switch
		{
			ModifierDescriptor.None => None, 
			ModifierDescriptor.Racial => Racial, 
			ModifierDescriptor.Difficulty => Difficulty, 
			ModifierDescriptor.BaseStatBonus => BaseStatBonus, 
			ModifierDescriptor.Polymorph => Polymorph, 
			ModifierDescriptor.UniqueItem => UniqueItem, 
			ModifierDescriptor.RighteousFury => RighteousFury, 
			ModifierDescriptor.DamageReduction => ArmorDamageReduction, 
			ModifierDescriptor.AreaOfEffectAbilityMiss => AreaOfEffectAbilityMiss, 
			ModifierDescriptor.DegreeOfSuccess => DegreeOfSuccess, 
			ModifierDescriptor.ScatterShot => ScatterShot, 
			ModifierDescriptor.ArmorPenalty => ArmorPenalty, 
			ModifierDescriptor.AttackerPerception => AttackerPerception, 
			ModifierDescriptor.WeaponSkillDifference => WeaponSkillDifference, 
			ModifierDescriptor.BurstFirePenalty => BurstFirePenalty, 
			ModifierDescriptor.Weapon => Weapon, 
			ModifierDescriptor.AttackerWeaponSkill => AttackerWeaponSkill, 
			ModifierDescriptor.OriginAdvancement => OriginAdvancement, 
			ModifierDescriptor.CareerAdvancement => CareerAdvancement, 
			ModifierDescriptor.OtherAdvancement => OtherAdvancement, 
			ModifierDescriptor.Immunity => Immunity, 
			ModifierDescriptor.HitChanceOverkill => HitChanceOverkill, 
			ModifierDescriptor.DistanceToTarget => DistanceToTarget, 
			ModifierDescriptor.BaseValue => BaseValue, 
			ModifierDescriptor.UntypedStackable => UntypedStackable, 
			ModifierDescriptor.UntypedUnstackable => UntypedUnstackable, 
			ModifierDescriptor.Wounds => Wounds, 
			ModifierDescriptor.Trauma => Trauma, 
			ModifierDescriptor.AttackerAgility => AttackerAgility, 
			ModifierDescriptor.PreciseAttack => PreciseAttack, 
			ModifierDescriptor.Morale => Morale, 
			ModifierDescriptor.BodyPartVital => BodyPartVital, 
			ModifierDescriptor.Recoil => Recoil, 
			ModifierDescriptor.AllyLeaderDead => AllyLeaderDead, 
			ModifierDescriptor.EnemyLeaderDead => EnemyLeaderDead, 
			ModifierDescriptor.MoraleChangeLocked => MoraleChangeLocked, 
			ModifierDescriptor.CanNotGainMorale => CanNotGainMorale, 
			ModifierDescriptor.EnemyCombatVeterancy => EnemyCombatVeterancy, 
			_ => $"[ModifierDescriptor] {type}", 
		};
	}
}
