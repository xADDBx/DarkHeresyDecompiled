using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties;

public static class PropertyContextHelper
{
	[CanBeNull]
	public static MechanicEntity GetTargetEntity(this PropertyContext context, PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => context.CurrentEntity, 
			PropertyTargetType.CurrentTarget => context.CurrentTargetEntity, 
			PropertyTargetType.ContextCaster => context.ContextCaster, 
			PropertyTargetType.ContextOwner => context.ContextOwner, 
			PropertyTargetType.ContextMainTarget => context.ContextMainTarget, 
			PropertyTargetType.RuleInitiator => context.RuleInitiator, 
			PropertyTargetType.RuleTarget => context.RuleTarget, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	[CanBeNull]
	public static Vector3? GetTargetPosition(this PropertyContext context, PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => context.CurrentEntity.Position, 
			PropertyTargetType.CurrentTarget => context.CurrentTargetPosition, 
			PropertyTargetType.ContextCaster => context.ContextCaster?.Position, 
			PropertyTargetType.ContextOwner => context.ContextOwner?.Position, 
			PropertyTargetType.ContextMainTarget => context.ContextMainTargetPosition, 
			PropertyTargetType.RuleInitiator => context.RuleInitiator?.Position, 
			PropertyTargetType.RuleTarget => context.RuleTarget?.Position, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	[CanBeNull]
	public static IntRect? GetTargetRectByType(this PropertyContext context, PropertyTargetType type)
	{
		BaseUnitEntity obj = context.GetTargetEntity(type) as BaseUnitEntity;
		if (obj == null)
		{
			if (!context.GetTargetPosition(type).HasValue)
			{
				return null;
			}
			return default(IntRect);
		}
		return obj.SizeRect;
	}

	public static string ToShortString(this PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => "CE", 
			PropertyTargetType.CurrentTarget => "CT", 
			PropertyTargetType.ContextCaster => "CC", 
			PropertyTargetType.ContextOwner => "CO", 
			PropertyTargetType.ContextMainTarget => "CMT", 
			PropertyTargetType.RuleInitiator => "RI", 
			PropertyTargetType.RuleTarget => "RT", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static string Colorized(this PropertyTargetType type)
	{
		if (type == PropertyTargetType.CurrentEntity)
		{
			type = FormulaTargetScope.CurrentTarget;
		}
		if (FormulaTargetScope.NeedColorization)
		{
			return $"<color='green'>{type}</color>";
		}
		return type.ToString();
	}
}
