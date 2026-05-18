using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/WaitFlag")]
[TypeId("09e0c54cbcb526c44bf099f8ea03d42d")]
public class WaitFlag : CommandBase
{
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag?.Get();

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
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
		return Flag.IsUnlocked;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}
}
