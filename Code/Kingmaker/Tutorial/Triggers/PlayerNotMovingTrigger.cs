using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[ComponentName("Movement/PlayerNotMovingTrigger")]
[TypeId("0f25735707f54eaf98dcbfd49fd4fff3")]
public class PlayerNotMovingTrigger : TutorialTriggerTimer, IGameTimeChangedHandler, ISubscriber, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>
{
	private TimeSpan m_TimeSinceNotMoving = TimeSpan.Zero;

	private static BaseUnitEntity Player => GameHelper.GetPlayerCharacter();

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (CanStart && !IsDone && !(Game.Instance.CurrentModeType != GameModeType.Default))
		{
			if (!Player.MovementAgent.IsReallyMoving)
			{
				m_TimeSinceNotMoving += delta;
			}
			if (m_TimeSinceNotMoving.Seconds >= TimerValue && !IsDone)
			{
				Actions.Run();
				IsDone = true;
			}
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (unit.IsInPlayerParty)
		{
			IsDone = true;
		}
	}
}
