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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return Flag.IsUnlocked;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
