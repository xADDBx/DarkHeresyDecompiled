using System;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties;

public readonly struct PropertyContext
{
	public class Scope : SimpleContextData<PropertyContext, Scope>
	{
	}

	[NotNull]
	public readonly MechanicEntity CurrentEntity;

	[CanBeNull]
	public readonly MechanicsContext MechanicContext;

	[CanBeNull]
	public readonly RulebookEvent Rule;

	[CanBeNull]
	public readonly TargetWrapper CurrentTarget;

	[CanBeNull]
	public readonly AbilityData Ability;

	[CanBeNull]
	public MechanicEntity CurrentTargetEntity => CurrentTarget?.Entity;

	[CanBeNull]
	public MechanicEntityFact Fact => MechanicContext?.Fact;

	[CanBeNull]
	public Vector3? CurrentTargetPosition => CurrentTarget?.Point;

	[CanBeNull]
	public float? CurrentTargetOrientation => CurrentTarget?.Orientation;

	[CanBeNull]
	public (Vector3 Position, float Orientation)? CurrentTargetPositionAndOrientation
	{
		get
		{
			if (!CurrentTargetPosition.HasValue || !CurrentTargetOrientation.HasValue)
			{
				return null;
			}
			return (CurrentTargetPosition.Value, CurrentTargetOrientation.Value);
		}
	}

	[CanBeNull]
	public MechanicEntity ContextMainTarget => ContextMainTargetWrapper?.Entity;

	[CanBeNull]
	public Vector3? ContextMainTargetPosition => ContextMainTargetWrapper?.Point;

	[CanBeNull]
	public float? ContextMainTargetOrientation => ContextMainTargetWrapper?.Orientation;

	[CanBeNull]
	public (Vector3 Position, float Orientation)? ContextMainTargetPositionAndOrientation
	{
		get
		{
			if (!ContextMainTargetPosition.HasValue || !ContextMainTargetOrientation.HasValue)
			{
				return null;
			}
			return (ContextMainTargetPosition.Value, ContextMainTargetOrientation.Value);
		}
	}

	[CanBeNull]
	public MechanicEntity ContextCaster => MechanicContext?.MaybeCaster;

	[CanBeNull]
	public MechanicEntity ContextOwner => MechanicContext?.MaybeOwner;

	[CanBeNull]
	private TargetWrapper ContextMainTargetWrapper => MechanicContext?.ClickedTarget;

	[CanBeNull]
	public MechanicEntity RuleInitiator => Rule?.Initiator;

	[CanBeNull]
	public MechanicEntity RuleTarget => Rule?.Target;

	public PropertyContext(MechanicEntity currentEntity = null, MechanicsContext context = null, TargetWrapper currentTarget = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		CurrentEntity = currentEntity ?? context?.ClickedTarget.Entity ?? context?.MaybeOwner ?? throw new InvalidOperationException("Current Entity is missing");
		MechanicContext = context ?? SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current;
		CurrentTarget = currentTarget ?? SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Current ?? ((TargetWrapper)(rule?.Target));
		Rule = rule ?? ((RulebookEvent)SimpleContextData<IRulebookEvent, MechanicsContext.Scope.Rule>.Current);
		Ability = ability ?? Rule?.MaybeAbility ?? SimpleContextData<AbilityExecutionProcess, AbilityExecutionProcess.Scope>.Current?.Context.Ability ?? MechanicContext?.Ability ?? MechanicContext?.SourceAbility;
	}

	public int GetValue(PropertyCalculator calculator)
	{
		return calculator.GetValue(this);
	}

	public bool GetBool(PropertyCalculator calculator)
	{
		return calculator.GetValue(this) != 0;
	}

	public PropertyContext WithCurrentEntity([NotNull] MechanicEntity currentEntity)
	{
		return new PropertyContext(currentEntity, MechanicContext, CurrentTarget, Rule);
	}

	public PropertyContext WithCurrentTarget([NotNull] MechanicEntity currentTarget)
	{
		return new PropertyContext(CurrentEntity, MechanicContext, currentTarget, Rule);
	}

	public PropertyContext WithContext([NotNull] MechanicsContext context)
	{
		return new PropertyContext(CurrentEntity, context, CurrentTarget, Rule);
	}

	public PropertyContext WithRule([NotNull] RulebookEvent rule)
	{
		return new PropertyContext(CurrentEntity, MechanicContext, CurrentTarget, rule);
	}

	public DisposableBag SetScope()
	{
		return DisposableBag.Claim(SimpleContextData<PropertyContext, Scope>.Set(this), MechanicContext?.SetScope());
	}
}
