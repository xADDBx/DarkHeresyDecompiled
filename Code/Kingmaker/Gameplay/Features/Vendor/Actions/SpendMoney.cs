using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Vendor.Actions;

[Serializable]
[TypeId("2e6a0af22d4e47948df4141f8c573132")]
public sealed class SpendMoney : GameAction
{
	[SerializeReference]
	public IntEvaluator Amount;

	public override string GetCaption()
	{
		return $"Spend {Amount} money";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.SpendMoney(Amount.GetValue());
	}
}
