using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.ContextActions;

[Serializable]
[TypeId("a1c5b4265a1b48c9a61f778c490752dd")]
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
		MechanicEntity maybeOwner = base.Context.MaybeOwner;
		TargetWrapper target = base.Target;
		TargetWrapper clickedTarget = base.Context.ClickedTarget;
		MechanicEntity entity = GetEntity(_caster);
		MechanicEntity entity2 = GetEntity(_owner);
		TargetWrapper targetWrapper = GetTargetWrapper(_target);
		TargetWrapper targetWrapper2 = GetTargetWrapper(_clickedTarget);
		if (entity == caster && entity2 == maybeOwner && targetWrapper2 == clickedTarget && targetWrapper != target)
		{
			using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(targetWrapper))
			{
				_actions.Run();
				return;
			}
		}
		using MechanicsContext mechanicsContext = MechanicsContext.Claim(base.Context.Blueprint, entity, entity2, base.Context, targetWrapper2, base.Context.Fact, base.Context.Ability);
		using (mechanicsContext.SetScope(targetWrapper))
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
			TargetType.Owner => base.Context.MaybeOwner, 
			TargetType.Target => base.TargetEntity, 
			TargetType.ClickedTarget => base.Context.ClickedTarget.Entity, 
			_ => throw new ArgumentOutOfRangeException("targetType", targetType, null), 
		}) ?? throw new NullReferenceException($"Entity for {targetType} is null"));
	}

	[NotNull]
	private TargetWrapper GetTargetWrapper(TargetType targetType)
	{
		return (TargetWrapper)((targetType switch
		{
			TargetType.Caster => (TargetWrapper)base.Caster, 
			TargetType.Owner => (TargetWrapper)base.Context.MaybeOwner, 
			TargetType.Target => base.Target, 
			TargetType.ClickedTarget => base.Context.ClickedTarget, 
			_ => throw new ArgumentOutOfRangeException("targetType", targetType, null), 
		}) ?? throw new NullReferenceException($"Target for {targetType} is null"));
	}
}
