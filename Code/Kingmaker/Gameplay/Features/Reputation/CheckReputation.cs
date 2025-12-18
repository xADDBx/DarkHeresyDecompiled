using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Utility.Helpers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Reputation;

[Serializable]
[TypeId("e86e85b76dff41beae511b061b4679d2")]
public sealed class CheckReputation : Condition
{
	public FactionType FactionType;

	public ReputationType ReputationType;

	public ComparisionType Comparision;

	[SerializeReference]
	public IntEvaluator TargetValue;

	protected override string GetConditionCaption()
	{
		return $"{ReputationType} of faction {FactionType} is {Comparision.GetDescription(TargetValue)}";
	}

	protected override bool CheckCondition()
	{
		int num = Game.Instance.Reputation.Get(FactionType, ReputationType);
		int value = TargetValue.GetValue();
		return Comparision.Check(num, value);
	}
}
