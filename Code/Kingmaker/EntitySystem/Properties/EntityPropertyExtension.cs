using System;
using System.Linq;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties;

public static class EntityPropertyExtension
{
	private static readonly Func<Entity, MechanicsContext, int?>[] Getters;

	static EntityPropertyExtension()
	{
		Getters = new Func<Entity, MechanicsContext, int?>[EnumUtils.GetMaxValuePlusOne<ContextProperty>() + 1];
		Getter(ContextProperty.None, (Entity _, MechanicsContext _) => 0);
		Getter(ContextProperty.BallisticSkill, delegate(Entity e, MechanicsContext _)
		{
			return Stat(e, StatType.BallisticSkill);
			static int? Stat(Entity entity, StatType type)
			{
				return entity.GetOptional<PartStatsContainer>()?.GetStatOptional(type);
			}
		});
		Getter(ContextProperty.WeaponSkill, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.WeaponSkill));
		Getter(ContextProperty.Strength, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Strength));
		Getter(ContextProperty.Toughness, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Toughness));
		Getter(ContextProperty.Agility, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Agility));
		Getter(ContextProperty.Intelligence, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Intelligence));
		Getter(ContextProperty.Willpower, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Willpower));
		Getter(ContextProperty.Perception, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Perception));
		Getter(ContextProperty.Fellowship, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Fellowship));
		Getter(ContextProperty.BallisticSkillBonus, delegate(Entity e, MechanicsContext _)
		{
			return StatBonus(e, StatType.BallisticSkill);
			static int? StatBonus(Entity entity, StatType type)
			{
				return entity.GetOptional<PartStatsContainer>()?.GetAttributeOptional(type)?.Bonus;
			}
		});
		Getter(ContextProperty.WeaponSkillBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.WeaponSkill));
		Getter(ContextProperty.StrengthBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Strength));
		Getter(ContextProperty.ToughnessBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Toughness));
		Getter(ContextProperty.AgilityBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Agility));
		Getter(ContextProperty.IntelligenceBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Intelligence));
		Getter(ContextProperty.WillpowerBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Willpower));
		Getter(ContextProperty.PerceptionBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Perception));
		Getter(ContextProperty.FellowshipBonus, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Fellowship));
		Getter(ContextProperty.Resolve, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Resolve));
		Getter(ContextProperty.Wounds, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.HitPoints));
		Getter(ContextProperty.MovementPoints, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.MovementPoints));
		Getter(ContextProperty.ActionPoints, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.ActionPoints));
		Getter(ContextProperty.CurrentWeaponRateOfFire, GetCurrentWeaponRateOfFire);
		Getter(ContextProperty.EnemiesAdjacent, GetEnemiesAdjacent);
		Getter(ContextProperty.CurrentMovementPoints, (Entity e, MechanicsContext _) => (int)(e.GetOptional<PartUnitCombatState>()?.MovementPoints ?? 0f));
		Getter(ContextProperty.CurrentActionPoints, (Entity e, MechanicsContext _) => e.GetOptional<PartUnitCombatState>()?.ActionPoints ?? 0);
		Getter(ContextProperty.SkillAthletics, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillAthletics));
		Getter(ContextProperty.SkillAwareness, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillAwareness));
		Getter(ContextProperty.SkillDemolition, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillDemolition));
		Getter(ContextProperty.SkillMedicae, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillMedicae));
		Getter(ContextProperty.SkillLoreXenos, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillLoreXenos));
		Getter(ContextProperty.SkillLoreWarp, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillLoreWarp));
		Getter(ContextProperty.SkillTechUse, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillTechUse));
		Getter(ContextProperty.SkillTenacity, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillTenacity));
		Getter(ContextProperty.SkillMobility, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillMobility));
		Getter(ContextProperty.SkillResistance, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillResistance));
		Getter(ContextProperty.SkillReflexes, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillReflexes));
		Getter(ContextProperty.SkillSleightOfHand, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillSleightOfHand));
		Getter(ContextProperty.SkillLoreHeresy, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillLoreHeresy));
		Getter(ContextProperty.SkillInterrogation, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillInterrogation));
		Getter(ContextProperty.SkillMettle, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillMettle));
		Getter(ContextProperty.SkillWits, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillWits));
		Getter(ContextProperty.SkillIntimidation, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillIntimidation));
		Getter(ContextProperty.SkillDiplomacy, (Entity e, MechanicsContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillDiplomacy));
		Getter(ContextProperty.ContextFactRank, (Entity _, MechanicsContext c) => c.Fact?.GetRank() ?? 0);
		static void Getter(ContextProperty propertyName, Func<Entity, MechanicsContext, int?> getter)
		{
			Getters[(int)propertyName] = getter;
		}
	}

	public static int GetValue(this ContextProperty property, Entity entity, MechanicsContext context)
	{
		try
		{
			if (entity == null)
			{
				PFLog.Default.ErrorWithReport($"Can't get property {property} from null");
				return 0;
			}
			int? num = Getters[(int)property]?.Invoke(entity, context);
			if (!num.HasValue)
			{
				PFLog.Default.ErrorWithReport($"Can't get property {property} from {entity}");
			}
			return num.GetValueOrDefault();
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, $"Exception in getter of property {property} ({entity})");
			return 0;
		}
	}

	private static int? GetCurrentWeaponRateOfFire(Entity e, MechanicsContext _)
	{
		return (SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.SourceAbility ?? SimpleContextData<PropertyContext, PropertyContext.Scope>.Current.Ability)?.GetWeaponStats().ResultRateOfFire;
	}

	private static int? GetEnemiesAdjacent(Entity e, MechanicsContext _)
	{
		BaseUnitEntity unit = e as BaseUnitEntity;
		if (unit == null)
		{
			return null;
		}
		return Game.Instance.EntityPools.AllUnits.Count((AbstractUnitEntity p) => p.DistanceToInCells(unit) <= 1);
	}
}
