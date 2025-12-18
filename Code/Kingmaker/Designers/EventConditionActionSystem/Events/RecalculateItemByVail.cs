using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[ComponentName("Events/RecalculateItemByVail")]
[AllowedOn(typeof(BlueprintItemEquipment))]
[TypeId("3d608c0ea45f99b448acc503ffdd1653")]
public class RecalculateItemByVail : EntityFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		EventBus.Subscribe(base.Owner);
	}

	protected override void OnDeactivate()
	{
		EventBus.Unsubscribe(base.Owner);
	}

	public void HandleVeilThicknessValueChanged(int delta, int value)
	{
		if (base.Owner is ItemEntity { Wielder: { } wielder } itemEntity)
		{
			itemEntity.OnWillUnequip();
			itemEntity.OnDidEquipped(wielder);
		}
	}
}
