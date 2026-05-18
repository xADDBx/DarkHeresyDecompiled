using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("c3a2064e333b431a89c0168ef0ede0ba")]
public class CheckContextBlueprintGetter : BoolPropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintScriptableObjectReference m_Blueprint;

	public BlueprintScriptableObject Blueprint => m_Blueprint;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Context BP is {Blueprint}";
	}

	protected override bool GetBaseValue()
	{
		return EvalContext.Current.Blueprint == Blueprint;
	}
}
