using Kingmaker.GameModes;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.Code.Framework.GameLog;

public class PartyGainExperienceLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventPartyGainExperience>
{
	public void HandleEvent(GameLogEventPartyGainExperience evt)
	{
		if (!(Game.Instance.CurrentModeType != GameModeType.SpaceCombat) || !evt.IsExperienceForDeath)
		{
			GameLogContext.Count = evt.Experience * Game.Instance.Player.ExperienceRatePercent / 100;
			AddMessage(new CombatLogMessage(LogThreadBase.Strings.XpGain.CreateCombatLogMessage(), null, hasTooltip: false));
		}
	}
}
