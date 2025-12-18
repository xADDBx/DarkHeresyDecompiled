using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.Items.Slots;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.Items;

[Serializable]
[TypeId("5b6ce1df43944a24a3b5c7c6cce39c9c")]
public sealed class ItemSlotEvaluator : AbstractItemSlotEvaluator
{
	[SerializeReference]
	[CanBeNull]
	public AbstractItemEvaluator Item;

	public override string GetCaption()
	{
		return $"Slot of {Item}";
	}

	protected override ItemSlot GetValueInternal()
	{
		return Item?.GetValue().HoldingSlot;
	}
}
