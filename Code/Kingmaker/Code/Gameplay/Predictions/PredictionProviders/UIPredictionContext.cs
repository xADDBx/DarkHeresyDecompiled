using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public struct UIPredictionContext
{
	public AbilityData Ability;

	public MechanicEntity Target;

	public MechanicEntity PointerTarget;

	public Vector3 CasterPosition;

	public IReadOnlyList<MechanicEntity> TargetsInPattern;

	public BlueprintBodyPart BodyPart;
}
