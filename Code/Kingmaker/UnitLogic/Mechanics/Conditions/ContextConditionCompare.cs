using System;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Gameplay.Utility.Helpers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("c5dd77fc57484bbba4bbc75f37fff18a")]
public class ContextConditionCompare : ContextCondition
{
	[SerializeField]
	private ComparisionType m_Type;

	public ContextValue CheckValue;

	public ContextValue TargetValue;

	protected override string GetConditionCaption()
	{
		return $"{CheckValue} is {m_Type.GetDescription(TargetValue)}";
	}

	protected override bool CheckCondition()
	{
		int num = CheckValue.Calculate(base.Eval);
		int num2 = TargetValue.Calculate(base.Eval);
		return m_Type.Check(num, num2);
	}
}
