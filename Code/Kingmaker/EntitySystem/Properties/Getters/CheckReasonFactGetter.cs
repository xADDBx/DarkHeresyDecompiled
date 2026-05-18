using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("02322c486992c2449ab0a19892384b33")]
public class CheckReasonFactGetter : BoolPropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintUnitFactReference m_Blueprint;

	public BlueprintUnitFact Blueprint => m_Blueprint;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Context BP is {Blueprint}";
	}

	protected override bool GetBaseValue()
	{
		return base.Context.Rule?.Reason.Fact?.Blueprint == Blueprint;
	}
}
