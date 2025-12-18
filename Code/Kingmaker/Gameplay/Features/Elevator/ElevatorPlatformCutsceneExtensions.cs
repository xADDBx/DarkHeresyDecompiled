using System;
using Kingmaker.AreaLogic.Cutscenes;

namespace Kingmaker.Gameplay.Features.Elevator;

public static class ElevatorPlatformCutsceneExtensions
{
	public static ElevatorPlatformEntity? GetElevatorPlatformOptional(this CutscenePlayerData player)
	{
		return player.Parameters.Params["Elevator"].Value as ElevatorPlatformEntity;
	}

	public static ElevatorPlatformEntity GetElevatorPlatform(this CutscenePlayerData player)
	{
		return player.GetElevatorPlatformOptional() ?? throw new InvalidOperationException("ElevatorPlatformEntity is missing in cutscene parameters by key Elevator");
	}
}
