using System;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes;

[ComponentName("Command/CommandDefaultTeleport")]
[TypeId("554a0a242cc4945418b3aed55202f609")]
public class CommandDefaultTeleport : CommandBase
{
	private const string ParamName = "TeleportDestination";

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!(player.Parameters.Params["TeleportDestination"] is BlueprintAreaEnterPoint blueprintAreaEnterPoint))
		{
			throw new InvalidOperationException("Cant find teleport target");
		}
		if (blueprintAreaEnterPoint.Area != Game.Instance.CurrentlyLoadedArea)
		{
			throw new InvalidOperationException($"Cant teleport to another map. Target map {blueprintAreaEnterPoint.Area}, current map {Game.Instance.CurrentlyLoadedArea}");
		}
		AreaEnterPoint areaEnterPoint = AreaEnterPoint.FindAreaEnterPointOnScene(blueprintAreaEnterPoint);
		if (areaEnterPoint == null)
		{
			throw new InvalidOperationException($"Cant find position of enter point {blueprintAreaEnterPoint}");
		}
		Game.Instance.Player.MoveCharacters(areaEnterPoint, moveFollowers: false, moveCamera: false);
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "<b>Teleport</b> to default destination";
	}
}
