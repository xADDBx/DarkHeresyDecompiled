using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingmaker.Gameplay.Features.UnitStats;

public class AttributeCategory
{
	public static readonly AttributeCategory Empty = new AttributeCategory();

	private readonly Dictionary<AttributeCategoryAdvanceType, int> _advances = new Dictionary<AttributeCategoryAdvanceType, int>();

	public AttributeCategoryType Type => (AttributeCategoryType)Math.Clamp(_advances.Sum((KeyValuePair<AttributeCategoryAdvanceType, int> i) => i.Value), 0, 4);

	public IEnumerable<(AttributeCategoryAdvanceType Type, int Value)> Advances => _advances.Select((KeyValuePair<AttributeCategoryAdvanceType, int> i) => (Key: i.Key, Value: i.Value));

	public void Add(AttributeCategoryAdvanceType type, int value)
	{
		_advances[type] = Get(type) + value;
	}

	public int Get(AttributeCategoryAdvanceType type)
	{
		return _advances.GetValueOrDefault(type);
	}
}
