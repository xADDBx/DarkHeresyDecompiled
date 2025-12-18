using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.ElementSystem;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("e45eddecc6b973948aa0f785b8954827")]
public abstract class ContextAction : GameAction
{
	[CanBeNull]
	protected AbilityExecutionContext AbilityContext => Context as AbilityExecutionContext;

	protected MechanicsContext Context => SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current;

	[NotNull]
	protected MechanicEntity Caster => Context.MaybeCaster ?? throw new Exception("Caster is missing");

	protected TargetWrapper Target => SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Current;

	[NotNull]
	protected MechanicEntity TargetEntity => Target.Entity ?? throw new Exception("Target entity is missing");

	[CanBeNull]
	protected Projectile Projectile => ContextData<AbilityExecutionContext.Data>.Current?.Projectile;

	[CanBeNull]
	protected RulePerformAttack AttackRule => ContextData<AbilityExecutionContext.Data>.Current?.AttackRule;

	public virtual bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		return true;
	}

	protected override void SetupDebugContext(ElementsDebugger debug)
	{
		if (ElementsDebugger.IsContextDebugEnabled)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append($"current = {SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Current?.Entity}\n");
			stringBuilder.Append($"clicked target = {SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.ClickedTarget?.Entity}\n");
			stringBuilder.Append($"caster  = {SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.MaybeCaster}\n");
			stringBuilder.Append($"owner   = {SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.MaybeOwner}");
			debug.ContextDebugData = new ContextDebugData
			{
				StringData = stringBuilder.ToString()
			};
		}
	}
}
