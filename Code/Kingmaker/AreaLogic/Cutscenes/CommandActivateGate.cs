using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes;

[ComponentName("Command/CommandActivateGate")]
[TypeId("bd6bb1af71d936e40b120344853cd3fc")]
public class CommandActivateGate : CommandBase
{
	public CommandSignalData SignalData = new CommandSignalData
	{
		Name = "Gate"
	};

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		player.SignalGateExtra(SignalData.GateId);
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
		return "<b>Activate Gate</b> ";
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		return new CommandSignalData[1] { SignalData };
	}
}
