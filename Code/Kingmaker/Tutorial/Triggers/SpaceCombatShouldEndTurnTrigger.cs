using System;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("6578ba4178eb4af8af86109b79cd9344")]
public class SpaceCombatShouldEndTurnTrigger : TutorialTriggerTimer, IGameTimeChangedHandler, ISubscriber, ITurnEndHandler, ISubscriber<IMechanicEntity>
{
	private TimeSpan m_TimeSinceNoAction = TimeSpan.Zero;

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (CanStart && !IsDone && !(Game.Instance.CurrentModeType != GameModeType.SpaceCombat))
		{
			m_TimeSinceNoAction += delta;
			if (m_TimeSinceNoAction.Seconds >= TimerValue && !IsDone)
			{
				Actions.Run();
				IsDone = true;
			}
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		IsDone = true;
	}
}
