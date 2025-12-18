using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Vendor.Actions;

[Serializable]
[TypeId("d70926dbd6d7402b921acfdb2441f67f")]
public sealed class GainMoney : GameAction
{
	[SerializeReference]
	public IntEvaluator Amount;

	public override string GetCaption()
	{
		return $"Gain {Amount} money";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.GainMoney(Amount.GetValue());
	}
}
