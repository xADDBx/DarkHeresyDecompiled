using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class BuffInformation
{
	public readonly Sprite Icon;

	public readonly string Name;

	public static BuffInformation Create(BlueprintBuff data)
	{
		return new BuffInformation(data);
	}

	public static BuffInformation Create(EntityFact fact)
	{
		return new BuffInformation(fact);
	}

	public static BuffInformation Create(ItemEntity item)
	{
		return new BuffInformation(item);
	}

	private BuffInformation()
	{
	}

	private BuffInformation(IUIDataProvider data)
	{
		Icon = data.Icon;
		Name = data.Name;
	}

	private BuffInformation(EntityFact fact)
	{
		Icon = fact.Icon;
		Name = fact.Name;
	}

	private BuffInformation(ItemEntity item)
	{
		Icon = item.Icon;
		Name = item.Name;
	}
}
