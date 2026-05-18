using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("7916b66f1f3f482eaec9ca6b69b380a5")]
public class EquipItemSetVoiceOver : EntityFactComponentDelegate<ItemEntity>, IEquipItemHandler<EntitySubscriber>, IEquipItemHandler, ISubscriber<IItemEntity>, ISubscriber, IEventTag<IEquipItemHandler, EntitySubscriber>, IViewAttachedHandler, ISubscriber<IEntity>, IViewDetachedHandler
{
	public int RTPCValue;

	public const string RTPC_EVENT_NAME = "RTPC_VO_Processing";

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
			TrySetRTPC(base.Owner.Wielder.View.AsMechanicEntityView(), RTPCValue);
		}
	}

	public static void TrySetRTPC(MechanicEntityView view, int value)
	{
		if (!(view == null))
		{
			AkUnitySoundEngine.SetRTPCValue("RTPC_VO_Processing", value, view.gameObject, 0);
			PFLog.VO.Log($"Setting RTPC_VO_Processing to {value} on {view.gameObject.name}");
		}
	}

	private void TryResetRTPC()
	{
		if (base.Owner.Wielder != null && base.Owner.Wielder.View != null)
		{
			TrySetRTPC(base.Owner.Wielder.View.AsMechanicEntityView(), 0);
		}
	}

	public void OnViewAttached(IEntityView view)
	{
		if (base.Owner.Wielder == view.Data)
		{
			TrySetRTPC();
		}
	}

	public void OnViewDetached(IEntityView view)
	{
		if (base.Owner.Wielder == view.Data)
		{
			TryResetRTPC();
		}
	}
}
