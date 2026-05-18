using System.Linq;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
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
			BaseUnitEntity baseUnitEntity = SelectRandomAlivePartyMember();
			if (baseUnitEntity != null)
			{
				using (EvalContext.PushAsksContext(baseUnitEntity, baseUnitEntity))
				{
					baseUnitEntity.View.Asks?.WeAreWinning.Schedule();
				}
			}
		}
		else if (state == PowerBalanceState.LosingBattle && combatGroup.IsPlayerGroup)
		{
			BaseUnitEntity baseUnitEntity2 = SelectRandomAlivePartyMember();
			if (baseUnitEntity2 != null)
			{
				using (EvalContext.PushAsksContext(baseUnitEntity2, baseUnitEntity2))
				{
					baseUnitEntity2.View.Asks?.WeAreLoosing.Schedule();
				}
			}
		}
		else
		{
			if (state != PowerBalanceState.Shattered || !combatGroup.IsPlayerEnemy)
			{
				return;
			}
			BaseUnitEntity baseUnitEntity3 = combatGroup.Units.Where((BaseUnitEntity u) => u.CanAct).Random(PFStatefulRandom.Bark);
			if (baseUnitEntity3 != null)
			{
				using (EvalContext.PushAsksContext(baseUnitEntity3, baseUnitEntity3))
				{
					baseUnitEntity3.View.Asks?.WeAreLostByMorale.Schedule();
				}
			}
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
