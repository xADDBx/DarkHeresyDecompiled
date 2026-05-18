using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandWaitForCombatEnd")]
[TypeId("81b7d7d16fb94c3ab5eff545982da12a")]
public class CommandWaitForCombatEnd : CommandBase
{
	private class Data
	{
		public double CurrentTime;

		public bool TimedOut;
	}

	[SerializeField]
	[Tooltip("The command will end even in combat if it takes longer than this")]
	private float m_TimeOut = 60f;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		player.ClearCommandData(this);
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.CurrentTime = time;
		if (commandData.CurrentTime > (double)m_TimeOut)
		{
			commandData.TimedOut = true;
			return CommandResult.FailWithReport("Command " + name + " in " + player.Cutscene.name + " is taking too long");
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
		if (player.GetCommandData<Data>(this).TimedOut)
		{
			return true;
		}
		return !Game.Instance.Player.IsInCombat;
	}

	public override string GetCaption()
	{
		return "Wait for combat to end";
	}
}
