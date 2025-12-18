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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		player.SignalGateExtra(SignalData.GateId);
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
		return "<b>Activate Gate</b> ";
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		return new CommandSignalData[1] { SignalData };
	}
}
