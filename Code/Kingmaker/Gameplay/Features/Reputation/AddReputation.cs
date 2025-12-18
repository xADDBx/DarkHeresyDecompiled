using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Reputation;

[Serializable]
[TypeId("e18e4eab5ed44941b7b42f42239f5b26")]
public class AddReputation : GameAction
{
	public FactionType FactionType;

	public ReputationType ReputationType;

	[SerializeReference]
	public IntEvaluator Value;

	public override string GetCaption()
	{
		return $"Add {Value} {ReputationType} for {FactionType}";
	}

	protected override void RunAction()
	{
		Game.Instance.Reputation.Add(FactionType, ReputationType, Value.GetValue());
	}
}
