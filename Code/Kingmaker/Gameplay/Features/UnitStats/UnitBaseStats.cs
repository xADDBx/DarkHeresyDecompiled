using System.Collections.Generic;
using System.Collections.Immutable;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Settings;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.UnitStats;

public sealed class UnitBaseStats
{
	private static readonly Dictionary<BlueprintUnit, Dictionary<int, UnitBaseStats>> _Cache = new Dictionary<BlueprintUnit, Dictionary<int, UnitBaseStats>>();

	private readonly BlueprintUnit _blueprint;

	private readonly EnemyDifficultyOption _enemyDurability;

	private readonly int _cr;

	private readonly ImmutableDictionary<AttributeType, AttributeCategory> _attributeCategories;

	private readonly ImmutableDictionary<StatType, StatBaseValue> _values;

	public StatBaseValue this[StatType stat] => _values.GetValueOrDefault(stat);

	public StatBaseValue this[AttributeType attribute] => _values.GetValueOrDefault(attribute.ToStatType());

	public StatBaseValue this[SkillType skill] => _values.GetValueOrDefault(skill.ToStatType());

	private UnitBaseStats(BlueprintUnit blueprint, EnemyDifficultyOption enemyDurability, int cr)
	{
		_blueprint = blueprint;
		_enemyDurability = enemyDurability;
		_cr = cr;
		_attributeCategories = UnitBaseStatsHelper.CalculateAttributeCategories(blueprint);
		_values = UnitBaseStatsHelper.CalculateStats(blueprint, enemyDurability, cr, GetAttributeCategoryType);
	}

	public static UnitBaseStats Get(BlueprintUnit blueprint, EnemyDifficultyOption enemyDurability, int cr)
	{
		if (!Application.isPlaying)
		{
			return new UnitBaseStats(blueprint, enemyDurability, cr);
		}
		if (!_Cache.TryGetValue(blueprint, out Dictionary<int, UnitBaseStats> value))
		{
			Dictionary<int, UnitBaseStats> dictionary2 = (_Cache[blueprint] = new Dictionary<int, UnitBaseStats>());
			value = dictionary2;
		}
		if (!value.TryGetValue(cr, out var value2) || value2._enemyDurability != enemyDurability)
		{
			return value[cr] = new UnitBaseStats(blueprint, enemyDurability, cr);
		}
		return value2;
	}

	public AttributeCategory GetAttributeCategory(AttributeType attribute)
	{
		return _attributeCategories.GetValueOrDefault(attribute, AttributeCategory.Empty);
	}

	public AttributeCategoryType GetAttributeCategoryType(AttributeType attribute)
	{
		return GetAttributeCategory(attribute).Type;
	}
}
