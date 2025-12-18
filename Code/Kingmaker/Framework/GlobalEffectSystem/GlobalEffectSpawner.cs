using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Framework.GlobalEffectSystem;

public sealed class GlobalEffectSpawner : MonoBehaviour
{
	[ValidateNotNull]
	public BpRef<BlueprintGlobalEffect> Effect = new BpRef<BlueprintGlobalEffect>();

	[Range(0f, 1f)]
	public float Weight = 1f;

	private void OnEnable()
	{
		GlobalEffectDirector.Shared.SetWeightFromScene(Effect, () => Weight);
	}

	private void OnDisable()
	{
		GlobalEffectDirector.Shared.RemoveWeightFromScene(Effect);
	}
}
