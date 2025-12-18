using System;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Framework.GlobalEffectSystem;

[Serializable]
[TypeId("2f630aebfe6b413aa5a5c9521c8c7335")]
public sealed class SetGlobalEffect : MechanicEntityFactComponentDelegate
{
	[ValidateNotNull]
	public BpRef<BlueprintGlobalEffect> Effect = new BpRef<BlueprintGlobalEffect>();

	[Range(0f, 1f)]
	public float Weight = 1f;

	[Min(0f)]
	public int Priority;

	protected override void OnActivateOrPostLoad()
	{
		GlobalEffectDirector.Shared.SetWeightFromDesigner(Effect.Blueprint, Weight, Priority, base.Runtime);
	}

	protected override void OnDeactivate()
	{
		GlobalEffectDirector.Shared.RemoveWeightFromDesigner(base.Runtime);
	}
}
