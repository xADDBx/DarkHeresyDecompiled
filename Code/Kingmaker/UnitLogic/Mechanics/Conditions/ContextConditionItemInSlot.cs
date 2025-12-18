using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Serializable]
[TypeId("65491e5166ac8a64d8c948688ea53483")]
public class ContextConditionItemInSlot : ContextCondition
{
	public EquipSlotType EquipmentSlot;

	[SerializeField]
	private bool IsCaster;

	[SerializeField]
	private BlueprintItemReference m_Item;

	public BlueprintItem Item => m_Item;

	protected override string GetConditionCaption()
	{
		if (!IsCaster)
		{
			return $"Check if target has {Item} in slot {EquipmentSlot}";
		}
		return $"Check if caster has {Item} in slot {EquipmentSlot}";
	}

	protected override bool CheckCondition()
	{
		PartUnitBody partUnitBody = ((!IsCaster) ? base.Target.Entity?.GetBodyOptional() : base.Context.MaybeOwner?.GetBodyOptional());
		return partUnitBody?.GetEquipSlot(EquipmentSlot, partUnitBody.CurrentHandEquipmentSetIndex).MaybeItem?.Blueprint == Item;
	}
}
