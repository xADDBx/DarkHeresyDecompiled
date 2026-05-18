using System;
using Kingmaker.Enums.Sound;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventAct : AnimationClipEvent
{
	public AnimationClipEventAct()
	{
	}

	public AnimationClipEventAct(float time)
		: base(time)
	{
	}

	public override void Start(IAnimationManager animationManager)
	{
		if (TryGetUnitEntity(animationManager, out var unitEntity))
		{
			EventBus.RaiseEvent((IMechanicEntity)unitEntity, (Action<IAnimationEventHandler>)delegate(IAnimationEventHandler h)
			{
				h.HandleAnimationEvent(MappedAnimationEventType.Act);
			}, isCheckRuntime: true);
		}
		animationManager.CallbackReceiver.PostCommandActEvent();
	}

	private static bool TryGetUnitEntity(IAnimationManager animationManager, out IAbstractUnitEntity unitEntity)
	{
		unitEntity = null;
		if (!(animationManager is UnitAnimationManager unitAnimationManager))
		{
			return false;
		}
		unitEntity = unitAnimationManager.View.Or(null)?.EntityData;
		if (unitEntity == null && unitAnimationManager.GetComponentInParent<MechadendriteSettings>() != null)
		{
			UnitAnimationManager componentInParent = unitAnimationManager.GetComponentInParent<UnitAnimationManager>();
			if ((object)componentInParent != null)
			{
				unitEntity = componentInParent.View.Or(null)?.EntityData;
			}
		}
		return unitEntity != null;
	}

	public override object Clone()
	{
		return new AnimationClipEventAct(base.Time);
	}

	public override string ToString()
	{
		return $"Act event at {base.Time}";
	}
}
