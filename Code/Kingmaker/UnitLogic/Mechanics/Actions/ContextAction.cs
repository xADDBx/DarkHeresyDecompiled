using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.ElementSystem;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("e45eddecc6b973948aa0f785b8954827")]
public abstract class ContextAction : GameAction
{
	protected EvalContext Context => EvalContext.Current;

	[NotNull]
	protected MechanicEntity Caster => Context.Caster ?? throw new Exception("Caster is missing");

	protected TargetWrapper Target => Context.Target;

	[NotNull]
	protected MechanicEntity TargetEntity => Target.Entity ?? throw new Exception("Target entity is missing");

	[CanBeNull]
	protected AbilityExecutionContext AbilityContext => Context.AbilityExecution;

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
			stringBuilder.Append($"current = {Context.Target?.Entity}\n");
			stringBuilder.Append($"clicked target = {Context.ClickedTarget?.Entity}\n");
			stringBuilder.Append($"caster  = {Context.Caster}\n");
			stringBuilder.Append($"owner   = {Context.Owner}");
			debug.ContextDebugData = new ContextDebugData
			{
				StringData = stringBuilder.ToString()
			};
		}
	}
}
