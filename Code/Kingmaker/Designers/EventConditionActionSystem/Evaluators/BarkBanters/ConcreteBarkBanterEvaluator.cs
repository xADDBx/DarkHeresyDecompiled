using System;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;

[Serializable]
[Obsolete]
[TypeId("ee5c2e43efcc4b179ebdd0fa5c174c28")]
public class ConcreteBarkBanterEvaluator : BarkBanterEvaluator
{
	[SerializeField]
	private BlueprintBarkBanterReference m_BarkBanterReference;

	public override string GetCaption()
	{
		return "Get concrete bark banter";
	}

	protected override BlueprintBarkBanter GetValueInternal()
	{
		return m_BarkBanterReference.Get();
	}
}
