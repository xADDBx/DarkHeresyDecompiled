using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Utility.Helpers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Vendor.Conditions;

[Serializable]
[TypeId("5275f9a50098453bb892268721bfa398")]
public sealed class CheckMoney : Condition
{
	public ComparisionType Comparision;

	[SerializeReference]
	public IntEvaluator TargetValue;

	protected override string GetConditionCaption()
	{
		return "Check money is " + Comparision.GetDescription(TargetValue);
	}

	protected override bool CheckCondition()
	{
		return Comparision.Check(Game.Instance.Player.Money, TargetValue.GetValue());
	}
}
