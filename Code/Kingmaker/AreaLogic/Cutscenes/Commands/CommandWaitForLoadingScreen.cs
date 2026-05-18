using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandWaitForLoadingScreen")]
[TypeId("e746516e60d0440b83e7994335d9aeec")]
public class CommandWaitForLoadingScreen : CommandBase
{
	private class Data
	{
		public bool Finished;
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		player.GetCommandData<Data>(this).Finished = skipping;
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Finished = true;
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!LoadingProcess.Instance.IsLoadingScreenActive)
		{
			commandData.Finished = !LoadingProcess.Instance.IsFadeActive;
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Finished = true;
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "Await Loading Screen";
	}
}
