using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("a4316c207b21c9b4fa4c5ec4c9300c9b")]
public class CheckBlueprintGetter : BoolPropertyGetter
{
	[SerializeField]
	private BlueprintMechanicEntityFact.Reference m_Blueprint;

	public BlueprintMechanicEntityFact Blueprint => m_Blueprint?.Get();

	protected override bool GetBaseValue()
	{
		return base.CurrentEntity.Blueprint == Blueprint;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return string.Format("{0} BP is {1}", FormulaTargetScope.Current, (Blueprint != null) ? ((object)Blueprint) : ((object)"<null>"));
	}
}
