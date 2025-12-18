using System;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Gameplay.Features.Elevator;

public sealed class ElevatorController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (ElevatorPlatformEntity elevatorPlatform in Game.Instance.EntityPools.ElevatorPlatforms)
		{
			try
			{
				elevatorPlatform.Update();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}
}
