using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators.Items;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Gameplay.Features.Items.Parts;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Items.ContextActions;

[Serializable]
[TypeId("227815cfca284a1abeb1fc7f208ef6a4")]
public sealed class ContextActionAddItemToSlot : ContextAction
{
	[SerializeReference]
	[CanBeNull]
	public AbstractItemSlotEvaluator Slot;

	[ValidateNotNull]
	public BpRef<BlueprintItem> Item = new BpRef<BlueprintItem>();

	public bool UntilTheEndOfCombat;

	public override string GetCaption()
	{
		return string.Format("Add {0} to [{1}] {2}", Item, Slot, UntilTheEndOfCombat ? "until the end of combat" : "");
	}

	protected override void RunAction()
	{
		ItemSlot itemSlot = Slot?.GetValue() ?? throw new NullReferenceException();
		ItemEntity itemEntity = Item.Blueprint.CreateEntity();
		if (UntilTheEndOfCombat)
		{
			if (!Game.Instance.Controllers.TurnController.InCombat)
			{
				throw new InvalidOperationException();
			}
			itemEntity.GetOrCreate<PartItemRemoveAtTheEndOfCombat>();
		}
		using (ContextData<ItemSlot.IgnoreLock>.Request())
		{
			itemSlot.InsertItem(itemEntity);
		}
	}
}
