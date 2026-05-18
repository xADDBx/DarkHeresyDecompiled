using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("cef37152812a4ba9b58d62fa76bc252c")]
public class AbilityAttackSpawnAreaEffect : BlueprintComponent
{
	public BlueprintAreaEffectReference AreaEffect;

	public bool OverridePatternWithAttackPattern;

	public ContextDurationValue DurationValue;

	[SerializeField]
	[Obsolete("Unused")]
	[InspectorReadOnly]
	private bool m_GetOrientationFromCaster;

	private BlueprintAreaEffect BlueprintAreaEffect => AreaEffect.Get();

	public void SpawnAreaEffect(AbilityExecutionContext context, TargetWrapper target)
	{
	}
}
