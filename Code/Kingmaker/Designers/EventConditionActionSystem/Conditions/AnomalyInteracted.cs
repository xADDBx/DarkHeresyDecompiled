using System;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("1e8f415e58564ae4fae2afd4a9cdf522")]
public class AnomalyInteracted : Condition
{
	[SerializeReference]
	public StarSystemObjectOnScene ObjectEvaluator;

	protected override string GetConditionCaption()
	{
		return "Was anomaly on this object interacted";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
