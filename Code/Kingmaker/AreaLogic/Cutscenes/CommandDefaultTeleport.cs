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

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!(player.Parameters.Params["TeleportDestination"] is BlueprintAreaEnterPoint blueprintAreaEnterPoint))
		{
			return CommandResult.Fail("Cant find teleport destination TeleportDestination");
		}
		if (blueprintAreaEnterPoint.Area != Game.Instance.CurrentlyLoadedArea)
		{
			return CommandResult.Fail($"Cant teleport to another map. Target map {blueprintAreaEnterPoint.Area}, current map {Game.Instance.CurrentlyLoadedArea}");
		}
		AreaEnterPoint areaEnterPoint = AreaEnterPoint.FindAreaEnterPointOnScene(blueprintAreaEnterPoint);
		if (areaEnterPoint == null)
		{
			return CommandResult.Fail($"Cant find area enter point {blueprintAreaEnterPoint}");
		}
		Game.Instance.Player.MoveCharacters(areaEnterPoint, moveFollowers: false, moveCamera: false);
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return OnRun(player, skipping: true);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "<b>Teleport</b> to default destination";
	}
}
