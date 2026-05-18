using System;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Gameplay.Features.Elevator;

public sealed class ElevatorControllerLate : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public void Tick()
	{
		float interpolationProgress = Game.Instance.RealTimeController.InterpolationProgress;
		foreach (ElevatorPlatformEntity elevatorPlatform in Game.Instance.EntityPools.ElevatorPlatforms)
		{
			try
			{
				InterpolateView(elevatorPlatform, interpolationProgress);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	private static void InterpolateView(ElevatorPlatformEntity elevator, float progress)
	{
		if (elevator.View is ElevatorPlatformView elevatorPlatformView)
		{
			elevatorPlatformView.Interpolate(progress);
		}
	}
}
