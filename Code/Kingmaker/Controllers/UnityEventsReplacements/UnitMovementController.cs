using Kingmaker.Controllers.Interfaces;
using Kingmaker.Pathfinding;

namespace Kingmaker.Controllers.UnityEventsReplacements;

public class UnitMovementController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		AstarPath active = AstarPath.active;
		if (!(active != null) || !Game.Instance.CurrentlyLoadedArea.IsNavmeshArea)
		{
			return;
		}
		foreach (UpdateHook instance in UpdateHook.Instances)
		{
			if (instance != null)
			{
				instance.Tick();
			}
		}
		active.Tick();
	}
}
