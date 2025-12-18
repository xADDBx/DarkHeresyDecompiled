using System.Linq;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Visual.Sound;

public class PowerBalanceAsksController : BaseAsksController, IPowerBalanceHandler, ISubscriber
{
	public void HandlePowerBalanceStateUpdate(MoraleGroup combatGroup, PowerBalanceState state)
	{
		if (state == PowerBalanceState.LosingBattle && combatGroup.IsPlayerEnemy)
		{
			SelectRandomAlivePartyMember()?.View.Asks?.WeAreWinning.Schedule();
		}
		else if (state == PowerBalanceState.LosingBattle && combatGroup.IsPlayerGroup)
		{
			SelectRandomAlivePartyMember()?.View.Asks?.WeAreLoosing.Schedule();
		}
		else if (state == PowerBalanceState.Shattered && combatGroup.IsPlayerEnemy)
		{
			combatGroup.Units.Where((BaseUnitEntity u) => u.CanAct).Random(PFStatefulRandom.Bark)?.View.Asks?.WeAreLostByMorale.Schedule();
		}
	}

	private BaseUnitEntity SelectRandomAlivePartyMember()
	{
		return Game.Instance.Player.Party.Where((BaseUnitEntity p) => p.CanAct).Random(PFStatefulRandom.Bark);
	}

	public void HandlePowerBalanceValueUpdate(MoraleGroup combatGroup)
	{
	}

	public void HandlePowerBalanceRecalculated()
	{
	}
}
