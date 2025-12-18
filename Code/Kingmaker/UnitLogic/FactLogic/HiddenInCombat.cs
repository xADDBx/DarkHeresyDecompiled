using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Combat/HiddenInCombat")]
[TypeId("14d2de2955ae47d4b6a57d939deec347")]
public class HiddenInCombat : MechanicEntityFactComponentDelegate, ITurnBasedModeHandler, ISubscriber
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool IsAlreadyHidden;
	}

	protected override void OnActivateOrPostLoad()
	{
		UpdateHidden(isDeactivating: false);
	}

	protected override void OnDeactivate()
	{
		UpdateHidden(isDeactivating: true);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateHidden(isDeactivating: false);
	}

	private void UpdateHidden(bool isDeactivating)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (!componentData.IsAlreadyHidden && Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			base.Owner.Features.Hidden.Retain();
			componentData.IsAlreadyHidden = true;
			base.Owner.MaybeMovementAgent?.Blocker.Unblock();
		}
		else if (componentData.IsAlreadyHidden && (!Game.Instance.Controllers.TurnController.TurnBasedModeActive || isDeactivating))
		{
			base.Owner.Features.Hidden.Release();
			componentData.IsAlreadyHidden = false;
		}
	}
}
