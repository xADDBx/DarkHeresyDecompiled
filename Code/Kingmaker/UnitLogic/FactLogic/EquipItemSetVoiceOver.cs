using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("7916b66f1f3f482eaec9ca6b69b380a5")]
public class EquipItemSetVoiceOver : EntityFactComponentDelegate<ItemEntity>, IEquipItemHandler<EntitySubscriber>, IEquipItemHandler, ISubscriber<IItemEntity>, ISubscriber, IEventTag<IEquipItemHandler, EntitySubscriber>, IViewAttachedHandler, ISubscriber<IEntity>, IViewDetachedHandler
{
	public int RTPCValue;

	private const string RTPC_EVENT_NAME = "RTPC_VO_Processing";

	private bool m_WasSet;

	public void OnDidEquipped()
	{
		TrySetRTPC();
	}

	public void OnWillUnequip()
	{
		TryResetRTPC();
	}

	private void TrySetRTPC()
	{
		if (base.Owner.Wielder != null && base.Owner.Wielder.View != null)
		{
			AkUnitySoundEngine.SetRTPCValue("RTPC_VO_Processing", RTPCValue, base.Owner.Wielder.View.gameObject, 0);
			PFLog.VO.Log($"Setting RTPC_VO_Processing to {RTPCValue} on {base.Owner.Wielder.View.gameObject.name}");
		}
	}

	private void TryResetRTPC()
	{
		if (base.Owner.Wielder != null && base.Owner.Wielder.View != null)
		{
			AkUnitySoundEngine.SetRTPCValue("RTPC_VO_Processing", 0f, base.Owner.Wielder.View.gameObject, 0);
			PFLog.VO.Log($"Setting RTPC_VO_Processing to {0} on {base.Owner.Wielder.View.gameObject.name}");
		}
	}

	public void OnViewAttached(IEntityViewBase view)
	{
		if (base.Owner.Wielder == view.Data)
		{
			TrySetRTPC();
		}
	}

	public void OnViewDetached(IEntityViewBase view)
	{
		if (base.Owner.Wielder == view.Data)
		{
			TryResetRTPC();
		}
	}
}
