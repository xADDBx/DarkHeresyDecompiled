using Kingmaker.Blueprints.Attributes;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandDelay")]
[TypeId("ce685e563b57ba14b8c03ba1ef90e435")]
public class CommandDelay : CommandBase
{
	private class Data
	{
		public float Time;

		public bool Finished;
	}

	public float Time;

	[ShowIf("Random")]
	public float MaxTime;

	public bool Random;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		player.GetCommandData<Data>(this).Time = (skipping ? 0.0001f : (Random ? player.Random.Range(Time, MaxTime) : Time));
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
		commandData.Finished = time >= (double)commandData.Time;
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Finished = true;
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		if (!Random)
		{
			return $"{Time} s";
		}
		return $"{Time}-{MaxTime} s";
	}
}
