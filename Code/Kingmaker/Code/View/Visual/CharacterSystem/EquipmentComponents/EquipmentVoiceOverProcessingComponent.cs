using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

[AllowedOn(typeof(BlueprintEquipmentFeature))]
[TypeId("9cd6c4a6af5446b8a4dbddcf0ce30128")]
public class EquipmentVoiceOverProcessingComponent : BlueprintComponent, IAddEquipmentEntityHandler, IRemoveEquipmentEntityHandler
{
	[SerializeField]
	public int RTPCValue;

	public void HandleEquipmentEntityAdded(AbstractUnitEntityView view)
	{
		EquipItemSetVoiceOver.TrySetRTPC(view, RTPCValue);
	}

	public void HandleEquipmentEntityRemoved(AbstractUnitEntityView view)
	{
		EquipItemSetVoiceOver.TrySetRTPC(view, 0);
	}
}
