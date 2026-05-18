using System;
using Kingmaker.AreaLogic.Cutscenes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Framework.GlobalEffectSystem;

[Serializable]
[TypeId("528d137541a14739b6bd12ac48107f3d")]
public sealed class CommandGlobalEffectSpan : CommandBase
{
	[ValidateNotNull]
	public BpRef<BlueprintGlobalEffect> Effect = new BpRef<BlueprintGlobalEffect>();

	[Range(0f, 1f)]
	public float Weight = 1f;

	[Min(0f)]
	public int Priority;

	public override bool IsContinuous => true;

	public override string GetCaption()
	{
		return $"Set global effect {Effect}={Weight:F2} (priority {Priority})";
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		GlobalEffectDirector.Shared.SetWeightFromDesigner(Effect.Blueprint, Weight, Priority, this);
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		GlobalEffectDirector.Shared.RemoveWeightFromDesigner(this);
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}
}
