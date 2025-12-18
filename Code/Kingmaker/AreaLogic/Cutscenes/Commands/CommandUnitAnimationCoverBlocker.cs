using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandUnitAnimationCoverBlocker")]
[TypeId("4394fb3648d14b5e95ab4111fc3fcff7")]
public class CommandUnitAnimationCoverBlocker : CommandBase
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool CoverAvailable;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		UnitAnimationManager animationManager = Unit.GetValue().View.AnimationManager;
		if (!(animationManager == null))
		{
			animationManager.InCutsceneCoverAvailable = CoverAvailable;
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}
}
