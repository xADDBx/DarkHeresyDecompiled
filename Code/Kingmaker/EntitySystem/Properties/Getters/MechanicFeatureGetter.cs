using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("12aaa30847dd4bab9ada22601914e152")]
public class MechanicFeatureGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private MechanicsFeatureType m_FeatureType;

	[SerializeField]
	private PropertyTargetType m_Target;

	protected override bool GetBaseValue()
	{
		return ((this.GetTargetByType(m_Target) as BaseUnitEntity) ?? throw new Exception($"MechanicFeatureGetter: can't find suitable target of type {m_Target}")).GetMechanicFeature(m_FeatureType).Count > 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{m_Target.Colorized()} has {m_FeatureType} feature";
	}
}
