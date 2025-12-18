namespace Kingmaker.Code.Framework.GameLog;

public class PartyUseAbilityLogThread : BaseUseAbilityLogThread, IGameLogEventHandler<GameLogEventPartyUseAbility>
{
	public void HandleEvent(GameLogEventPartyUseAbility evt)
	{
		HandleUseAbility(evt.Ability, null);
	}
}
