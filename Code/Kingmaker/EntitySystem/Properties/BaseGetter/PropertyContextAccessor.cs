using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.BaseGetter;

public static class PropertyContextAccessor
{
	public interface IBase
	{
	}

	public interface IRequired : IBase
	{
	}

	public interface IOptional : IBase
	{
	}

	public interface IMechanicContext : IRequired, IBase
	{
	}

	public interface IRule : IRequired, IBase
	{
	}

	public interface IAbility : IRequired, IBase
	{
	}

	public interface IContextCaster : IRequired, IBase
	{
	}

	public interface IContextMainTargetEntity : IRequired, IBase
	{
	}

	public interface IContextMainTarget : IRequired, IBase
	{
	}

	public interface ICurrentTargetEntity : IRequired, IBase
	{
	}

	public interface ICurrentTarget : IRequired, IBase
	{
	}

	public interface IRuleInitiator : IRequired, IBase
	{
	}

	public interface IRuleTarget : IRequired, IBase
	{
	}

	public interface IAbilityWeapon : IRequired, IBase
	{
	}

	public interface ITargetByType : IRequired, IBase
	{
	}

	public interface IOwnerBlueprint : IRequired, IBase
	{
	}

	public interface IOptionalMechanicContext : IOptional, IBase
	{
	}

	public interface IOptionalRule : IOptional, IBase
	{
	}

	public interface IOptionalAbility : IOptional, IBase
	{
	}

	public interface IOptionalContextCaster : IOptional, IBase
	{
	}

	public interface IOptionalContextMainTargetEntity : IRequired, IBase
	{
	}

	public interface IOptionalContextMainTarget : IOptional, IBase
	{
	}

	public interface IOptionalCurrentTargetEntity : IOptional, IBase
	{
	}

	public interface IOptionalCurrentTarget : IOptional, IBase
	{
	}

	public interface IOptionalRuleInitiator : IOptional, IBase
	{
	}

	public interface IOptionalRuleTarget : IOptional, IBase
	{
	}

	public interface IOptionalAbilityWeapon : IOptional, IBase
	{
	}

	public interface IOptionalTargetByType : IOptional, IBase
	{
	}

	public interface IOptionalFact : IOptional, IBase
	{
	}

	private static IEvalContext Context => EvalContext.Current;

	private static Exception BuildException<T>(this T _) where T : IBase
	{
		return new Exception("Failed to get property by " + typeof(T).Name);
	}

	[NotNull]
	public static IEvalContext GetEvalContext(this IMechanicContext accessor)
	{
		return Context ?? throw accessor.BuildException();
	}

	[NotNull]
	public static RulebookEvent GetRule(this IRule accessor)
	{
		return Context.Rule ?? throw accessor.BuildException();
	}

	[NotNull]
	public static AbilityData GetAbility(this IAbility accessor)
	{
		return Context.Ability ?? throw accessor.BuildException();
	}

	[NotNull]
	public static Entity GetContextCaster(this IContextCaster accessor)
	{
		return Context.Caster ?? throw accessor.BuildException();
	}

	[NotNull]
	public static MechanicEntity GetCurrentTarget(this ICurrentTargetEntity accessor)
	{
		return Context.CurrentTargetEntity ?? throw accessor.BuildException();
	}

	public static Vector3 GetCurrentTargetPosition(this ICurrentTarget accessor)
	{
		return (Context.Target ?? throw accessor.BuildException()).Point;
	}

	public static float GetCurrentTargetOrientation(this ICurrentTarget accessor)
	{
		return (Context.Target ?? throw accessor.BuildException()).Orientation;
	}

	[NotNull]
	public static MechanicEntity GetContextMainTarget(this IContextMainTargetEntity accessor)
	{
		return Context.ClickedTarget?.Entity ?? throw accessor.BuildException();
	}

	public static Vector3 GetContextMainTargetPosition(this IContextMainTarget accessor)
	{
		return (Context.ClickedTarget ?? throw accessor.BuildException()).Point;
	}

	public static float GetContextMainTargetOrientation(this IContextMainTarget accessor)
	{
		return (Context.ClickedTarget ?? throw accessor.BuildException()).Orientation;
	}

	[NotNull]
	public static Entity GetRuleInitiator(this IRuleInitiator accessor)
	{
		return Context.Rule?.Initiator ?? throw accessor.BuildException();
	}

	[NotNull]
	public static Entity GetRuleTarget(this IRuleTarget accessor)
	{
		return Context.Rule?.Target ?? throw accessor.BuildException();
	}

	[NotNull]
	public static ItemEntityWeapon GetAbilityWeapon(this IAbilityWeapon accessor)
	{
		return Context.AbilityWeapon ?? throw accessor.BuildException();
	}

	[NotNull]
	public static MechanicEntity GetTargetByType(this ITargetByType accessor, PropertyTargetType type)
	{
		return Context.GetEntityByType(type) ?? throw accessor.BuildException();
	}

	public static Vector3 GetTargetPositionByType(this ITargetByType accessor, PropertyTargetType type)
	{
		return (Context.GetEntityByType(type) ?? throw accessor.BuildException()).Position;
	}

	public static IntRect GetTargetRectByType(this ITargetByType accessor, PropertyTargetType type)
	{
		return ((Context.GetEntityByType(type) as BaseUnitEntity) ?? throw accessor.BuildException()).SizeRect;
	}

	public static BlueprintScriptableObject GetOwnerBlueprint(this IOwnerBlueprint accessor)
	{
		return (BlueprintScriptableObject)((Element)accessor).Owner;
	}

	[CanBeNull]
	public static IEvalContext GetEvalContext(this IOptionalMechanicContext _)
	{
		return Context;
	}

	[CanBeNull]
	public static RulebookEvent GetRule(this IOptionalRule _)
	{
		return Context.Rule;
	}

	[CanBeNull]
	public static AbilityData GetAbility(this IOptionalAbility _)
	{
		return Context.Ability;
	}

	[CanBeNull]
	public static Entity GetContextCaster(this IOptionalContextCaster _)
	{
		return Context.Caster;
	}

	[CanBeNull]
	public static MechanicEntity GetCurrentTarget(this IOptionalCurrentTargetEntity _)
	{
		return Context.CurrentTargetEntity;
	}

	[CanBeNull]
	public static Vector3? GetCurrentTargetPosition(this IOptionalCurrentTarget _)
	{
		return Context.Target?.Point;
	}

	[CanBeNull]
	public static float? GetCurrentTargetOrientation(this IOptionalCurrentTarget _)
	{
		return Context.Target?.Orientation;
	}

	[CanBeNull]
	public static MechanicEntity GetContextMainTarget(this IOptionalContextMainTargetEntity _)
	{
		return Context.ClickedTarget?.Entity;
	}

	[CanBeNull]
	public static Vector3? GetContextMainTargetPosition(this IOptionalContextMainTarget _)
	{
		return Context.ClickedTarget?.Point;
	}

	[CanBeNull]
	public static float? GetContextMainTargetOrientation(this IOptionalContextMainTarget _)
	{
		return Context.ClickedTarget?.Orientation;
	}

	[CanBeNull]
	public static Entity GetRuleInitiator(this IOptionalRuleInitiator _)
	{
		return Context.Rule?.Initiator;
	}

	[CanBeNull]
	public static Entity GetRuleTarget(this IOptionalRuleTarget _)
	{
		return Context.Rule?.Target;
	}

	[CanBeNull]
	public static ItemEntityWeapon GetAbilityWeapon(this IOptionalAbilityWeapon _)
	{
		return Context.AbilityWeapon;
	}

	[CanBeNull]
	public static Entity GetTargetByType(this IOptionalTargetByType _, PropertyTargetType type)
	{
		return Context.GetEntityByType(type);
	}

	[CanBeNull]
	public static Vector3? GetTargetPositionByType(this IOptionalTargetByType _, PropertyTargetType type)
	{
		return Context.GetEntityPositionByType(type);
	}

	[CanBeNull]
	public static IntRect? GetTargetRectByType(this IOptionalTargetByType _, PropertyTargetType type)
	{
		return Context.GetEntityRectByType(type);
	}

	[CanBeNull]
	public static MechanicEntityFact GetFact(this IOptionalFact _)
	{
		return Context.Fact;
	}
}
