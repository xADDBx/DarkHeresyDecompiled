using System;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
public sealed class AbilitySpawnAreaEffectSettings
{
	[SerializeField]
	private BpRef<BlueprintAreaEffect> m_Blueprint;

	[Tooltip("Если true, то эффект поспавнится мгновенно, не дожидаясь проджектайлов и делеев")]
	public bool SpawnImmediately;

	public bool UseAttackPattern;

	public ContextDurationValue DurationValue;

	[InspectorReadOnly]
	[InfoBox("В данный момент не поддерживается (и непонятно, для каких кейсов это нужно). Если понадобится - пишите программистам.")]
	public bool GetOrientationFromCaster;

	[CanBeNull]
	public BlueprintAreaEffect Blueprint => m_Blueprint;
}
