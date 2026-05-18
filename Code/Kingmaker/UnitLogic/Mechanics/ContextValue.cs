using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Framework;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextValue
{
	[HideInInspector]
	public ContextValueType ValueType;

	[HideInInspector]
	public int Value;

	[HideInInspector]
	public ContextProperty Property;

	[SerializeField]
	[HideInInspector]
	private BlueprintEntityPropertyReference m_CustomProperty;

	[SerializeField]
	[HideInInspector]
	public ContextPropertyName PropertyName;

	[SerializeField]
	[HideInInspector]
	public float Factor = 1f;

	public PropertyCalculatorBlueprint CustomProperty => m_CustomProperty?.Get();

	public bool IsZero
	{
		get
		{
			if (Value == 0)
			{
				return ValueType == ContextValueType.Const;
			}
			return false;
		}
	}

	public ContextValue()
	{
	}

	public ContextValue([NotNull] ContextValue other)
		: this()
	{
		ValueType = other.ValueType;
		Value = other.Value;
		Property = other.Property;
		m_CustomProperty = other.m_CustomProperty;
		PropertyName = other.PropertyName;
		Factor = other.Factor;
	}

	public int Calculate(IEvalContext context)
	{
		if (ValueType != 0 && context == null)
		{
			PFLog.Default.Error("Context is missing");
			return 0;
		}
		BlueprintScriptableObject blueprint = context?.Blueprint;
		MechanicEntity mechanicEntity = EvalContext.Current.Target?.Entity ?? context?.ClickedTarget?.Entity;
		int num = ValueType switch
		{
			ContextValueType.Const => Value, 
			ContextValueType.CasterProperty => Property.GetValue(context.Caster, context), 
			ContextValueType.TargetProperty => Property.GetValue(mechanicEntity, context), 
			ContextValueType.CasterCustomProperty => CustomProperty.GetValue(context.Caster, context), 
			ContextValueType.TargetCustomProperty => CustomProperty.GetValue(mechanicEntity, context), 
			ContextValueType.CasterNamedProperty => blueprint.CalculateValue(PropertyName, context.Caster, context), 
			ContextValueType.TargetNamedProperty => blueprint.CalculateValue(PropertyName, mechanicEntity, context), 
			ContextValueType.ContextProperty => context[PropertyName], 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (!(Math.Abs(Factor) < 0.001f) && ValueType != 0)
		{
			return Mathf.RoundToInt((float)num * Factor);
		}
		return num;
	}

	public static implicit operator ContextValue(int value)
	{
		return new ContextValue
		{
			ValueType = ContextValueType.Const,
			Value = value
		};
	}

	public override string ToString()
	{
		string text = ValueType switch
		{
			ContextValueType.Const => Value.ToString(), 
			ContextValueType.CasterProperty => $"C.{Property}", 
			ContextValueType.TargetProperty => $"T.{Property}", 
			ContextValueType.CasterCustomProperty => "C." + (SimpleBlueprintExtendAsObject.Or(CustomProperty, null)?.name ?? "<missing-property>"), 
			ContextValueType.TargetCustomProperty => "T." + (SimpleBlueprintExtendAsObject.Or(CustomProperty, null)?.name ?? "<missing-property>"), 
			ContextValueType.CasterNamedProperty => $"C.{PropertyName}", 
			ContextValueType.TargetNamedProperty => $"T.{PropertyName}", 
			ContextValueType.ContextProperty => $"Ctx.{PropertyName}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (ValueType != 0)
		{
			if (!(Math.Abs(Factor - (float)(int)Factor) < 0.01f))
			{
				return $"{text}*{Factor:F2}";
			}
			return $"{text}*{(int)Factor}";
		}
		return text;
	}
}
