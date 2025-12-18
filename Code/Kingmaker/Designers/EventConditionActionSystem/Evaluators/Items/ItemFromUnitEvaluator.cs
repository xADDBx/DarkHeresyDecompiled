using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.Items;

[Obsolete]
[TypeId("558654abb1697ac469a5eb72c56796fc")]
public class ItemFromUnitEvaluator : AbstractItemEvaluator
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_Blueprint;

	[CanBeNull]
	[SerializeReference]
	public AbstractUnitEvaluator Wielder;

	public BlueprintItem Blueprint => m_Blueprint;

	public override string GetCaption()
	{
		if (Wielder != null)
		{
			return $"Item [{Blueprint}] equipped by [{Wielder}]";
		}
		return $"Item [{Blueprint}] from player's inventory";
	}

	protected override ItemEntity GetValueInternal()
	{
		if (Wielder != null)
		{
			if (!(Wielder.GetValue() is BaseUnitEntity baseUnitEntity))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Evaluator {this}, {Wielder} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return null;
			}
			foreach (ItemSlot equipmentSlot in baseUnitEntity.Body.EquipmentSlots)
			{
				if (equipmentSlot.MaybeItem?.Blueprint == Blueprint)
				{
					return equipmentSlot.MaybeItem;
				}
			}
		}
		foreach (ItemEntity item in Game.Instance.PartySharedInventory.Collection)
		{
			if (item.Blueprint == Blueprint)
			{
				return item;
			}
		}
		throw new FailToEvaluateException(this);
	}
}
