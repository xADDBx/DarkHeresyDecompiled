using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventPartyUseAbility : GameLogEvent<GameLogEventPartyUseAbility>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IPartyUseAbilityHandler, ISubscriber
	{
		public void HandleUseAbility(AbilityData ability)
		{
			AddEvent(new GameLogEventPartyUseAbility(ability));
		}
	}

	public readonly AbilityData Ability;

	public GameLogEventPartyUseAbility(AbilityData ability)
	{
		Ability = ability;
	}
}
