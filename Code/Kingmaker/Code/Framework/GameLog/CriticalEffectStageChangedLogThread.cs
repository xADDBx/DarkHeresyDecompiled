using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.Code.Framework.GameLog;

public class CriticalEffectStageChangedLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventCriticalEffectStageChanged>
{
	public void HandleEvent(GameLogEventCriticalEffectStageChanged evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Entity;
		GameLogContext.Description = evt.BodyPart.Name.Text;
		GameLogContext.PreviousValue = evt.Previous;
		GameLogContext.CurrentValue = evt.Current;
		AddMessage(LogThreadBase.Strings.CriticalEffectStageChanged.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, evt.Entity));
	}
}
