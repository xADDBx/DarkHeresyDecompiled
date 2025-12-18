using System.Collections.Generic;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.Code.Framework.GameLog;

public class CombatLogBarkLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventBark>
{
	private const int Capacity = 5;

	private readonly Queue<string> m_PreviousMessages = new Queue<string>(6);

	public void HandleEvent(GameLogEventBark evt)
	{
		AbstractUnitEntity abstractUnitEntity = evt.Actor as AbstractUnitEntity;
		if ((!Game.Instance.Controllers.TurnController.TurnBasedModeActive || !(Game.Instance.CurrentModeType == GameModeType.Default) || abstractUnitEntity == null || abstractUnitEntity.IsInCombat) && !m_PreviousMessages.Contains(evt.Text))
		{
			m_PreviousMessages.Enqueue(evt.Text);
			if (m_PreviousMessages.Count > 5)
			{
				m_PreviousMessages.Dequeue();
			}
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)abstractUnitEntity;
			GameLogContext.Text = evt.Text;
			AddMessage((GameLogContext.SourceEntity.Value != null) ? new CombatLogMessage(LogThreadBase.Strings.UnitBark.CreateCombatLogMessage(), null, hasTooltip: false) : new CombatLogMessage(LogThreadBase.Strings.ObjectBark.CreateCombatLogMessage(), null, hasTooltip: false));
		}
	}
}
