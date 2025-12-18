using System;
using Kingmaker.AreaLogic.Etudes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Framework.GlobalEffectSystem;

[Serializable]
[TypeId("afe434007e2040a1af1803e727ce2d47")]
public sealed class EtudeBracketSetGlobalEffect : EtudeBracketTrigger
{
	[ValidateNotNull]
	public BpRef<BlueprintGlobalEffect> Effect = new BpRef<BlueprintGlobalEffect>();

	[Range(0f, 1f)]
	public float Weight = 1f;

	[Min(0f)]
	public int Priority;

	protected override void OnEnter()
	{
		GlobalEffectDirector.Shared.SetWeightFromDesigner(Effect.Blueprint, Weight, Priority, base.Runtime);
	}

	protected override void OnResume()
	{
		GlobalEffectDirector.Shared.SetWeightFromDesigner(Effect.Blueprint, Weight, Priority, base.Runtime);
	}

	protected override void OnExit()
	{
		GlobalEffectDirector.Shared.RemoveWeightFromDesigner(base.Runtime);
	}
}
