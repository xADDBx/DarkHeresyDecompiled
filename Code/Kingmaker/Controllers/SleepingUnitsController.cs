using System.Collections.Generic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers;

public class SleepingUnitsController : IControllerTick, IController, IControllerStart, IControllerStop, IAreaHandler, ISubscriber
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void OnStart()
	{
		Tick();
	}

	public void OnStop()
	{
		Game.Instance.EntityPools.ClearAwakeUnits();
	}

	public void OnAreaBeginUnloading()
	{
		Game.Instance.EntityPools.ClearAwakeUnits();
	}

	public void OnAreaDidLoad()
	{
	}

	public void Tick()
	{
		using (ProfileScope.New("Units"))
		{
			List<AbstractUnitEntity> list = ListPool<AbstractUnitEntity>.Claim();
			foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
			{
				using (ProfileScope.New("Update dead bodies"))
				{
					if (allUnit.LifeState.IsFinallyDead)
					{
						allUnit.LifeState.IsDeathRevealed = allUnit.IsInCameraFrustum && allUnit.IsVisibleForPlayer;
					}
				}
				bool isSleeping = allUnit.IsSleeping;
				if (allUnit.AwakeTimer >= 0f)
				{
					allUnit.AwakeTimer -= Game.Instance.Controllers.TimeController.DeltaTime;
				}
				if (allUnit.AwakeTimerTicks > 0)
				{
					allUnit.AwakeTimerTicks--;
				}
				allUnit.IsSleeping = ShouldBeSleeping(allUnit);
				if (allUnit.IsSleeping != isSleeping && (bool)allUnit.View)
				{
					allUnit.View.UpdateViewActive();
				}
				if (!allUnit.IsSleeping)
				{
					list.Add(allUnit);
				}
			}
			Game.Instance.EntityPools.SetNewAwakeUnits(list);
			ListPool<AbstractUnitEntity>.Release(list);
		}
		using (ProfileScope.New("Groups"))
		{
			Game.Instance.UnitGroups.UpdateAwakeGroups();
		}
	}

	private static bool ShouldBeSleeping(AbstractUnitEntity unit)
	{
		if (!unit.IsInGame || (unit.Suppressed && !unit.IsInCombat))
		{
			return true;
		}
		if (!unit.Sleepless && unit.FreezeOutsideCamera && !unit.IsInCombat && CutsceneControlledUnit.IsFreezingAllowed(unit) && !unit.IsInCameraFrustum)
		{
			return true;
		}
		if ((bool)unit.Sleepless)
		{
			return false;
		}
		if (!CutsceneControlledUnit.IsSleepingAllowed(unit))
		{
			return false;
		}
		if (unit.IsExtra && !unit.IsDead && (unit.IsInFogOfWar || !unit.IsInCameraFrustum))
		{
			return true;
		}
		if (unit.IsInFogOfWar && !unit.IsInCombat)
		{
			return unit.Commands.Empty;
		}
		return false;
	}
}
