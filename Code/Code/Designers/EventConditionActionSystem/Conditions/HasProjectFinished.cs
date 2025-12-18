using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Code.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("bc2d43173eaf4f59950b23969511be3b")]
public class HasProjectFinished : Condition
{
	[SerializeField]
	private BlueprintColonyProjectReference m_Project;

	private BlueprintColonyProject Project => m_Project?.Get();

	protected override string GetConditionCaption()
	{
		return "Has project " + Project?.Name + " finished";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
