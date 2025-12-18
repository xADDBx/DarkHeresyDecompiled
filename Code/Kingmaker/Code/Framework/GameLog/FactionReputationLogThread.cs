using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.Code.Framework.GameLog;

public class FactionReputationLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventFactionReputation>
{
	public void HandleEvent(GameLogEventFactionReputation evt)
	{
		GameLogContext.Text = UIStrings.Instance.CharacterSheet.GetFactionLabel(evt.FactionType);
		GameLogContext.SecondText = $"{UtilityFaction.GetSpriteLabel(evt.ReputationType)}{Math.Abs(evt.Points)}";
		AddMessage(new CombatLogMessage((evt.Points < 0) ? LogThreadBase.Strings.FactionReputationLost.CreateCombatLogMessage() : LogThreadBase.Strings.FactionReputationGained.CreateCombatLogMessage(), null, hasTooltip: false));
	}
}
