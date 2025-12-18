using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Stats.Base;

public static class StatTypeHelper
{
	public static readonly StatType[] AllStats = (from v in EnumUtils.GetValues<StatType>()
		where v != StatType.Unknown
		select v).ToArray();

	public static readonly StatType[] Attributes = (from v in EnumUtils.GetValues<AttributeType>()
		where v != AttributeType.Unknown
		select v).Select(ToStatType).ToArray();

	public static readonly StatType[] Skills = (from v in EnumUtils.GetValues<SkillType>()
		where v != SkillType.Unknown
		select v).Select(ToStatType).ToArray();

	public static readonly StatType[] KnowledgeSkills = (from v in EnumUtils.GetValues<KnowledgeSkillType>()
		where v != KnowledgeSkillType.Unknown
		select v).Select(ToStatType).ToArray();

	public static readonly ImmutableDictionary<StatType, StatType> BaseStats = new Dictionary<StatType, StatType>
	{
		{
			StatType.SkillAthletics,
			StatType.Strength
		},
		{
			StatType.SkillTenacity,
			StatType.Strength
		},
		{
			StatType.SkillResistance,
			StatType.Toughness
		},
		{
			StatType.SkillDemolition,
			StatType.Agility
		},
		{
			StatType.SkillMobility,
			StatType.Agility
		},
		{
			StatType.SkillReflexes,
			StatType.Agility
		},
		{
			StatType.SkillSleightOfHand,
			StatType.Agility
		},
		{
			StatType.SkillLoreHeresy,
			StatType.Intelligence
		},
		{
			StatType.SkillLoreXenos,
			StatType.Intelligence
		},
		{
			StatType.SkillLoreWarp,
			StatType.Intelligence
		},
		{
			StatType.SkillTechUse,
			StatType.Intelligence
		},
		{
			StatType.SkillInterrogation,
			StatType.Willpower
		},
		{
			StatType.SkillMettle,
			StatType.Willpower
		},
		{
			StatType.SkillAwareness,
			StatType.Perception
		},
		{
			StatType.SkillWits,
			StatType.Perception
		},
		{
			StatType.SkillIntimidation,
			StatType.Fellowship
		},
		{
			StatType.SkillDiplomacy,
			StatType.Fellowship
		},
		{
			StatType.SkillMedicae,
			StatType.Fellowship
		},
		{
			StatType.Initiative,
			StatType.Agility
		},
		{
			StatType.Resolve,
			StatType.Fellowship
		},
		{
			StatType.HitPoints,
			StatType.Toughness
		},
		{
			StatType.MovementPoints,
			StatType.Agility
		},
		{
			StatType.Defence,
			StatType.Agility
		}
	}.ToImmutableDictionary();

	private static readonly StatType[] s_DisplayOrder = new StatType[29]
	{
		StatType.WeaponSkill,
		StatType.BallisticSkill,
		StatType.Strength,
		StatType.Toughness,
		StatType.Agility,
		StatType.Intelligence,
		StatType.Perception,
		StatType.Willpower,
		StatType.Fellowship,
		StatType.SkillAthletics,
		StatType.SkillTenacity,
		StatType.SkillResistance,
		StatType.SkillDemolition,
		StatType.SkillMobility,
		StatType.SkillReflexes,
		StatType.SkillSleightOfHand,
		StatType.SkillLoreHeresy,
		StatType.SkillLoreXenos,
		StatType.SkillLoreWarp,
		StatType.SkillTechUse,
		StatType.SkillInterrogation,
		StatType.SkillMettle,
		StatType.SkillAwareness,
		StatType.SkillWits,
		StatType.SkillIntimidation,
		StatType.SkillDiplomacy,
		StatType.SkillMedicae,
		StatType.Initiative,
		StatType.HitPoints
	};

	private static readonly SkillType[] s_CombatSkills = new SkillType[5]
	{
		SkillType.Tenacity,
		SkillType.Resistance,
		SkillType.Reflexes,
		SkillType.Mettle,
		SkillType.Medicae
	};

	private static Enum[] s_DisplayOrderActual;

	public static Enum[] DisplayOrder
	{
		get
		{
			if (s_DisplayOrderActual == null)
			{
				s_DisplayOrderActual = s_DisplayOrder.Concat(EnumUtils.GetValues<StatType>()).Distinct().Cast<Enum>()
					.ToArray();
			}
			return s_DisplayOrderActual;
		}
	}

	public static bool IsAttribute(this StatType stat)
	{
		if (stat != 0)
		{
			return Attributes.Contains(stat);
		}
		return false;
	}

	public static bool IsSkill(this StatType stat)
	{
		if (stat != 0)
		{
			return Skills.Contains(stat);
		}
		return false;
	}

	public static bool IsKnowledge(this StatType stat)
	{
		if (stat != 0)
		{
			return KnowledgeSkills.HasItem(stat);
		}
		return false;
	}

	public static bool IsCombatSkill(this StatType stat)
	{
		if (stat.IsSkill())
		{
			return stat.ToSkillType().IsCombatSkill();
		}
		return false;
	}

	public static bool IsCombatSkill(this SkillType skill)
	{
		if (skill != 0)
		{
			return s_CombatSkills.HasItem(skill);
		}
		return false;
	}

	public static StatType ToStatType(this AttributeType attribute)
	{
		return (StatType)attribute;
	}

	public static StatType ToStatType(this SkillType skill)
	{
		return (StatType)skill;
	}

	public static StatType ToStatType(this KnowledgeSkillType skill)
	{
		return (StatType)skill;
	}

	public static SkillType ToSkillType(this StatType stat)
	{
		if (!stat.IsSkill() && stat != 0)
		{
			throw new ArgumentOutOfRangeException("stat");
		}
		return (SkillType)stat;
	}

	public static AttributeType ToAttributeType(this StatType stat)
	{
		if (!stat.IsAttribute() && stat != 0)
		{
			throw new ArgumentOutOfRangeException("stat");
		}
		return (AttributeType)stat;
	}

	public static StatType GetBaseStat(this StatType stat)
	{
		if (!BaseStats.TryGetValue(stat, out var value))
		{
			throw new ArgumentOutOfRangeException("stat");
		}
		return value;
	}

	public static AttributeType GetBaseAttribute(this SkillType skill)
	{
		return skill.ToStatType().GetBaseStat().ToAttributeType();
	}
}
