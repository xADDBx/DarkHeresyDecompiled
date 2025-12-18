using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using UnityEngine;

namespace Kingmaker.Code.Framework.GameLog;

public class UnitFakeDeathMessageLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventFakeCompanionDeath>
{
	public void HandleEvent(GameLogEventFakeCompanionDeath evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Unit;
		AddMessage(new CombatLogMessage(evt.Message, default(Color), PrefixIcon.None));
	}
}
