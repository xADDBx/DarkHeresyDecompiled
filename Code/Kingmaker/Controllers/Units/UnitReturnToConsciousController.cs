using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers.Units;

public class UnitReturnToConsciousController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			PartLifeState lifeState = allUnit.LifeState;
			if (lifeState != null && !allUnit.IsInCombat && !allUnit.Features.DoNotHealOutOfCombat && !lifeState.IsConscious && !lifeState.IsFinallyDead)
			{
				MakeUnitConscious(allUnit);
			}
		}
	}

	public static void MakeUnitConscious(AbstractUnitEntity unit)
	{
		UnitLifeController.ForceUnitConscious(unit);
	}
}
