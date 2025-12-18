using System;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("2fef7db5bb962ed4c80b1436a54e2a4e")]
public class DoorNotOpeningTrigger : TutorialTriggerTimer, IGameTimeChangedHandler, ISubscriber, IInteractionHandler, ISubscriber<IBaseUnitEntity>
{
	private TimeSpan m_TimeSinceNotOpening = TimeSpan.Zero;

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (CanStart && !IsDone && !(Game.Instance.CurrentModeType != GameModeType.Default))
		{
			m_TimeSinceNotOpening += delta;
			if (m_TimeSinceNotOpening.Seconds >= TimerValue && !IsDone)
			{
				Actions.Run();
				IsDone = true;
			}
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public void OnInteract(AbstractInteractionPart interaction)
	{
		if (interaction is InteractionDoorPart)
		{
			IsDone = true;
		}
	}

	public void OnInteractionRestricted(AbstractInteractionPart interaction)
	{
	}
}
