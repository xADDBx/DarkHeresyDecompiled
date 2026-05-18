using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.ContextActions;

[Serializable]
[TypeId("a1c5b4265a1b48c9a61f778c490752dd")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Owner,
	ContextField.Target,
	ContextField.ClickedTarget
})]
[SetsContext(ContextField.Caster, Availability.Definitely)]
[SetsContext(ContextField.Owner, Availability.Definitely)]
[SetsContext(ContextField.Target, Availability.Definitely)]
[SetsContext(ContextField.ClickedTarget, Availability.Definitely)]
public sealed class ContextActionSetupContext : ContextAction
{
	public enum TargetType
	{
		Caster,
		Owner,
		Target,
		ClickedTarget
	}

	[SerializeField]
	private TargetType _caster;

	[SerializeField]
	private TargetType _owner = TargetType.Owner;

	[SerializeField]
	private TargetType _target = TargetType.Target;

	[SerializeField]
	private TargetType _clickedTarget = TargetType.ClickedTarget;

	[SerializeField]
	private ActionList _actions = new ActionList();

	public override string GetCaption()
	{
		if (_caster == TargetType.Caster && _owner == TargetType.Owner && _target == TargetType.Target && _clickedTarget == TargetType.ClickedTarget)
		{
			return "Setup context: keep unchanged";
		}
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("Setup context:");
		if (_caster != 0)
		{
			builder.Append($" caster={_caster}");
		}
		if (_owner != TargetType.Owner)
		{
			builder.Append($" owner={_owner}");
		}
		if (_target != TargetType.Target)
		{
			builder.Append($" target={_target}");
		}
		if (_clickedTarget != TargetType.ClickedTarget)
		{
			builder.Append($" clickedTarget={_clickedTarget}");
		}
		return builder.ToString();
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Caster;
		MechanicEntity owner = base.Context.Owner;
		TargetWrapper target = base.Target;
		TargetWrapper clickedTarget = base.Context.ClickedTarget;
		MechanicEntity entity = GetEntity(_caster);
		MechanicEntity entity2 = GetEntity(_owner);
		TargetWrapper targetWrapper = GetTargetWrapper(_target);
		TargetWrapper targetWrapper2 = GetTargetWrapper(_clickedTarget);
		if (entity == caster && entity2 == owner && targetWrapper2 == clickedTarget && targetWrapper != target)
		{
			using (base.Context.PushTarget(targetWrapper))
			{
				_actions.Run();
				return;
			}
		}
		using (EvalContext.Build().Caster(entity).Owner(entity2)
			.Target(targetWrapper)
			.ClickedTarget(targetWrapper2)
			.Push())
		{
			_actions.Run();
		}
	}

	[NotNull]
	private MechanicEntity GetEntity(TargetType targetType)
	{
		return (MechanicEntity)((targetType switch
		{
			TargetType.Caster => base.Caster, 
			TargetType.Owner => base.Context.Owner, 
			TargetType.Target => base.TargetEntity, 
			TargetType.ClickedTarget => base.Context.ClickedTarget?.Entity, 
			_ => throw new ArgumentOutOfRangeException("targetType", targetType, null), 
		}) ?? throw new NullReferenceException($"Entity for {targetType} is null"));
	}

	[NotNull]
	private TargetWrapper GetTargetWrapper(TargetType targetType)
	{
		return (TargetWrapper)((targetType switch
		{
			TargetType.Caster => (TargetWrapper)base.Caster, 
			TargetType.Owner => (TargetWrapper)base.Context.Owner, 
			TargetType.Target => base.Target, 
			TargetType.ClickedTarget => base.Context.ClickedTarget, 
			_ => throw new ArgumentOutOfRangeException("targetType", targetType, null), 
		}) ?? throw new NullReferenceException($"Target for {targetType} is null"));
	}
}
