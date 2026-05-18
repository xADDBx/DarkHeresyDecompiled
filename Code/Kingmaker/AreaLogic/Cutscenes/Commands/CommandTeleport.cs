using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandTeleport")]
[TypeId("96c32a6355a5cbe419f21098bb7591d4")]
public class CommandTeleport : CommandBase
{
	[InfoBox("If this command is used to teleport to a different area, the cutscene would be stopped completely.\nDo not do this unless this is the last command in the cutscene.")]
	public BlueprintAreaEnterPointReference TargetPoint;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (TargetPoint.Get().Area == Game.Instance.CurrentlyLoadedArea)
		{
			Game.Instance.Teleport(TargetPoint.Get());
		}
		else
		{
			player.Stop();
			Game.Instance.LoadArea(TargetPoint, AutoSaveMode.None);
		}
		return CommandResult.Success;
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		return false;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return !LoadingProcess.Instance.IsLoadingInProcess;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "Teleport to (" + TargetPoint.NameSafe() + ")";
	}
}
