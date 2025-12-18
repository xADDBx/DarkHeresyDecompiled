using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers;

public class HealthController : IController, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (!Game.Instance.IsSpaceCombat)
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			PartHealth partHealth = mechanicEntity?.GetHealthOptional();
			if (partHealth != null && !TurnController.IsInTurnBasedCombat() && mechanicEntity.IsInPlayerParty && !mechanicEntity.Features.DoNotReviveOutOfCombat)
			{
				partHealth.HealDamageAll();
				mechanicEntity.GetArmorOptional()?.HealDamageAll();
			}
		}
	}
}
