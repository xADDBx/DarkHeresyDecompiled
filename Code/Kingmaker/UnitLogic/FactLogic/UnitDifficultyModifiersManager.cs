using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("2e982373853d4e26b7e61354b88923e0")]
public abstract class UnitDifficultyModifiersManager : UnitFactComponentDelegate, IDifficultyChangedClassHandler, ISubscriber, IUnitChangeAttackFactionsHandler, ISubscriber<IBaseUnitEntity>
{
	private class TransientData : IEntityFactComponentTransientData
	{
		public readonly List<StatType> ModifiedStats = new List<StatType>();
	}

	protected override void OnActivateOrPostLoad()
	{
		UpdateModifiers();
	}

	protected override void OnDeactivate()
	{
		RemoveModifiers();
	}

	public void HandleDifficultyChanged()
	{
		UpdateModifiers();
	}

	protected virtual void UpdateModifiers()
	{
	}

	protected void RemoveModifiers()
	{
		TransientData transientData = RequestTransientData<TransientData>();
		foreach (StatType modifiedStat in transientData.ModifiedStats)
		{
			base.Owner.GetStatOptional(modifiedStat)?.RemoveModifiersFrom(base.Runtime);
		}
		transientData.ModifiedStats.Clear();
	}

	protected void AddPercentModifier(StatType statType, int percentModifier)
	{
		ModifiableValue statOptional = base.Owner.GetStatOptional(statType);
		if (statOptional != null)
		{
			int value = Mathf.FloorToInt((float)statOptional.BaseValue * ((float)percentModifier / 100f));
			if (statOptional.AddModifier(value, base.Runtime, ModifierDescriptor.Difficulty).HasValue)
			{
				RequestTransientData<TransientData>().ModifiedStats.Add(statType);
			}
		}
	}

	protected void AddModifier(StatType statType, int flatModifier)
	{
		ModifiableValue statOptional = base.Owner.GetStatOptional(statType);
		if (statOptional != null)
		{
			statOptional.AddModifier(flatModifier, base.Runtime, ModifierDescriptor.Difficulty);
			RequestTransientData<TransientData>().ModifiedStats.Add(statType);
		}
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		if (unit == base.Owner)
		{
			UpdateModifiers();
		}
	}
}
