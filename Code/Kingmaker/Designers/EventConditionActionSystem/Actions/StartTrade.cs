using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Vendor;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("63f4331f2b9d14b4cbdca44a66b1bd43")]
public class StartTrade : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Vendor;

	public override string GetCaption()
	{
		return $"Start Trade {Vendor}";
	}

	protected override void RunAction()
	{
		VendorHelper.TradeLogic.BeginTrading(Vendor?.GetValue());
	}
}
