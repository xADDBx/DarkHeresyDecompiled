using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace Kingmaker.Framework.Abilities;

public interface IAbilityPipelineHandler
{
	void OnProcessStart(AbilityExecutionContext context);

	void PrepareCast(AbilityExecutionContext context);

	void OnProcessEnd(AbilityExecutionContext context);

	void OnProcessCleanup(AbilityExecutionContext context);

	void SpawnFxs(AbilityExecutionContext context, AbilitySpawnFxTime time, [CanBeNull] TargetWrapper selectedTarget = null);

	IEnumerator<AbilityDeliveryTarget> DeliverTargets(AbilityDeliverEffect deliverEffect, AbilityExecutionContext context, TargetWrapper clickedTarget);

	void InvokeHaloEffect(AbilityHaloEffect haloEffect, AbilityExecutionContext context);

	IEnumerable<AbilityDeliveryTarget> GetAdditionalTargets(AbilityAdditionalTargets component, AbilityExecutionContext context);

	bool ShouldApplyToDeliveryTarget(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget);

	bool IsImmune(AbilityExecutionContext context, AbilityDeliveryTarget target);

	void InvokeApplyEffect([CanBeNull] AbilityApplyEffect applyEffect, AbilityExecutionContext context, TargetWrapper target);

	void OnEffectApplied(AbilityExecutionContext context);

	void OnTryToApplyToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target);

	void OnEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target);
}
