using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintEtude))]
[TypeId("34ed37943b30447a8d15bde729df81ee")]
public class OverrideAreaCR : EntityFactComponentDelegate, IAreaHandler, ISubscriber, IAreaActivationHandler
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public CROverrideToken Token { get; set; }
	}

	[SerializeField]
	private int m_NewCR;

	public int NewCR => m_NewCR;

	void IAreaHandler.OnAreaBeginUnloading()
	{
		if (TryClearOverride())
		{
			EventBus.RaiseEvent(delegate(IAreaCRChangedHandler h)
			{
				h.HandleAreaCRChanged();
			});
		}
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		TryApplyOverride();
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		if (base.OwnerBlueprint is BlueprintEtude && RequestTransientData<ComponentData>().Token != null)
		{
			EventBus.RaiseEvent(delegate(IAreaCRChangedHandler h)
			{
				h.HandleAreaCRChanged();
			});
		}
	}

	protected override void OnActivate()
	{
		if (TryApplyOverride())
		{
			EventBus.RaiseEvent(delegate(IAreaCRChangedHandler h)
			{
				h.HandleAreaCRChanged();
			});
		}
	}

	protected override void OnDeactivate()
	{
		if (TryClearOverride())
		{
			EventBus.RaiseEvent(delegate(IAreaCRChangedHandler h)
			{
				h.HandleAreaCRChanged();
			});
		}
	}

	private bool TryApplyOverride()
	{
		if (!(base.OwnerBlueprint is BlueprintEtude blueprintEtude))
		{
			return false;
		}
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState == null || loadedAreaState.AreaGuid != blueprintEtude.LinkedAreaPart.AssetGuid)
		{
			return false;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.Token != null)
		{
			return false;
		}
		componentData.Token = loadedAreaState.Settings.PushCROverride(NewCR);
		return true;
	}

	private bool TryClearOverride()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.Token == null)
		{
			return false;
		}
		bool result = Game.Instance.LoadedAreaState?.Settings.PopCROverride(componentData.Token) ?? false;
		componentData.Token = null;
		return result;
	}
}
