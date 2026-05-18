using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("d04a8b4d1fec4545b9e4d90b81ce2498")]
public class HasFactGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		return ((EvalContext.Current.GetEntityByType(Target) as BaseUnitEntity) ?? throw new Exception($"HasFactGetter: can't find suitable target of type {Target}")).Facts.Contains((BlueprintUnitFact)m_Fact);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " has " + m_Fact.NameSafe();
	}
}
