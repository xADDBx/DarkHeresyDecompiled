using System;
using System.Linq;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties;

public static class EntityPropertyExtension
{
	private static readonly Func<MechanicEntity, IEvalContext, int?>[] Getters;

	static EntityPropertyExtension()
	{
		Getters = new Func<MechanicEntity, IEvalContext, int?>[EnumUtils.GetMaxValuePlusOne<ContextProperty>() + 1];
		Getter(ContextProperty.None, (MechanicEntity _, IEvalContext _) => 0);
		Getter(ContextProperty.BallisticSkill, delegate(MechanicEntity e, IEvalContext _)
		{
			return Stat(e, StatType.BallisticSkill);
			static int? Stat(MechanicEntity entity, StatType type)
			{
				return entity.Actor.GetStat(type, null, default(StatContext), ".cctor");
			}
		});
		Getter(ContextProperty.WeaponSkill, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.WeaponSkill));
		Getter(ContextProperty.Strength, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Strength));
		Getter(ContextProperty.Toughness, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Toughness));
		Getter(ContextProperty.Agility, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Agility));
		Getter(ContextProperty.Intelligence, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Intelligence));
		Getter(ContextProperty.Willpower, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Willpower));
		Getter(ContextProperty.Perception, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Perception));
		Getter(ContextProperty.Fellowship, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Fellowship));
		Getter(ContextProperty.BallisticSkillBonus, delegate(MechanicEntity e, IEvalContext _)
		{
			return StatBonus(e, StatType.BallisticSkill);
			static int? StatBonus(MechanicEntity entity, StatType type)
			{
				return entity.Actor.GetStatBonus(type);
			}
		});
		Getter(ContextProperty.WeaponSkillBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.WeaponSkill));
		Getter(ContextProperty.StrengthBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Strength));
		Getter(ContextProperty.ToughnessBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Toughness));
		Getter(ContextProperty.AgilityBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Agility));
		Getter(ContextProperty.IntelligenceBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Intelligence));
		Getter(ContextProperty.WillpowerBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Willpower));
		Getter(ContextProperty.PerceptionBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Perception));
		Getter(ContextProperty.FellowshipBonus, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C1_2(e, StatType.Fellowship));
		Getter(ContextProperty.Resolve, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.Resolve));
		Getter(ContextProperty.Wounds, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.MaxHitPoints));
		Getter(ContextProperty.MovementPoints, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.MovementPoints));
		Getter(ContextProperty.ActionPoints, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.ActionPoints));
		Getter(ContextProperty.CurrentWeaponRateOfFire, GetCurrentWeaponRateOfFire);
		Getter(ContextProperty.EnemiesAdjacent, GetEnemiesAdjacent);
		Getter(ContextProperty.CurrentMovementPoints, (MechanicEntity e, IEvalContext _) => (int)(e.GetOptional<PartUnitCombatState>()?.MovementPoints ?? 0f));
		Getter(ContextProperty.CurrentActionPoints, (MechanicEntity e, IEvalContext _) => e.GetOptional<PartUnitCombatState>()?.ActionPoints ?? 0);
		Getter(ContextProperty.SkillAthletics, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillAthletics));
		Getter(ContextProperty.SkillAwareness, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillAwareness));
		Getter(ContextProperty.SkillDemolition, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillDemolition));
		Getter(ContextProperty.SkillMedicae, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillMedicae));
		Getter(ContextProperty.SkillLoreXenos, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillLoreXenos));
		Getter(ContextProperty.SkillLoreWarp, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillLoreWarp));
		Getter(ContextProperty.SkillTechUse, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillTechUse));
		Getter(ContextProperty.SkillTenacity, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillTenacity));
		Getter(ContextProperty.SkillMobility, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillMobility));
		Getter(ContextProperty.SkillResistance, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillResistance));
		Getter(ContextProperty.SkillReflexes, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillReflexes));
		Getter(ContextProperty.SkillSleightOfHand, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillSleightOfHand));
		Getter(ContextProperty.SkillLoreHeresy, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillLoreHeresy));
		Getter(ContextProperty.SkillInterrogation, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillInterrogation));
		Getter(ContextProperty.SkillMettle, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillMettle));
		Getter(ContextProperty.SkillWits, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillWits));
		Getter(ContextProperty.SkillIntimidation, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillIntimidation));
		Getter(ContextProperty.SkillDiplomacy, (MechanicEntity e, IEvalContext _) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C1_1(e, StatType.SkillDiplomacy));
		Getter(ContextProperty.ContextFactRank, (MechanicEntity _, IEvalContext c) => c.Fact?.GetRank() ?? 0);
		static void Getter(ContextProperty propertyName, Func<MechanicEntity, IEvalContext, int?> getter)
		{
			Getters[(int)propertyName] = getter;
		}
	}

	public static int GetValue(this ContextProperty property, MechanicEntity entity, IEvalContext context)
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

	private static int? GetCurrentWeaponRateOfFire(MechanicEntity e, IEvalContext _)
	{
		return (EvalContext.Current.SourceAbility ?? EvalContext.Current.Ability)?.GetWeaponStats().ResultRateOfFire;
	}

	private static int? GetEnemiesAdjacent(MechanicEntity e, IEvalContext _)
	{
		BaseUnitEntity unit = e as BaseUnitEntity;
		if (unit == null)
		{
			return null;
		}
		return Game.Instance.EntityPools.AllUnits.Count((AbstractUnitEntity p) => p.DistanceToInCells(unit) <= 1);
	}
}
