using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.UnitLogic.Commands;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

public sealed class DetectiveServoskullController : IControllerTick, IController
{
	private static DetectiveServoskullRoot Settings => ConfigRoot.Instance.DetectiveServoskull;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		PartDetectiveServoSkull partDetectiveServoSkull = PartDetectiveServoSkull.Find();
		if (partDetectiveServoSkull != null)
		{
			if (!partDetectiveServoSkull.IsBusy)
			{
				FollowLeader(partDetectiveServoSkull);
			}
			else
			{
				StopFollowingLeader(partDetectiveServoSkull);
			}
		}
	}

	private static void StopFollowingLeader(PartDetectiveServoSkull servoskull)
	{
		if (servoskull.Owner.Commands.Current is UnitFollow unitFollow)
		{
			unitFollow.Interrupt();
		}
	}

	private void FollowLeader(PartDetectiveServoSkull servoskull)
	{
		if ((servoskull.Owner.Position - servoskull.Leader.Position).magnitude > Settings.TeleportWhenDistanceToLeaderIsGreaterThan)
		{
			servoskull.Owner.Position = servoskull.IdlePosition;
			servoskull.Owner.DesiredOrientation = servoskull.Leader.DesiredOrientation;
		}
		else
		{
			UnitFollowParams cmdParams = new UnitFollowParams(servoskull.Leader, servoskull.IdlePosition)
			{
				ForceMove = true
			};
			servoskull.Owner.Commands.Run(cmdParams);
		}
	}

	private static void UpdateFlyingHeight(PartDetectiveServoSkull servoskull)
	{
		float deltaTime = Game.Instance.Controllers.TimeController.DeltaTime;
		servoskull.FlyHeightDeviationProgress += Settings.FlyHeightDeviationSpeed * deltaTime;
		servoskull.FlyHeightDifference = ((servoskull.FlyHeightDifference > 0f) ? Math.Max(0f, servoskull.FlyHeightDifference - deltaTime * 2f) : Math.Min(0f, servoskull.FlyHeightDifference + deltaTime * 2f));
		float num = Settings.FlyingHeightDeviation.Evaluate(servoskull.FlyHeightDeviationProgress);
		float num2 = (servoskull.IsBusy ? Settings.ScanFlyHeight : Settings.FlyHeight);
		float num3 = servoskull.FlyHeightDifference + num2 + num;
		if (Math.Abs(servoskull.Owner.FlyHeight - num3) > 0.01f)
		{
			servoskull.Owner.FlyHeight = ((servoskull.Owner.FlyHeight > num3) ? Math.Max(num3, servoskull.Owner.FlyHeight - deltaTime) : Math.Min(num3, servoskull.Owner.FlyHeight + deltaTime));
			servoskull.Owner.View?.ForcePlaceAboveGround();
		}
	}
}
