using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Obsolete("Unused in code")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("b557f8c8bd8e67440b84e93292d2e370")]
public class AbilityActionCamera : BlueprintComponent
{
	private enum CameraTargetType
	{
		Target,
		Caster
	}

	[Header("Сейчас выбор таргет или кастер не влияет ни на что")]
	[SerializeField]
	private CameraTargetType CameraFollow;

	[SerializeField]
	[Range(0f, 100f)]
	private int TriggerActionCameraChance = 30;

	public AbilityActionCameraSettings GetSettings(UnitUseAbility abilityCommand)
	{
		return default(AbilityActionCameraSettings);
	}
}
