using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[ComponentName("Stats/AddStatModifier")]
[TypeId("f08844ce14d498a45a9fc64582489a2a")]
public sealed class AddStatModifier : UnitFactComponentDelegate
{
	public enum StatSelectorType
	{
		Single,
		AllAttributes,
		AllSkills
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public StatSelectorType StatSelector;

	[ShowIf("IsSingleStat")]
	public StatType Stat;

	public ContextValueModifierWithType Value = new ContextValueModifierWithType
	{
		Enabled = true
	};

	[SerializeField]
	private ActionList m_ActionsOnAdd = new ActionList();

	private bool IsSingleStat => StatSelector == StatSelectorType.Single;

	protected override void OnActivateOrPostLoad()
	{
		if (!Restrictions.IsPassed(base.Context))
		{
			return;
		}
		if (StatSelector == StatSelectorType.Single)
		{
			AddModifier(Stat);
		}
		else
		{
			StatType[] array = ((StatSelector == StatSelectorType.AllAttributes) ? StatTypeHelper.Attributes : StatTypeHelper.Skills);
			foreach (StatType statType in array)
			{
				AddModifier(statType);
			}
		}
		m_ActionsOnAdd.Run();
	}

	protected override void OnDeactivate()
	{
		if (StatSelector == StatSelectorType.Single)
		{
			RemoveModifier(Stat);
			return;
		}
		StatType[] array = ((StatSelector == StatSelectorType.AllAttributes) ? StatTypeHelper.Attributes : StatTypeHelper.Skills);
		foreach (StatType statType in array)
		{
			RemoveModifier(statType);
		}
	}

	private void AddModifier(StatType statType)
	{
		ModifiableValue statOptional = base.Owner.Stats.GetStatOptional(statType);
		if (statOptional != null)
		{
			int value = Value.Calculate(base.Context);
			statOptional.AddModifier(Value.ModifierType, value, base.Runtime, Descriptor);
		}
	}

	private void RemoveModifier(StatType statType)
	{
		base.Owner.Stats.GetStatOptional(statType)?.RemoveModifiersFrom(base.Runtime);
	}
}
