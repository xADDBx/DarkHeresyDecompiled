using Kingmaker.EntitySystem;
using Kingmaker.Items.Slots;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("6bda0dd3de3e4fa8b1a7a63f7bad01cd")]
public class DisableEquipmentSlot : UnitFactComponentDelegate
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public ItemSlot Slot;
	}

	private enum SlotType
	{
		Armor,
		MainHand,
		OffHand
	}

	[SerializeField]
	private SlotType m_SlotType;

	protected override void OnActivateOrPostLoad()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.Slot = GetSlot();
		if (componentData.Slot != null)
		{
			componentData.Slot.Lock.Retain();
			componentData.Slot.RetainDeactivateFlag();
		}
	}

	protected override void OnDeactivate()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.Slot != null)
		{
			componentData.Slot.Lock.Release();
			componentData.Slot.ReleaseDeactivateFlag();
			componentData.Slot = null;
		}
	}

	private ItemSlot GetSlot()
	{
		switch (m_SlotType)
		{
		case SlotType.Armor:
			return base.Owner.Body.Armor;
		case SlotType.MainHand:
			return base.Owner.Body.PrimaryHand;
		case SlotType.OffHand:
			return base.Owner.Body.SecondaryHand;
		default:
			PFLog.Default.Error($"Can't extract slot of type {m_SlotType}");
			return null;
		}
	}
}
