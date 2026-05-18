using System;

namespace Kingmaker.Framework.ContextContract;

public static class ContextEntryPointContracts
{
	private static readonly ContextContract _AbilityExecutionScopeBase;

	private static readonly ContextContract _FeatureLifecycleContract;

	private static readonly ContextContract _BuffLifecycleContract;

	public static ContextContract For(ContextEntryPointKind kind)
	{
		return kind switch
		{
			ContextEntryPointKind.AbilityCastRestriction => ContextContract.Cold(), 
			ContextEntryPointKind.AbilityOnCast => _AbilityExecutionScopeBase.With(ContextField.Target, Availability.Definitely), 
			ContextEntryPointKind.AbilityTargetValidation => ContextContract.Cold(), 
			ContextEntryPointKind.AbilityDelivery => _AbilityExecutionScopeBase.With(ContextField.Target, Availability.Definitely).With(ContextField.Pattern, Availability.Maybe), 
			ContextEntryPointKind.AbilityPatternTargetSelection => _AbilityExecutionScopeBase.With(ContextField.Target, Availability.Definitely).With(ContextField.Pattern, Availability.Definitely), 
			ContextEntryPointKind.AbilityLoSRangeException => ContextContract.Cold(), 
			ContextEntryPointKind.AbilityApplyEffect => _AbilityExecutionScopeBase.With(ContextField.Target, Availability.Definitely).With(ContextField.CurrentEntity, Availability.Definitely).With(ContextField.Pattern, Availability.Maybe), 
			ContextEntryPointKind.AbilityHaloOuter => _AbilityExecutionScopeBase.With(ContextField.Pattern, Availability.Definitely).With(ContextField.Target, Availability.Definitely), 
			ContextEntryPointKind.AbilityHaloInner => _AbilityExecutionScopeBase.With(ContextField.Pattern, Availability.Definitely).With(ContextField.CurrentEntity, Availability.Definitely).With(ContextField.Target, Availability.Definitely), 
			ContextEntryPointKind.AbilityAdditionalTargets => _AbilityExecutionScopeBase.With(ContextField.Target, Availability.Definitely).With(ContextField.CurrentEntity, Availability.Maybe).With(ContextField.Pattern, Availability.Maybe), 
			ContextEntryPointKind.AbilityCustomLogicCleanup => ContextContract.Cold(), 
			ContextEntryPointKind.BuffComponentLifecycle => _BuffLifecycleContract, 
			ContextEntryPointKind.BuffComponentRulebookHandler => _BuffLifecycleContract.With(ContextField.Rule, Availability.Definitely), 
			ContextEntryPointKind.BuffComponent => _BuffLifecycleContract, 
			ContextEntryPointKind.FeatureComponentLifecycle => _FeatureLifecycleContract, 
			ContextEntryPointKind.FeatureComponentRulebookHandler => _FeatureLifecycleContract.With(ContextField.Rule, Availability.Definitely), 
			_ => throw new ArgumentOutOfRangeException("kind", kind, null), 
		};
	}

	static ContextEntryPointContracts()
	{
		ContextContract.Builder builder = ContextContract.Builder.New();
		builder = builder.Set(ContextField.Caster, Availability.Definitely);
		builder = builder.Set(ContextField.Owner, Availability.Definitely);
		builder = builder.Set(ContextField.Ability, Availability.Definitely);
		builder = builder.Set(ContextField.Blueprint, Availability.Definitely);
		builder = builder.Set(ContextField.ClickedTarget, Availability.Definitely);
		builder = builder.Set(ContextField.Fact, Availability.Definitely);
		builder = builder.Set(ContextField.Direction, Availability.Definitely);
		builder = builder.Set(ContextField.SourceCastPosition, Availability.Definitely);
		builder = builder.Set(ContextField.SourceAbility, Availability.Definitely);
		builder = builder.Set(ContextField.SourceCaster, Availability.Definitely);
		builder = builder.Set(ContextField.SourceBlueprint, Availability.Definitely);
		builder = builder.Set(ContextField.SourceFact, Availability.Definitely);
		builder = builder.Set(ContextField.SourceAbilityBlueprint, Availability.Definitely);
		builder = builder.Set(ContextField.SourceClickedTarget, Availability.Definitely);
		_AbilityExecutionScopeBase = builder.Freeze();
		builder = ContextContract.Builder.New();
		builder = builder.Set(ContextField.Caster, Availability.Definitely);
		builder = builder.Set(ContextField.Owner, Availability.Definitely);
		builder = builder.Set(ContextField.Fact, Availability.Definitely);
		builder = builder.Set(ContextField.Blueprint, Availability.Definitely);
		builder = builder.Set(ContextField.FactComponent, Availability.Definitely);
		builder = builder.Set(ContextField.Ability, Availability.Maybe);
		builder = builder.Set(ContextField.ClickedTarget, Availability.Definitely);
		builder = builder.Set(ContextField.Target, Availability.Definitely);
		builder = builder.Set(ContextField.SourceCaster, Availability.Definitely);
		builder = builder.Set(ContextField.SourceBlueprint, Availability.Definitely);
		builder = builder.Set(ContextField.SourceFact, Availability.Definitely);
		builder = builder.Set(ContextField.SourceClickedTarget, Availability.Definitely);
		builder = builder.Set(ContextField.SourceAbility, Availability.Maybe);
		builder = builder.Set(ContextField.SourceAbilityBlueprint, Availability.Maybe);
		_FeatureLifecycleContract = builder.Freeze();
		builder = ContextContract.Builder.New();
		builder = builder.Set(ContextField.Caster, Availability.Definitely);
		builder = builder.Set(ContextField.Owner, Availability.Definitely);
		builder = builder.Set(ContextField.Fact, Availability.Definitely);
		builder = builder.Set(ContextField.Blueprint, Availability.Definitely);
		builder = builder.Set(ContextField.FactComponent, Availability.Definitely);
		builder = builder.Set(ContextField.Ability, Availability.Maybe);
		builder = builder.Set(ContextField.ClickedTarget, Availability.Definitely);
		builder = builder.Set(ContextField.Target, Availability.Definitely);
		builder = builder.Set(ContextField.SourceCaster, Availability.Definitely);
		builder = builder.Set(ContextField.SourceBlueprint, Availability.Definitely);
		builder = builder.Set(ContextField.SourceFact, Availability.Definitely);
		builder = builder.Set(ContextField.SourceClickedTarget, Availability.Definitely);
		builder = builder.Set(ContextField.SourceAbility, Availability.Maybe);
		builder = builder.Set(ContextField.SourceAbilityBlueprint, Availability.Maybe);
		_BuffLifecycleContract = builder.Freeze();
	}
}
